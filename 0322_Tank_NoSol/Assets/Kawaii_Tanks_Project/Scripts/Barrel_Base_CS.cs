using UnityEngine;


namespace ChobiAssets.KTP
{

    public class Barrel_Base_CS : MonoBehaviour
    {
        /* 
		 * This script is attached to the "Barrel_Base" in the tank.
		 * This script is used for creating and setting the barrel by the editor script.
		*/

        public Mesh partMesh;

        public int collidersNum;
        public Mesh[] collidersMesh;

        public int materialsNum = 1;
        public Material[] materials;

        public float offsetX = 0.0f;
        public float offsetY = 0.0f;
        public float offsetZ = 0.0f;

        // For editor script.
        public bool hasChanged;


        void Start()
        {
            Destroy(this);
        }

    }

}