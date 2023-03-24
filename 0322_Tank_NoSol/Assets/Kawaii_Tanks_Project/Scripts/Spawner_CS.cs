using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace ChobiAssets.KTP
{

    public class Spawner_CS : MonoBehaviour
    {
        /* 
		 * This script instantiates the specified tank prefab, and overwrites the settings.
		 * When the tank is destroyed, this script respawns the tank automatically.
		*/

        [Header("Spawning settings")]
        [Tooltip("Prefab of the tank.")] public GameObject tankPrefab;
        [Tooltip("Interval for respawning.")] public float interval = 10.0f;

        [Header("Tank settings")]
        [Tooltip("Player or NPC.")] public bool isPlayer = true;
        [Tooltip("Relationship (0 = Friend, 1 = Enemy)"), Range(0, 1)] public int relationship = 0;
        [Tooltip("Durability of this tank.")] public float durability = 100.0f;
        [Tooltip("Maximum Speed (Meter per Second)")] public float maxSpeed = 7.0f;
        [Tooltip("Attack force of the bullet.")] public float attackForce = 100.0f;
        [Tooltip("Multiplier for the reloading time.")] public float reloadTimeMultiplier = 1.0f;
        [Tooltip("Kill-Count for this tank.")] public int cost = 1;

        [Header("AI settings")]
        [Tooltip("Set 'Waypoint_Pack' in the scene.")] public GameObject waypointPack;
        [Tooltip("Allowable range of each waypoint.")] public float waypointRadius = 5.0f;
        [Tooltip("0 = Order, 1 = Random"), Range(0, 1)] public int patrolType = 1; // 0=Order, 1=Random.
        [Tooltip("Do not attack anything.")] public bool noAttack = false;
        [Tooltip("Do not chase the target.")] public bool breakthrough = false;
        [Tooltip("Visibility radius of the AI.")] public float visibilityRadius = 512.0f;
        [Tooltip("Angle range from the rear for the blind spot.")] public float deadAngle = 30.0f;
        [Tooltip("The AI approaches the target until this distance.")] public float approachDistance = 64.0f;
        [Tooltip("The time period for giving up chasing the target.")] public float lostCount = 30.0f;


        GameObject currentTank;
        bool isRespawning;


        void Start()
        {
            // Send this reference to the "Score_Manage_CS" in the scene.
            if (Score_Manager_CS.instance)
            {
                Score_Manager_CS.instance.Get_Spawner(this);
            }

            // Spawn the first tank.
            StartCoroutine("Respawn", 0.0f);
        }


        void Update()
        {
            if (isRespawning)
            { // The tank is respawning now.
                return;
            }

            // Check the tank is living.
            if (currentTank.tag == "Finish")
            { // The tank is dead.

                // Respawn the tank after the interval.
                StartCoroutine("Respawn", interval);

                // Call "Respawning_Circle_CS" in the scene.
                if (isPlayer && Respawning_Circle_CS.instance)
                {
                    Respawning_Circle_CS.instance.StartCoroutine("Respawn", interval);
                }
            }
        }


        IEnumerator Respawn(float interval)
        {
            isRespawning = true;

            // Wait.
            yield return new WaitForSeconds(interval);

            // Check the mission is going on.
            if (this.enabled == false)
            { // Mission is finished. (Disabled by "Score_Manager_CS".)
                yield break;
            }

            // Check the spawning point.
            while (Detect_Tank() == true)
            { // There is any tank in the spawning point.
                // Wait a second.
                yield return new WaitForSeconds(1.0f);
                yield return null;
            }
            // There is no tank in the spawning point.

            // Remove the old one from the scene.
            if (currentTank)
            {
                Destroy(currentTank);
            }

            // Spawn new tank.
            currentTank = Instantiate(tankPrefab, transform.position, transform.rotation) as GameObject;

            // Overwrite the settings.
            Overwrite_Settings();

            isRespawning = false;
        }


        bool Detect_Tank()
        {
            // Check the tank around the spawning point.
            var colliders = Physics.OverlapSphere(transform.position, 5.0f, Layer_Settings_CS.Detect_Body_Layer_Mask);
            for(int i = 0; i < colliders.Length; i++)
            {
                if (currentTank && colliders[i].transform.root.gameObject == currentTank)
                { // The tank was spawned by this spawner, and should be destroyed now.
                    continue;
                }
                // There is any tank in the spawning point.
                return true;
            }
            // There is no tank in the spawning point.
            return false;
        }


        void Overwrite_Settings()
        {
            // Overwrite the "ID_Control_CS".
            var idScript = currentTank.GetComponent<ID_Control_CS>();
            idScript.isPlayer = isPlayer;
            idScript.relationship = relationship;
            idScript.cost = cost;

            // Overwrite the "Damage_Control_CS", and set this reference.
            var damageScript = currentTank.GetComponent<Damage_Control_CS>();
            damageScript.durability = durability;
            damageScript.spawnerScript = this;

            // Overwrite the "Wheel_Control_CS".
            var wheelScript = currentTank.GetComponentInChildren<Wheel_Control_CS>();
            if (wheelScript)
            {
                wheelScript.maxSpeed = maxSpeed;
            }

            // Overwrite the "Fire_Spawn_CS", and set this reference.
            var fireSpawnScript = currentTank.GetComponentInChildren<Fire_Spawn_CS>();
            fireSpawnScript.attackForce = attackForce;
            fireSpawnScript.spawnerScript = this;

            // Overwrite the "Fire_Control_CS".
            var fireControlScript = currentTank.GetComponentInChildren<Fire_Control_CS>();
            fireControlScript.reloadTime *= reloadTimeMultiplier;

            // Overwrite the "AI_Control_CS".
            var aiScript = currentTank.GetComponentInChildren<AI_Control_CS>();
            if (aiScript)
            {
                aiScript.waypointPack = waypointPack;
                aiScript.waypointRadius = waypointRadius;
                aiScript.patrolType = patrolType;
                aiScript.noAttack = noAttack;
                aiScript.breakthrough = breakthrough;
                aiScript.visibilityRadius = visibilityRadius;
                aiScript.deadAngle = deadAngle;
                aiScript.approachDistance = approachDistance;
                aiScript.lostCount = lostCount;
            }
        }

    }

}