using UnityEngine;
using System.Collections;

namespace ChobiAssets.KTP
{

    public class AI_Hand_Control_CS : MonoBehaviour
    {
        /*
		 * This script is attached to "AI_Hand" placed under the "AI_Core" in the AI tank.
		 * This script provides auto-brake function to the AI tank.
		 * This script works in combination with "AI_Control_CS" in the parent object.
		*/

        [HideInInspector] public bool isWorking = false; // Referred to from "AI_Control_CS".

        bool isTouching = false;
        float touchCount;
        Collider touchCollider;
        AI_Control_CS aiScript;
        Wheel_Control_CS wheelControlScript;
        Transform thisTransform;
        Vector3 currentPos;
        Vector3 currentScale;
        float initialPosZ;
        float maxScale;


        void Start()
        {
            Initialize();
        }


        void Initialize()
        {
            // Get the references via "ID_Control_CS".
            var idScript = GetComponentInParent<ID_Control_CS>();
            aiScript = idScript.aiScript;
            wheelControlScript = idScript.wheelControlScript;

            // Set the layer.
            gameObject.layer = 2; // "Ignore Raycast" layer.

            // Make it invisible.
            Renderer renderer = GetComponent<Renderer>();
            if (renderer)
            {
                renderer.enabled = false;
            }

            // Make it a trigger.
            Collider collider = GetComponent<Collider>();
            if (collider)
            {
                collider.isTrigger = true;
            }

            // Set the max scale according to the maximum speed.
            maxScale = wheelControlScript.maxSpeed;

            // Get the initial position and scale.
            thisTransform = transform;
            currentPos = thisTransform.localPosition;
            initialPosZ = currentPos.z;
            currentScale = thisTransform.localScale;
        }


        void Update()
        {
            Control_Scale();
            Auto_Brake();
        }


        void Control_Scale()
        {
            float tempRate = Mathf.Pow(wheelControlScript.currentVelocityMag / wheelControlScript.maxSpeed, 2.0f);
            currentScale.z = Mathf.Lerp(0.0f, maxScale, tempRate);
            currentPos.z = Mathf.Lerp(initialPosZ, initialPosZ + (maxScale * 0.5f), tempRate);

            // Set the position and scale.
            thisTransform.localPosition = currentPos;
            thisTransform.localScale = currentScale;
        }


        void Auto_Brake()
        {
            if (isWorking == false)
            {
                return;
            }

            if (isTouching)
            { // The hand is touching an obstacle now.
                if (touchCollider == null)
                { // The collider might have been removed from the scene.
                    isTouching = false;
                    return;
                }

                touchCount += Time.deltaTime;
                if (touchCount > 2.0f)
                {
                    touchCount = 0.0f;
                    // Call the "AI_Control_CS" to escape from a stuck.
                    aiScript.Escape_From_Stuck();
                }
                return;

            }
            else
            { // The hand has been away form the obstacle.
                touchCount -= Time.deltaTime;
                if (touchCount < 0.0f)
                {
                    touchCount = 0.0f;
                    isWorking = false;
                }
            }
        }


        void OnTriggerStay(Collider collider)
        { // Called when the hand touches an obstacle.
            if (isTouching == false && collider.attachedRigidbody)
            { // The hand is not touching an obstacle until now, and the collider has a rigidbody.
                isWorking = true;
                isTouching = true;
                touchCollider = collider;
            }
        }


        void OnTriggerExit()
        { // Called when the hand has been away form the obstacle.
            isTouching = false;
        }


        void Pause(bool isPaused)
        { // Called from "Game_Controller_CS".
            this.enabled = !isPaused;
        }

    }

}