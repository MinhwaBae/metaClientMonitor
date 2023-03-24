using UnityEngine;
using System.Collections;
using UnityEngine.UI;


// This script must be attached to "Cannon_Base".
namespace ChobiAssets.KTP
{

    public class Fire_Control_CS : MonoBehaviour
    {
        /*
		 * This script is attached to the "Cannon_Base" in the tank.
		 * This script controls the firining of the tank.
		 * When firing, this script calls "Fire_Spawn_CS" and "Barrel_Control_CS" scripts placed under this object in the hierarchy.
		 * In case of AI tank, this script works in combination with "AI_Control_CS", "Turret_Control_CS", "Cannon_Control_CS" and "Aiming_Control_CS".
		*/

        [Header("Fire control settings")]
        [Tooltip("Loading time. (Sec)")] public float reloadTime = 4.0f; // Referred to from "Reloading_Circle_CS".
        [Tooltip("Recoil force with firing.")] public float recoilForce = 5000.0f;
        [Tooltip("Display the reloading circle.")] public bool useReloadingCircle = true;


        // Referred to from "Reloading_Circle_CS".
        [HideInInspector] public bool isLoaded = true;
        [HideInInspector] public float loadingCount;

        // Referred to from "Fire_Control_Input_99_AI_CS" script.
        [HideInInspector] public AI_Control_CS aiScript;
        [HideInInspector] public Fire_Spawn_CS fireSpawnScript;
        [HideInInspector] public Aiming_Control_CS aimingControlScript;


        Rigidbody bodyRigidbody;
        Transform thisTransform;
        Barrel_Control_CS barrelScript;

        Fire_Control_Input_00_Base_CS inputScript;
        bool isPlayer;


        void Start()
        {
            Initialize();
        }


        void Initialize()
        {
            thisTransform = transform;
            barrelScript = thisTransform.parent.GetComponentInChildren<Barrel_Control_CS>();

            // Get the references via "ID_Control_CS".
            var idScript = GetComponentInParent<ID_Control_CS>();
            isPlayer = idScript.isPlayer;
            bodyRigidbody = idScript.bodyRigidbody;
            aimingControlScript = idScript.aimingScript;
            fireSpawnScript = idScript.fireSpawnScript;
            aiScript = idScript.aiScript;

            // Set the input script.
            Set_Input_Script();

            // Prepare the "inputScript".
            inputScript.Prepare(this);

            // Send this reference to the "Reloading_Circle_CS" in the scene.
            if (isPlayer && useReloadingCircle && Reloading_Circle_CS.instance)
            {
                Reloading_Circle_CS.instance.Get_Fire_Control_Script(this);
            }
        }


        protected virtual void Set_Input_Script()
        {
            if (aiScript)
            {
                inputScript = gameObject.AddComponent<Fire_Control_Input_99_AI_CS>();
            }
            else
            {
#if !UNITY_ANDROID && !UNITY_IPHONE
                inputScript = gameObject.AddComponent<Fire_Control_Input_01_Desktop_CS>();
#else
                inputScript = gameObject.AddComponent<Fire_Control_Input_02_Mobile_CS>();
#endif
            }
        }


        void Update()
        {
            if (isLoaded == false)
            {
                return;
            }

            if (isPlayer || aiScript)
            {
                inputScript.Get_Input();
            }
        }


        public void Fire()
        {
            // Call the "Fire_Spawn_CS".
            fireSpawnScript.Fire_Linkage();

            // Call the "Barrel_Control_CS".
            barrelScript.Fire_Linkage();

            // Add recoil shock force to the MainBody.
            bodyRigidbody.AddForceAtPosition(-thisTransform.forward * recoilForce, thisTransform.position, ForceMode.Impulse);

            // Reload.
            StartCoroutine("Reload");
        }


        IEnumerator Reload()
        {
            isLoaded = false;
            loadingCount = 0.0f;

            while (loadingCount < reloadTime)
            {
                loadingCount += Time.deltaTime;
                yield return null;
            }

            isLoaded = true;
            loadingCount = reloadTime;
        }


        void Destroyed_Linkage()
        { // Called from "Damage_Control_CS".
            Destroy(this);
        }


        void Pause(bool isPaused)
        { // Called from "Game_Controller_CS".
            this.enabled = !isPaused;
        }

    }

}
