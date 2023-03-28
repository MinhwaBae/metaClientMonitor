using UnityEngine;


namespace ChobiAssets.KTP
{

	public class AI_Helper_CS : MonoBehaviour
	{
        /*
		 * This script is attached to the top object of the tank.
		 * This script works in combination with "AI_Headquaters_CS" in the scene.
		 * These variables are used for giving the target information to the AI tanks.
		 * Also the variables are used for detecting and attacking the target by "AI_Control_CS".
		*/

        [Header ("AI helper settings")]
        [Tooltip("Upper offset for detecting this tank from other AI tanks.")] public float visibilityUpperOffset = 1.0f;

        // Referred to from "AI_Headquaters_CS".
        public ID_Control_CS idScript;
        public Transform bodyTransform; // Referred to from also "AI_Control_CS".
        public AI_Control_CS aiScript;


        void Start()
        {
            // Get the references via "ID_Control_CS".
            idScript = GetComponentInParent<ID_Control_CS>();
            bodyTransform = idScript.bodyTransform;
            aiScript = idScript.aiScript;

            // Send this reference to "AI_Headquaters_CS" in the scene.
            if (AI_Headquaters_CS.instance)
            {
                AI_Headquaters_CS.instance.Receive_Helpers(this);
            }
        }


        void OnDestroy()
        { // Called when the tank is removed from the scene.

            // Call "AI_Headquaters_CS" in the scene to remove this reference from the list.
            if (AI_Headquaters_CS.instance)
            {
                AI_Headquaters_CS.instance.Remove_From_List(this);
            }
        }


    }

}