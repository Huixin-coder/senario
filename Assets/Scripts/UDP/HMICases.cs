namespace UCL_Impearl
{
    using UnityEngine;

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Xml;
    using System.Data.Common;
    // using System.Diagnostics.Eventing.Reader;

    public class HMICases : MonoBehaviour
    {
        public enum HMICase
        {
            AVAvailable,                // AV is available. The user can now press the button on the screen, which will activate the AV
            AVActive,                   // AV is active.
            TakeOver,                   // The user must take over immediately
            ManualMode,                 // The user is driving in manual mode
            HMIBlack,
            HMIOff,                     // HMI is not showing, program is dormant in the background and can be activated
        }
        UDPSend[] UdpScripts;
        UDPSend UdpSend;
        UDPReceive[] UdpReceives;
        UDPReceive UdpReceive;
        public HMICase Case = HMICase.ManualMode;
        [HideInInspector] 
        public bool brakeEventWithVisuals = false;
        public bool HMIOverlayOff = false;

        private GameObject UserVehicle;
        private CameraBlanker CameraBlanker;

        public void Start() {
            // For Debug
            Case = HMICase.AVAvailable;
            UdpScripts = GameObject.FindGameObjectWithTag("UDPManager").GetComponents<UDPSend>();
            foreach (UDPSend UdpScript in UdpScripts) {
                if (UdpScript.TargetConnection == UDPSend.UDPTarget.Injection) {
                    UdpSend = UdpScript;
                }
            }
            UdpReceives = GameObject.FindGameObjectWithTag("UDPManager").GetComponents<UDPReceive>();
            foreach (UDPReceive thisUDP in UdpReceives) {
                if (thisUDP.TargetConnection == UDPReceive.UDPTarget.Injection) {
                    UdpReceive = thisUDP;
                }
            }
            UserVehicle = GameObject.FindGameObjectWithTag("UserVehicle");
            CameraBlanker = GameObject.FindGameObjectWithTag("SceneManager").GetComponent<CameraBlanker>();
            // UdpReceive = GameObject.FindGameObjectWithTag("UserVehicle").GetComponents<UDPReceive>()[1];

            // For practice and baseline drive
            // Case = HMICase.HMIBlack;
        }

        public void Update() {
            if (Input.GetKeyDown(KeyCode.U)) // Press 'U' to toggle
            {
                HMIOverlayOff = !HMIOverlayOff; // Toggle HMIOverlay 
            }


            // if car is in autonomous mode
            if (UserVehicle.GetComponent<NWH.VehiclePhysics2.VehicleController>().input.AVIsConnected) {
                // if brake event is on, the user shall take over
                if (brakeEventWithVisuals) {
                    Case = HMICase.TakeOver;
                }
                else {
                    Case = HMICase.AVActive;
                }
            }
            // else car must be in manual mode
            else {
                // if AV is available or a button is pressed on the keyboard, signalise AV as available
                if (Case == HMICase.AVAvailable || Input.GetAxis("AVavailable") == 1.0f) {
                    Case = HMICase.AVAvailable;
                    if (UdpReceive.GetParameter("HMIenableAV").GetValue() == 1) {
                        // User has pressed button
                        UdpReceive.GetParameter("HMIenableAV").SetValue(0);
                        // Activate Autonomous Mode
                        Case = HMICase.AVActive;
                        this.gameObject.GetComponent<NWH.VehiclePhysics2.VehicleController>().input.AVIsConnected = true;
                    }
                }
                else {
                    Case = HMICase.ManualMode;
                }
            }

            // if screen is blanked, HMI should be blanked
            if ((Case == HMICase.AVActive) && (CameraBlanker.isBlanked)) 
            {
                Case = HMICase.HMIBlack;
            }
            // if car is in automated mode and the HMI is unlocked, turn the overlay off
            else if ((Case == HMICase.AVActive) && (HMIOverlayOff)) {
                Case = HMICase.HMIOff;
            }

            // send data to Injection
            if (UdpSend){
                UdpSend.UpdateParameterValue("HMI", (int)Case+1);
            }
            
        }      

    }
}