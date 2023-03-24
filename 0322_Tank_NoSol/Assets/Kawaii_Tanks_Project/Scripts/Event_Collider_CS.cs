using UnityEngine;
using System.Collections;

namespace ChobiAssets.KTP
{

    public class Event_Collider_CS : MonoBehaviour
    {
        /*
         * This script is attached to "Event_Collider" in the scene.
         * When the tank touches this collider, the penalty score is added to the specified team.
        */

        [Header("Event settings")]
        [Tooltip("Relationship of the tank should be detected. (0 = Friend, 1 = Enemy)"), Range(0, 1)] public int detectedRelationship = 1;
        [Tooltip("Relationship of the team should get the penalty score. (0 = Friend, 1 = Enemy)"), Range(0, 1)] public int targetRelationship = 0;
        [Tooltip("Penalty 'Killed Count'")] public int penaltyKilledCount = 10;
        [Tooltip("Make it visible or not.")] public bool isVisible = false;


        void Start()
        {
            Initialize();
        }


        void Initialize()
        {
            // Set the layer.
            gameObject.layer = 2; // Ignore Raycast.

            // Make all the colliders triggers.
            var colliders = GetComponents<Collider>();
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].isTrigger = true;
            }

            // Make it invisible.
            var renderer = GetComponent<Renderer>();
            renderer.enabled = isVisible;
        }


        void OnTriggerEnter(Collider collider)
        {
            if (collider.gameObject.layer == Layer_Settings_CS.Body_Layer)
            { // The collision object should be a tank body.

                // Check the tank is living.
                var rootTransform = collider.transform.root;
                if (rootTransform.tag == "Finish")
                { // The tank is already dead, or already touched any "Event_Collider".
                    return;
                }

                // Check the relationship.
                var idScript = rootTransform.GetComponent<ID_Control_CS>();
                if (idScript && idScript.relationship == detectedRelationship)
                { // The tank has the same relationship.

                    // Call "Score_Manager_CS" in the scene to update the score.
                    if (Score_Manager_CS.instance)
                        Score_Manager_CS.instance.Update_Total_Killed(targetRelationship, penaltyKilledCount);

                    // Make the tank resapwn by changing the tag to "Finish".
                    rootTransform.tag = "Finish";
                }
            }
        }
    }

}
