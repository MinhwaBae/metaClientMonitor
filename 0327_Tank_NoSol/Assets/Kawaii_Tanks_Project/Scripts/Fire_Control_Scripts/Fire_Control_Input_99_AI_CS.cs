using UnityEngine;


namespace ChobiAssets.KTP
{

    public class Fire_Control_Input_99_AI_CS : Fire_Control_Input_00_Base_CS
    {

        Turret_Control_CS turretScript;
        Cannon_Control_CS cannonScript;
        float obstacleCount;
        float waitingCount;
        float aimingCount;


        public override void Prepare(Fire_Control_CS fireControlScript)
        {
            this.fireControlScript = fireControlScript;

            turretScript = GetComponentInParent<Turret_Control_CS>();
            cannonScript = GetComponent<Cannon_Control_CS>();
        }


        public override void Get_Input()
        {
            if (fireControlScript.aiScript.detectFlag == false)
            { // The AI does not detect the target.
                return;
            }

            // Check the turret and the cannon are ready to fire.
            if (turretScript.isReady && cannonScript.isReady)
            { // The turret and the cannon are ready to fire.
              // Check the "Fire_Spawn_CS" can aim the target.
                if (fireControlScript.fireSpawnScript.canAim == false)
                { // The "Fire_Spawn_CS" cannot aim the target.
                    // Change the aiming offset.
                    obstacleCount += Time.deltaTime;
                    if (obstacleCount > 1.0f)
                    {
                        obstacleCount = 0.0f;
                        fireControlScript.aimingControlScript.AI_Random_Offset();
                    }
                    return;
                }
                // The "Fire_Spawn_CS" can aim the target.
                obstacleCount = 0.0f;

                // Count the aiming time.
                aimingCount += Time.deltaTime;
                if (aimingCount > 0.2f)
                {
                    // Fire.
                    fireControlScript.Fire();
                    aimingCount = 0.0f;
                    fireControlScript.aimingControlScript.AI_Random_Offset();
                }
            }
            else
            { // The turret and the cannon are not ready to fire.
                aimingCount = 0.0f;

                // Count the waiting time.
                waitingCount += Time.deltaTime;
                if (waitingCount > 2.0f)
                { // The target might be in the dead angle.
                    // Change the aiming offset.
                    waitingCount = 0.0f;
                    fireControlScript.aimingControlScript.AI_Random_Offset();
                }
            }

        }

    }

}