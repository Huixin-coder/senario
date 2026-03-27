using System;
using UnityEngine;

namespace NWH.VehiclePhysics2.Sound.SoundComponents
{
    /// <summary>
    ///     CarRadioSound.
    /// </summary>
    [Serializable]
    public class CarRadioComponent : SoundComponent
    {
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
                if (vc.powertrain.engine.IsRunning || vc.powertrain.engine.starterActive)
                {
                    if (!Source.isPlaying && Source.enabled)
                    {                       
                        SetPitch(basePitch);
                        SetVolume(baseVolume);
                        Play();
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
            //baseVolume = 0.2f;
            //basePitch = 0.4f;

            if (Clip == null)
            {
                AddDefaultClip("BlinkerOn");
            }
        }
    }
}