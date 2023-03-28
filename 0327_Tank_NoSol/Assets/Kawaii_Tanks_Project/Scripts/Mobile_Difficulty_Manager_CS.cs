using System.Collections;
using UnityEngine;


namespace ChobiAssets.KTP
{

    public class Mobile_Difficulty_Manager_CS : MonoBehaviour
    {
        /*
         * This script is attached to "Game_Controller" in the scene.
         * This script is used for adjusting the difficulty of the scene for playing on mobile platforms.
        */

        [Header("Enemy settings on mobile devices.")]
        [Tooltip("Multiplier for enemy's Durability.")] public float durabilityMultiplier = 0.75f;
        [Tooltip("Multiplier for enemy's Maximum Speed.")] public float speedMultiplier = 1.0f;
        [Tooltip("Multiplier for enemy's Attack Force.")] public float attackMultiplier = 0.75f;


        void Awake()
        { // This function must be called in "Awake()" before the tanks are spawned by "Spawner_CS" in the Start().
#if UNITY_ANDROID || UNITY_IPHONE
            Adjust_Difficulty();
#endif
            Destroy(this);
        }


        void Adjust_Difficulty()
        {
            // Overwrite the variables of "Spawner_CS" scripts in the scene.
            var spawnerScripts = FindObjectsOfType<Spawner_CS>();
            for (int i = 0; i < spawnerScripts.Length; i++)
            {
                if (spawnerScripts[i].relationship == 1)
                { // Enemy
                    spawnerScripts[i].durability *= durabilityMultiplier;
                    spawnerScripts[i].maxSpeed *= speedMultiplier;
                    spawnerScripts[i].attackForce *= attackMultiplier;
                }
            }
        }

    }

}
