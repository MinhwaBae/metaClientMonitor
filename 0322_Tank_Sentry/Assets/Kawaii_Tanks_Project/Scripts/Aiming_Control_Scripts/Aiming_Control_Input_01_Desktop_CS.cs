using UnityEngine;


namespace ChobiAssets.KTP
{

    public class Aiming_Control_Input_01_Desktop_CS : Aiming_Control_Input_00_Base_CS
    {
#if !UNITY_ANDROID && !UNITY_IPHONE


        public override void Prepare(Aiming_Control_CS aimingScript)
        {
            this.aimingScript = aimingScript;

            // Set the "useAutoTurn".
            aimingScript.useAutoTurn = true;

            // Set the initial aiming mode.
            aimingScript.mode = 1; // Free aiming.
            aimingScript.Switch_Mode();
        }


        public override void Get_Input()
        {
            // Switch the aiming mode.
            if (Key_Bindings_CS.IsSwitchAimingModeKeyDown())
            {
                if (aimingScript.mode == 0 || aimingScript.mode == 2)
                {
                    aimingScript.mode = 1; // Free aiming.
                }
                else
                {
                    aimingScript.mode = 0; // Keep the initial positon.
                }
                aimingScript.Switch_Mode();
            }


            // Adjust aiming.
            if (Key_Bindings_CS.IsAimingKeyPressing())
            { // The gun camera is enabled now.

                // Set the adjust angle.
                aimingScript.adjustAngle += Key_Bindings_CS.GetAimingAxis() * General_Settings_CS.aimingSensibility;

                // Check it is locking-on now.
                if (aimingScript.targetTransform == null)
                { // Now not locking-on.
                    // Try to find a new target.
                    screenCenter.x = Screen.width * 0.5f;
                    screenCenter.y = Screen.height * 0.5f;
                    aimingScript.Reticle_Aiming(screenCenter);
                }
            }
            else
            { // The gun camera is disabled now.

                // Reset the adjust angle.
                aimingScript.adjustAngle = Vector3.zero;

                // Free aiming.
                if (aimingScript.mode == 1)
                { // Free aiming.

                    // Find the target.
                    screenCenter.x = Screen.width * 0.5f;
                    screenCenter.y = Screen.height * 0.5f;
                    aimingScript.Cast_Ray_Free(screenCenter);
                }
            }
            
            /*
            // Front lock on.
            if (Key_Bindings_CS.IsLockOnFrontKeyDown())
            {
                aimingScript.Auto_Lock(2);
                return;
            }

            // Left lock on.
            if (Key_Bindings_CS.IsLockOnLeftKeyDown())
            {
                aimingScript.Auto_Lock(0);
                return;
            }

            // Right lock on.
            if (Key_Bindings_CS.IsLockOnRightKeyDown())
            {
                aimingScript.Auto_Lock(1);
                return;
            }
            */
            
        }
#endif
    }

}