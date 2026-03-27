using System;
using System.Collections.Generic;
using NWH.Common.Input;
using UnityEngine;
using UnityEngine.Serialization;
using UCL_Impearl;
using NWH.VehiclePhysics2.Powertrain;
using NWH.VehiclePhysics2.Powertrain.Wheel;




namespace NWH.VehiclePhysics2.Input
{
    /// <summary>
    ///     Manages vehicle input by retrieving it from the active InputProvider and filling in the InputStates with the
    ///     fetched data.
    /// </summary>
    [Serializable]
    public class VehicleInputHandler : VehicleComponent
    {
        /// <summary>
        ///     When enabled input will be auto-retrieved from the InputProviders present in the scene.
        ///     Disable to manualy set the input through external scripts, i.e. AI controller.
        /// </summary>
        [FormerlySerializedAs("autoSettable")] public bool autoSetInput = false;

        /// <summary>
        ///     Should be set to true when the scene is to start with the Impearl Sim as the Input System
        ///     Ensure that AutoSetInput is set to false if this is set to true
        /// </summary>
        [FormerlySerializedAs("autoSettable")] public bool ImpearlSimulatorIsConnected = true;

        /// <summary>
        ///     Should be set to true when the scene is to start with the AV as the Input System
        ///     Ensure that AutoSetInput is set to false if this is set to true
        /// </summary>
        [FormerlySerializedAs("autoSettable")] public bool AVIsConnected = false;

        /// <summary>
        ///     Should be set to true when the scene is to start with the Impearl Portable Sim (Trakracer) as the Input System
        ///     Ensure that AutoSetInput is set to false if this is set to true
        /// </summary>
        [FormerlySerializedAs("autoSettable")] public bool ImpearlPortableIsConnected = false;

        //jenyang - static of the field above that can be called by methods in other classes without an object instance reference
        public static bool ImpearlPortableIsConnected_static;

        //jenyang variable to store data from logitech wheel - see LogitechGSDK.cs for more info
        public LogitechGSDK.DIJOYSTATE2ENGINES LogitechWheelState = new LogitechGSDK.DIJOYSTATE2ENGINES();
        //LogitechWheelState = new DIJOYSTATE2ENGINES();

        /// <summary>
        ///     All the input states of the vehicle. Can be used to set input through scripting or copy the inputs
        ///     over from other vehicle, such as truck to trailer.
        /// </summary>
        [Tooltip(
            "All the input states of the vehicle. Can be used to set input through scripting or copy the inputs\r\nover from other vehicle, such as truck to trailer.")]
        public VehicleInputStates states;

        /// <summary>
        ///     Swaps throttle and brake axes when vehicle is in reverse.
        /// </summary>
        [Tooltip("    Swaps throttle and brake axes when vehicle is in reverse.")]
        public bool swapInputInReverse = true;

        /// <summary>
        ///     Should steering input be inverted?
        /// </summary>
        [Tooltip("Should steering input be inverted?")]
        public bool invertSteering;

        /// <summary>
        ///     Should throttle input be inverted?
        /// </summary>
        [Tooltip("Should throttle input be inverted?")]
        public bool invertThrottle;

        /// <summary>
        ///     Should brake input be inverted?
        /// </summary>
        [Tooltip("Should brake input be inverted?")]
        public bool invertBrakes;

        /// <summary>
        ///     Should clutch input be inverted?
        /// </summary>
        [Tooltip("Should clutch input be inverted?")]
        public bool invertClutch;

        /// <summary>
        ///     Should handbrake input be inverted?
        /// </summary>
        [Tooltip("Should handbrake input be inverted?")]
        public bool invertHandbrake;

        /// <summary>
        ///     Input with lower value than deadzone will be ignored.
        /// </summary>
        [Range(0.02f, 0.5f)]
        [SerializeField] private float deadzone = 0.03f;

        /// <summary>
        ///     List of scene input providers.
        /// </summary>
        private List<InputProvider> _inputProviders = new List<InputProvider>();

        /// <summary>
        ///     Input deadzone. Value is limited to [0.02f, 0.5f] range for stability reasons
        ///     and setting it to a value out of this range will result in it getting clamped.
        /// </summary>
        public float Deadzone
        {
            get { return deadzone; }
            set { deadzone = value < 0.02f ? 0.02f : value > 0.5f ? 0.5f : value; }
        }

        /// <summary>
        ///     Convenience function for setting throttle/brakes as a single value.
        ///     Use Throttle/Brake axes to apply throttle and braking separately.
        ///     If the set value is larger than 0 throttle will be set, else if value is less than 0 brake axis will be set.
        /// </summary>
        public float Vertical
        {
            get { return states.throttle - states.brakes; }
            set
            {
                float clampedValue = value < -1 ? -1 : value > 1 ? 1 : value;
                if (value > 0)
                {
                    states.throttle = clampedValue;
                    states.brakes   = 0;
                }
                else
                {
                    states.throttle = 0;
                    states.brakes   = -clampedValue;
                }
            }
        }

        /// <summary>
        ///     Throttle axis.
        ///     For combined throttle/brake input (such as prior to v1.0.1) use 'Vertical' instead.
        /// </summary>
        public float Throttle
        {
            get { return states.throttle; }
            set
            {
                value           = value < 0f ? 0f : value > 1f ? 1f : value;
                value           = invertThrottle ? 1f - value : value;
                states.throttle = value;
            }
        }

        /// <summary>
        ///     Brake axis.
        ///     For combined throttle/brake input use 'Vertical' instead.
        /// </summary>
        public float Brakes
        {
            get { return states.brakes; }
            set
            {
                value         = value < 0f ? 0f : value > 1f ? 1f : value;
                value         = invertBrakes ? 1f - value : value;
                states.brakes = value;
            }
        }

        /// <summary>
        ///     Returns throttle or brake input based on 'swapInputInReverse' setting and current gear.
        ///     If swapInputInReverse is true, brake will act as throttle and vice versa while driving in reverse.
        /// </summary>
        public float InputSwappedThrottle
        {
            get { return IsInputSwapped ? Brakes : Throttle; }
        }

        /// <summary>
        ///     Returns throttle or brake input based on 'swapInputInReverse' setting and current gear.
        ///     If swapInputInReverse is true, throttle will act as brake and vise versa while driving in reverse.
        /// </summary>
        public float InputSwappedBrakes
        {
            get { return IsInputSwapped ? Throttle : Brakes; }
        }

        /// <summary>
        ///     Steering axis.
        /// </summary>
        public float Steering
        {
            get { return states.steering; }
            set
            {
                value           = value < -1f ? -1f : value > 1f ? 1f : value;
                value           = invertSteering ? -value : value;
                states.steering = value;
            }
        }

        /// <summary>
        ///     Clutch axis.
        /// </summary>
        public float Clutch
        {
            get { return states.clutch; }
            set
            {
                value         = value < 0f ? 0f : value > 1f ? 1f : value;
                value         = invertClutch ? 1f - value : value;
                states.clutch = value;
            }
        }

        public bool EngineStartStop
        {
            get { return states.engineStartStop; }
            set { states.engineStartStop = value; }
        }

        public bool ExtraLights
        {
            get { return states.extraLights; }
            set { states.extraLights = value; }
        }

        public bool HighBeamLights
        {
            get { return states.highBeamLights; }
            set { states.highBeamLights = value; }
        }

        public float Handbrake
        {
            get { return states.handbrake; }
            set
            {
                value            = value < 0f ? 0f : value > 1f ? 1f : value;
                value            = invertHandbrake ? 1f - value : value;
                states.handbrake = value;
            }
        }

        public bool HazardLights
        {
            get { return states.hazardLights; }
            set { states.hazardLights = value; }
        }

        public bool Horn
        {
            get { return states.horn; }
            set { states.horn = value; }
        }

        public bool LeftBlinker
        {
            get { return states.leftBlinker; }
            set { states.leftBlinker = value; }
        }

        public bool LowBeamLights
        {
            get { return states.lowBeamLights; }
            set { states.lowBeamLights = value; }
        }

        public bool RightBlinker
        {
            get { return states.rightBlinker; }
            set { states.rightBlinker = value; }
        }

        public bool ShiftDown
        {
            get { return states.shiftDown; }
            set { states.shiftDown = value; }
        }

        public int ShiftInto
        {
            get { return states.shiftInto; }
            set { states.shiftInto = value; }
        }

        public bool ShiftUp
        {
            get { return states.shiftUp; }
            set { states.shiftUp = value; }
        }

        public bool TrailerAttachDetach
        {
            get { return states.trailerAttachDetach; }
            set { states.trailerAttachDetach = value; }
        }

        public bool CruiseControl
        {
            get { return states.cruiseControl; }
            set { states.cruiseControl = value; }
        }

        public bool Boost
        {
            get { return states.boost; }
            set { states.boost = value; }
        }

        public bool FlipOver
        {
            get { return states.flipOver; }
            set { states.flipOver = value; }
        }
        
        /// <summary>
        ///     True when throttle and brake axis are swapped.
        /// </summary>
        public bool IsInputSwapped
        {
            get { return swapInputInReverse && vc.powertrain.transmission.IsInReverse; }
        }

        /// <summary>
        ///     Should be the GameObject with UDP Send and Receive Attached
        /// </summary>
        private UDPReceive UdpReceive;
        private UDPSend UdpSend;
        // private AutonomousVehicle AutonomousVehicle;
        // private AVController AVController;
        // private WaypointProgressTracker WaypointProgressTracker;
        // private GameObject TimeElapsed;
        // private HMICases HMICases;
        private TriggerEyeTracking TET;
        private float ThrottleThreshold = 0;
        private float BrakesThreshold = 0;
        private float SteeringThreshold = 0;
        private bool AVwasConnected = false;
        private float SteeringWheelPosition = 0.0f;
        private float MaxSteeringWheelAngle = 540f * Mathf.PI / 180;


        public override void Initialize()
        {
            _inputProviders = InputProvider.Instances;

            if (autoSetInput && (_inputProviders == null || _inputProviders.Count == 0))
            {
                Debug.LogWarning(
                    "No InputProviders are present in the scene. " +
                    "Make sure that one or more InputProviders are present (DesktopInputProvider, MobileInputProvider, etc.).");
            }

            states.Reset(); // Reset states to make sure that initial values are neutral in case the behaviour was copied or similar.
            
            if (ImpearlSimulatorIsConnected){
                // UDPReceive[] UdpReceives = this.vc.transform.gameObject.GetComponents<UDPReceive>();
                UDPReceive[] UdpReceives = GameObject.FindGameObjectWithTag("UDPManager").GetComponents<UDPReceive>();
                foreach (UDPReceive thisUDP in UdpReceives) {
                    if (thisUDP.TargetConnection == UDPReceive.UDPTarget.CarSimulator) {
                        UdpReceive = thisUDP;
                    }
                }
                // UCL_Impearl.UDPSend[] UDPScripts = this.vc.transform.gameObject.GetComponents<UCL_Impearl.UDPSend>();
                UCL_Impearl.UDPSend[] UDPScripts = GameObject.FindGameObjectWithTag("UDPManager").GetComponents<UCL_Impearl.UDPSend>();
                foreach (UCL_Impearl.UDPSend UDPScript in UDPScripts) {
                    if (UDPScript.TargetConnection == UCL_Impearl.UDPSend.UDPTarget.CarSimulator) {
                        UdpSend = UDPScript;
                    }
                }
                if(UdpSend == null || UdpReceive == null){
                    throw new Exception("The Vehicle GameObject needs to have a UDP Receive and Send Script attached");
                }
                ThrottleThreshold = BrakesThreshold = 0.1f;
                SteeringThreshold = 0.01f;
            }

            // AutonomousVehicle = this.vc.transform.gameObject.GetComponent<AutonomousVehicle>();
            // AVController = this.vc.transform.gameObject.GetComponent<AVController>();
            // HMICases = this.vc.transform.gameObject.GetComponent<HMICases>();
            // WaypointProgressTracker = this.vc.transform.gameObject.GetComponent<WaypointProgressTracker>();
            // TimeElapsed = GameObject.FindGameObjectWithTag("SceneManager").transform.GetChild(0).transform.GetChild(4).gameObject;
            TET = GameObject.FindGameObjectWithTag("SceneManager").GetComponent<TriggerEyeTracking>();
            // if (AVIsConnected){
            //     if(AVController == null || WaypointProgressTracker == null || AutonomousVehicle == null){
            //         throw new Exception("The Vehicle GameObject needs to have an AutonomousVehicle, AVController and WaypointProgressTracker Component attached.");
            //     }

            //     AutonomousVehicle.enabled = true;
            //     AVController.enabled = true;
            //     WaypointProgressTracker.enabled = true;

            // } else {
            //     AutonomousVehicle.enabled = false;
            //     AVController.enabled = false;
            //     WaypointProgressTracker.enabled = false;

            // }

            if (ImpearlPortableIsConnected)
            {
                UDPReceive[] UdpReceives = this.vc.transform.gameObject.GetComponents<UDPReceive>();
                foreach (UDPReceive thisUDP in UdpReceives) {
                    if (thisUDP.TargetConnection == UDPReceive.UDPTarget.CarSimulator) {
                        UdpReceive = thisUDP;
                    }
                }
                UCL_Impearl.UDPSend[] UDPScripts = this.vc.transform.gameObject.GetComponents<UCL_Impearl.UDPSend>();
                foreach (UCL_Impearl.UDPSend UDPScript in UDPScripts) {
                    if (UDPScript.TargetConnection == UCL_Impearl.UDPSend.UDPTarget.CarSimulator) {
                        UdpSend = UDPScript;
                    }
                }
                Debug.Log("ImpearlPortable (Trakracer setup) is Connected!!!");
                //Change the static variable
                ImpearlPortableIsConnected_static = true;
                if (UdpSend == null || UdpReceive == null)
                {
                    throw new Exception("The Vehicle GameObject needs to have a UDP Receive and Send Script attached");
                }
                ThrottleThreshold = BrakesThreshold = SteeringThreshold = 0.1f;
            }

            base.Initialize();
        }


        public override void Update()
        {
            if (!Active)
            {
                return;
            }
 
            if (!autoSetInput)
            {
                return;
            }

            Throttle = InputProvider.CombinedInput<VehicleInputProviderBase>(i => i.Throttle());

            // Debug.Log("Throttle is: " + Throttle);

            Brakes   = InputProvider.CombinedInput<VehicleInputProviderBase>(i => i.Brakes());
            
            // Debug.Log("Brakes is:" + Brakes);


            Steering  = InputProvider.CombinedInput<VehicleInputProviderBase>(i => i.Steering());

            // if (UnityEngine.Input.GetKeyDown(KeyCode.F1)) {
            //     // toggle autonomous behaviour with F1
            //     AVIsConnected = !AVIsConnected;
            //     HMICases.brakeEventWithVisuals = false;
            // }
            
            if (ImpearlSimulatorIsConnected){
                Throttle =  ConvertRange(0.77f, 2.12f, 1f, 0f, UdpReceive.GetParameter("CAN_VIn1").GetValue());
                Brakes   =  ConvertRange(3.04f, 3.45f, 0f, 1f, UdpReceive.GetParameter("CAN_VIn2").GetValue());
                Steering =  ConvertRange(-MaxSteeringWheelAngle, MaxSteeringWheelAngle, -1f, 1f, UdpReceive.GetParameter("aSteer").GetValue());
                // Debug.Log(Throttle + " " + Brakes + " " + Steering);
            }

            // if (AVIsConnected && !AVwasConnected) {
            //         AVwasConnected = true;
            //         GameObject.FindWithTag("AudioManager").GetComponent<AudioManager>().Play("autonomousModeEnabled");
            //         SteeringWheelPosition = Steering;
            // }

            // if ((AVIsConnected) && (Throttle > ThrottleThreshold || Brakes > BrakesThreshold || Mathf.Abs(Mathf.Abs(Steering) - Mathf.Abs(SteeringWheelPosition)) > SteeringThreshold)){
            //     AVIsConnected = false;
            //     TimeElapsed.GetComponent<TimeElapsed>().enabled = true;
            //     TET.SendEventToEyeTracker("ManualMode");
            //     // UnityEngine.Debug.Log(Throttle + " " + ThrottleThreshold + " " + Brakes + " " + BrakesThreshold + " " + Mathf.Abs(Mathf.Abs(Steering) - Mathf.Abs(SteeringWheelPosition)) + " " + SteeringThreshold);
            // }
                
            // if (InputProvider.CombinedInput<VehicleInputProviderBase>(i => i.AVControl()))
            // {
            //     AVIsConnected = true;
            // }

            Clutch    = InputProvider.CombinedInput<VehicleInputProviderBase>(i => i.Clutch());
            Handbrake = InputProvider.CombinedInput<VehicleInputProviderBase>(i => i.Handbrake());
            ShiftInto = CombinedInputGear<VehicleInputProviderBase>(i => i.ShiftInto());

            ShiftUp   |= InputProvider.CombinedInput<VehicleInputProviderBase>(i => i.ShiftUp());
            ShiftDown |= InputProvider.CombinedInput<VehicleInputProviderBase>(i => i.ShiftDown());

            LeftBlinker    |= InputProvider.CombinedInput<VehicleInputProviderBase>(i => i.LeftBlinker());
            RightBlinker   |= InputProvider.CombinedInput<VehicleInputProviderBase>(i => i.RightBlinker());
            LowBeamLights  |= InputProvider.CombinedInput<VehicleInputProviderBase>(i => i.LowBeamLights());
            HighBeamLights |= InputProvider.CombinedInput<VehicleInputProviderBase>(i => i.HighBeamLights());
            HazardLights   |= InputProvider.CombinedInput<VehicleInputProviderBase>(i => i.HazardLights());
            ExtraLights    |= InputProvider.CombinedInput<VehicleInputProviderBase>(i => i.ExtraLights());

            Horn            =  InputProvider.CombinedInput<VehicleInputProviderBase>(i => i.Horn());
            EngineStartStop |= InputProvider.CombinedInput<VehicleInputProviderBase>(i => i.EngineStartStop());

            Boost = InputProvider.CombinedInput<VehicleInputProviderBase>(i => i.Boost());
            TrailerAttachDetach = TrailerAttachDetach ||
                                  InputProvider.CombinedInput<VehicleInputProviderBase>(i => i.TrailerAttachDetach());
            CruiseControl |= InputProvider.CombinedInput<VehicleInputProviderBase>(i => i.CruiseControl());
            FlipOver      =  FlipOver || InputProvider.CombinedInput<VehicleInputProviderBase>(i => i.FlipOver());


            // if (AVIsConnected){
            //     if (AutonomousVehicle.enabled == false) {
            //         AutonomousVehicle.enabled = true;
            //     }
            //     if (AVController.enabled == false) {
            //         AVController.enabled = true;
            //     }
            //     if (WaypointProgressTracker.enabled == false) {
            //         WaypointProgressTracker.enabled = true;
            //     }
            //     Throttle =  AVController.Throttle;
            //     Brakes   =  AVController.Brakes;
            //     // Debug.Log("VehicleInputHandler " + Throttle + " - " + Brakes);
            //     Steering =  AVController.SteeringAngle;
            // }

            // else {
            //     if (AVwasConnected) {
            //         AVwasConnected = false;
            //         GameObject.FindWithTag("AudioManager").GetComponent<AudioManager>().StopPlaying("attentionRequired");
            //         GameObject.FindWithTag("AudioManager").GetComponent<AudioManager>().Play("autonomousModeDisabled");
            //     }
            // }
            
            //jenyang
            //Taking input from logitech steering wheel if Trakracer is connected
            if (ImpearlPortableIsConnected)
            {
                Debug.Log("Impearl Portable is Connected yo guys!");

                //Code for Trakracer Logitech Steering Wheel

                if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
                {
                    LogitechGSDK.LogiStopSpringForce(0);
                    Debug.Log("Yay we can sense force input from Logitech");
                    //LogitechGSDK.DIJOYSTATE2ENGINES LogitechWheelState;
                    LogitechWheelState = LogitechGSDK.LogiGetStateUnity(0);
                    Throttle = MapThrottle(LogitechWheelState.lY);
                    Brakes = MapBrakes(LogitechWheelState.lRz);
                    Steering = MapSteering(LogitechWheelState.lX); 
                    //Debug.Log(LogitechWheelState.lRz); //Brakes is lRz, varies from 32767 to -32768
                    //Debug.Log(LogitechWheelState.lY); //Throttle Pedal is lY, varies from 32767 to -32768
                    //Debug.Log(LogitechWheelState.lX); //Steering Wheel is lX, neutral is 0, left is negative, right is positive


                }
                else
                {
                    LogitechGSDK.LogiPlaySpringForce(0, 50, 50, 50);
                    Debug.Log("I'm so sorry it doesn't work");
                }

            } 
            //end of jenyang's additions         
            
        }
        
        public static float ConvertRange(
        float originalStart, float originalEnd, // original range
        float newStart, float newEnd, // desired range
        float value) // value to convert
        {
            double scale = (double)(newEnd - newStart) / (originalEnd - originalStart);
            return (float)(newStart + ((value - originalStart) * scale));
        }

        //Jenyang - Defining some helper functions for later
        public static int MapThrottle(int WheelData)
        {
            if (WheelData > 32000) { return 0; }
            else { return 1; }
        }

        public static int MapBrakes(int WheelData)
        {
            if (WheelData > 32000) { return 0; }
            else { return 1; }
        }

        public static float MapSteering(int WheelData)
        {
            int SteeringLeftBound = -10000;
            int SteeringRightBound = 10000;
            int NewLeftBound = -1;
            int NewRightBound = 1;
            float ScalingFactor = (float)(NewRightBound - NewLeftBound) / (SteeringRightBound - SteeringLeftBound);
            return (float)(NewRightBound + ((WheelData - SteeringRightBound) * ScalingFactor));
            //if (WheelData > 50) { return 1; }
            //else if(WheelData < -50) { return -1; }
            //else { return 0; }
        }

        //end of Jenyang's additional code

        public override void FixedUpdate()
        {
        }


        public override void Disable()
        {
            base.Disable();
            states.Reset();
        }


        public static int CombinedInputGear<T>(Func<T, int> selector) where T : InputProvider
        {
            int gear = -999;
            foreach (InputProvider ip in InputProvider.Instances)
            {
                if (ip is T)
                {
                    int tmp = selector(ip as T);
                    if (tmp > gear)
                    {
                        gear = tmp;
                    }
                }
            }

            return gear;
        }


        public void ResetShiftFlags()
        {
            states.shiftUp   = false;
            states.shiftDown = false;
            states.shiftInto = -999;
        }


        private delegate bool BinaryInputDelegate();

        //jenyang
        //This static method is called by DashGUIController to decide whether to display dashboard
        public static bool CheckForImpearlPortable()
        {
            if (ImpearlPortableIsConnected_static)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //jenyang

    }
}