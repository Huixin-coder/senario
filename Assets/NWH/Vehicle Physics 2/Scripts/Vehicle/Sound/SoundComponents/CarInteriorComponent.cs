using System;
using UnityEngine;

namespace NWH.VehiclePhysics2.Sound.SoundComponents
{
    /// <summary>
    ///     CarInteriorSound.
    /// </summary>
    [Serializable]
    public class CarInteriorComponent : SoundComponent
    {

        public float VehicleSpeed;
        private static float MaxExpectedSpeed = 200f;
        private static float MinExpectedSpeed = 10f;

        public override bool GetInitLoop()
        {
            return true;
        }


        public override void Update()
        {

            if (!Active)
            {
                return;
            }

            if (Source != null && Clip != null)
            {
                float VehicleSpeed = NWH.VehiclePhysics2.VehicleGUI.DashGUIController.PassSpeedData();

                

                if (vc.powertrain.engine.IsRunning || vc.powertrain.engine.starterActive)
                {
                    if (VehicleSpeed > 10) //speed you want sound to start kicking in
                    {

                        if (!Source.isPlaying && Source.enabled)
                        {
                            
                            SetPitch(basePitch);
                            SetVolume(baseVolume);
                            Play();
                            
                        }

                        //Slowly increasing the sound volume with increasing speed
                        if (Source.isPlaying && Source.enabled)
                        {
                            //Debug.Log("Base Volume is: " + baseVolume);


                            //Debug.Log("Vehicle Speed: " + VehicleSpeed);
                            baseVolume = (VehicleSpeed - MinExpectedSpeed)/(MaxExpectedSpeed-MinExpectedSpeed);

                            if (baseVolume < 1.0f)
                            {
                                SetVolume(baseVolume);
                                //Debug.Log("Volume set to: " + baseVolume);
                            }
                        }

                    } else
                    {
                        Stop();
                    }

                    

                    
                }
                else if (Source.isPlaying)
                {
                    Stop();
                }

                //SetVolume(0);
                //SetPitch(0);
            }
        }


        public override void FixedUpdate()
        {
        }


        public override void SetDefaults(VehicleController vc)
        {
            base.SetDefaults(vc);
            //baseVolume = 0.1f;
            //basePitch = 0.4f;

            if (Clip == null)
            {
                AddDefaultClip("BlinkerOn");
            }
        }
    }
}