using UnityEngine;
using System.Collections;

namespace ChobiAssets.KTP
{

	public class Create_SupportWheels_CS : MonoBehaviour
	{
        /* 
		 * This script is attached to the "SupportWheels" object in the tank.
		 * This script is used for creating and setting the support wheels (return rollers) by the editor script.
		*/

        public float wheelDistance = 2.13f;
		public int num = 2;
		public float spacing = 1.8f;
		public Mesh wheelMesh;
		public Material wheelMaterial;
        public int wheelMaterialsNum = 1;
        public Material[] wheelMaterials;
        public float radiusOffset;

        // For editor script.
        public bool hasChanged;
    }

}