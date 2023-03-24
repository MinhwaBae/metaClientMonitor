using UnityEngine;


namespace ChobiAssets.KTP
{

    [DefaultExecutionOrder (-1)] // This script must be executed before all the scripts executed the initializing.
    public class ID_Control_CS : MonoBehaviour
    {
        /* 
		 * This script is attached to the top object of the tank.
		 * This script stores the tank identification such as the relationship, and several scripts refer to the values.
		 * Also, the position marker for this tank is instantiated by this script.
		*/

        [Header("ID settings")]
        [Tooltip("Player or NPC.")] public bool isPlayer = true;
        [Tooltip("Relationship (0 = Friend, 1 = Enemy)"), Range(0, 1)] public int relationship = 0;
        [Tooltip("'Kill Count' for this tank.")] public int cost = 1;

        [Header("Camera offset settings")]
        [Tooltip("Offset height for the main camera pivot.")] public float cameraOffset = 3.0f;


        // Referred to from the scripts in the tank parts.
        [HideInInspector] public Rigidbody bodyRigidbody;
        [HideInInspector] public Transform bodyTransform;
        [HideInInspector] public Aiming_Control_CS aimingScript;
        [HideInInspector] public Wheel_Control_CS wheelControlScript;
        [HideInInspector] public Fire_Spawn_CS fireSpawnScript;
        [HideInInspector] public AI_Control_CS aiScript;


        void Start()
        { // Do not change to 'Awake()', because the values are overwritten by "Spawner_CS" before 'Start()'.

            // Store components of the tank.
            Store_Components();

            // Call the "Camera_Manager_CS" in the scene only when the tank is player's.
            if (isPlayer && Camera_Manager_CS.instance)
            {
                Camera_Manager_CS.instance.Set_Follow_Target(bodyTransform, cameraOffset);
            }

            // Send message to the "Game_Controller" in the scene to add this tank to the list.
            if (Game_Controller_CS.instance)
            {
                Game_Controller_CS.instance.SendMessage("Receive_ID_Script", this, SendMessageOptions.DontRequireReceiver);
            }
        }


        void Store_Components()
        {
            bodyRigidbody = GetComponentInChildren<Rigidbody>();
            bodyTransform = bodyRigidbody.transform;
            aimingScript = bodyTransform.GetComponent<Aiming_Control_CS>();
            wheelControlScript = bodyTransform.GetComponent<Wheel_Control_CS>();
            fireSpawnScript = bodyTransform.GetComponentInChildren<Fire_Spawn_CS>();
            aiScript = bodyTransform.GetComponentInChildren<AI_Control_CS>();
        }


        void Destroyed_Linkage()
        { // Called from "Damage_Control_CS".

            // Change the tag.
            gameObject.tag = "Finish";
        }


        void OnDestroy()
        { // Called when the tank is removed from the scene.
            
            // Send message to the "Game_Controller" in the scene to remove this tank from the lists.
            if (Game_Controller_CS.instance)
            {
                Game_Controller_CS.instance.SendMessage("Remove_ID", this, SendMessageOptions.DontRequireReceiver);
            }
        }

    }

}
