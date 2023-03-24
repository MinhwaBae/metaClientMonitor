using UnityEngine;
using System.Collections;
using UnityEngine.UI;


namespace ChobiAssets.KTP
{

    public class Damage_Control_CS : MonoBehaviour
    {
        /*
         * This script is attached to the top object of the tank.
         * This script controls the damage system of the tank.
         * The durability (Hit points) and the damaged visual effects are controlled by this script.
         * 
        */ 
        
        [Header("Damage settings")]
        [Tooltip("Durability (Hit points) of the tank.")] public float durability = 300.0f;
        [Tooltip("Prefab for destroyed effects.")] public GameObject destroyedPrefab;
        [Tooltip("Prefab for dying effects.")] public GameObject dyingPrefab;
        [Tooltip("Rate for starting dying.")] public float dyingRate = 0.3f;
        [Tooltip("Prefab for displaying the durability.")] public GameObject textPrefab;
        [Tooltip("Name of the canvas for 'Text Prefab'.")] public string canvasName = "Canvas_Texts";


        // Set by "Spawner_CS".
        [HideInInspector] public Spawner_CS spawnerScript;


        Transform bodyTransform;
        float killHight = -64.0f;
        Damage_Display_CS displayScript;
        float initialDurability;
        float currentDurability;
        GameObject dyingObject;
        bool isInvincible;
        bool isPlayer;
        bool isDead;
        AI_Control_CS aiScript;


        void Start()
        {
            Initialize();
        }


        void Initialize()
        {
            // Get the references via "ID_Control_CS".
            var idScript = GetComponentInParent<ID_Control_CS>();
            isPlayer = idScript.isPlayer;
            bodyTransform = idScript.bodyTransform;
            aiScript = idScript.aiScript;

            // Set the kill height.
            if (Score_Manager_CS.instance)
            {
                killHight = Score_Manager_CS.instance.killHight;
            }

            // Store the initial durability.
            initialDurability = durability;

            // Set the current durability.
            currentDurability = initialDurability;

            // Call "Score_Manager_CS" in the scene to update the current durability.
            if (spawnerScript && Score_Manager_CS.instance)
            {
                Score_Manager_CS.instance.Update_Current_Durability(spawnerScript, Mathf.Ceil(currentDurability), initialDurability);
            }

            // Setup the damage text.
            Set_Damage_Text();

            // Make the tank invincible for a moment.
            StartCoroutine("Invincible_Timer", 5.0f);
        }


        void Set_Damage_Text()
        {
            if (textPrefab == null || string.IsNullOrEmpty(canvasName) || initialDurability == Mathf.Infinity)
            {
                return;
            }

            // Find the canvas.
            var canvasObject = GameObject.Find(canvasName);
            if (canvasObject == null)
            {
                Debug.LogWarning(canvasName + " cannot be found in the scene.");
                return;
            }

            // Instantiate the text prefab.
            var textObject = Instantiate(textPrefab, Vector3.zero, Quaternion.identity, canvasObject.transform) as GameObject;

            // Setup the "Damage_Display_CS" script in the text object.
            displayScript = textObject.GetComponent<Damage_Display_CS>();
            displayScript.targetTransform = bodyTransform;
            displayScript.isPlayer = isPlayer;
        }


        IEnumerator Invincible_Timer(float duration)
        { // Make the tank invincible during the specified time.
            isInvincible = true;
            yield return new WaitForSeconds(duration);
            isInvincible = false;
        }


        void Update()
        {
            // Check the hight and the rotation.
            Check_Height_And_Rotation();
        }


        void Check_Height_And_Rotation()
        {
            if (bodyTransform.position.y < killHight)
            { // The tank is under the kill hight.
                Start_Destroying();
                return;
            }

            if (Mathf.Abs(Mathf.DeltaAngle(0.0f, bodyTransform.localEulerAngles.z)) > 90.0f)
            { // The tank has rolled over.
                Start_Destroying();
                return;
            }
        }


        public bool Get_Damage(float damageValue)
        { // Called from "Bullet_Nav_CS".
            if (isDead)
            { // The tank has already destroyed.
                return false;
            }

            if (aiScript)
            { // AI tank.
                // Call "AI_Control_CS" to disable the dead angle.
                aiScript.StartCoroutine("Wake_Up_Timer");
            }

            // Check the tank is invincible.
            if (isInvincible)
            {
                return false;
            }

            // Reduce the current durability.
            currentDurability -= damageValue;
            currentDurability = Mathf.Clamp(currentDurability, 0.0f, initialDurability);

            // Call "Score_Manager_CS" in the scene to update the current durability.
            if (spawnerScript && Score_Manager_CS.instance)
            {
                Score_Manager_CS.instance.Update_Current_Durability(spawnerScript, Mathf.Ceil(currentDurability), initialDurability);
            }

            // Check the tank is alive or not.
            if (currentDurability > 0.0f)
            { // Alive.
                // Display the damage text
                if (displayScript)
                {
                    displayScript.Get_Damage(currentDurability, initialDurability);
                }

                // Call the "Warning_Image_CS" in the scene.
                if (isPlayer && Warning_Image_CS.instance)
                {
                    Warning_Image_CS.instance.StartCoroutine("Hit_Warning");
                }

                // Display the dying effect.
                if (dyingObject == null && currentDurability < initialDurability * dyingRate)
                { // The dying object has not been spawned yet, and the current durability is less than the dying rate.
                    Spawn_Dying_Effect();
                }
                
                return false; // The bullet could not destroy the tank.
            }
            else
            { // Dead
                Start_Destroying();
                
                return true; // The bullet has destroyed the tank.
            }
        }


        public void Get_Recovery(float recoveryValue)
        {
            // Increase the current durability.
            currentDurability += recoveryValue;
            currentDurability = Mathf.Clamp(currentDurability, 0.0f, initialDurability);

            // Call "Score_Manager_CS" in the scene to update the current durability.
            if (spawnerScript && Score_Manager_CS.instance)
            {
                Score_Manager_CS.instance.Update_Current_Durability(spawnerScript, Mathf.Ceil(currentDurability), initialDurability);
            }

            // Display the damage text
            if (displayScript)
            {
                displayScript.Get_Damage(currentDurability, initialDurability);
            }

            // Remove the dying effect.
            if (dyingObject && currentDurability > initialDurability * dyingRate)
            { // The dying object had been spawned, and the durability is greater than the dying rate.
                Destroy(dyingObject);
            }
        }


        void Spawn_Dying_Effect()
        {
            // Instantiate the dying prefab.
            if (dyingPrefab)
            {
                dyingObject = Instantiate(dyingPrefab, bodyTransform.position, Quaternion.identity, bodyTransform) as GameObject;
            }
        }


        void Start_Destroying()
        {
            // Set the dead flag.
            isDead = true;

            // Call "Score_Manager_CS" in the scene to update 'killed' count.
            if (spawnerScript && Score_Manager_CS.instance)
            {
                Score_Manager_CS.instance.Update_Killed_Count(spawnerScript);
            }

            // Send message to all the parts.
            transform.root.BroadcastMessage("Destroyed_Linkage", SendMessageOptions.DontRequireReceiver);
        }


        void Destroyed_Linkage()
        {
            // Spawn the destroyed effects.
            if (destroyedPrefab)
            {
                Instantiate(destroyedPrefab, bodyTransform.position, Quaternion.identity, bodyTransform);
            }

            // Remove the damage text.
            if (displayScript)
            {
                Destroy(displayScript.gameObject);
            }

            // Remove the dying effect.
            if (dyingObject)
            {
                Destroy(dyingObject);
            }

            // Remove this script.
            Destroy(this);
        }


        void Pause(bool isPaused)
        { // Called from "Game_Controller_CS".
            this.enabled = !isPaused;
        }

    }

}