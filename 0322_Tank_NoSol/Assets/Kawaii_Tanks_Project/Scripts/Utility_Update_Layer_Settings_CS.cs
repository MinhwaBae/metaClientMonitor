using UnityEngine;


namespace ChobiAssets.KTP
{

    public class Utility_Update_Layer_Settings_CS : MonoBehaviour
    {
        [ContextMenu("Update_Layer_Settings")]


        void Update_Layer_Settings()
        {

            // MainBody.
            var bodyObject = GetComponentInChildren<Rigidbody>().gameObject;
            bodyObject.layer = Layer_Settings_CS.Body_Layer;


            // Road Wheels.
            var createRoadWheelsScripts = GetComponentsInChildren<Create_RoadWheels_CS>();
            foreach (var createRoadWheelScript in createRoadWheelsScripts)
            {
                var children = createRoadWheelScript.GetComponentsInChildren<Transform>();
                foreach (var child in children)
                {
                    if (child.GetComponent<Wheel_Rotate_CS>())
                    { // The child is a wheel.
                        child.gameObject.layer = Layer_Settings_CS.Wheels_Layer;
                    }
                    else
                    { // The child is a suspension.
                        child.gameObject.layer = Layer_Settings_CS.Reinforce_Layer;
                    }
                }

            }

            
            // Sprocket and Idler Wheels.
            var createSprocketWheelsScripts = GetComponentsInChildren<Create_SprocketWheels_CS>();
            foreach (var createSprocketWheelsScript in createSprocketWheelsScripts)
            {
                var children = createSprocketWheelsScript.GetComponentsInChildren<Transform>();
                foreach (var child in children)
                {
                    if (child.GetComponent<Wheel_Rotate_CS>() || child.GetComponent<Wheel_Sync_CS>())
                    { // The child is a wheel.
                        child.gameObject.layer = Layer_Settings_CS.Wheels_Layer;
                    } // The child is a tensioner arm. >> Default layer.
                }
            }
           

            // Support Wheels.
            var createSupportWheelsScripts = GetComponentsInChildren<Create_SupportWheels_CS>();
            foreach (var createSupportWheelsScript in createSupportWheelsScripts)
            {
                var children = createSupportWheelsScript.GetComponentsInChildren<Transform>();
                foreach (var child in children)
                {
                    // All the children are wheels.
                    child.gameObject.layer = Layer_Settings_CS.Wheels_Layer;
                }

            }
            

            /* (Note.)
             *  The layers of "Armor_Collider", "Extra_Collider" and Bullets are set automatically at the start by attached scripts.
            */

        }
    }

}
