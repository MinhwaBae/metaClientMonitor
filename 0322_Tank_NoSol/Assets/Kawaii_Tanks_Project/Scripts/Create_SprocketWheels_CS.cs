using UnityEngine;
using System.Collections;

namespace ChobiAssets.KTP
{

	public class Create_SprocketWheels_CS : MonoBehaviour
	{
        /* 
		 * This script is attached to "SprocketWheesls" and "IdlerWheels" objects in the tank.
		 * This script is used for creating and setting the sprocket wheels to idler wheels by the editor script.
		*/

        public bool useArm = false;
		public float armDistance = 2.2f;
		public float armLength = 0.15f;
		public float armAngle = 60.0f;
		public Mesh armMesh_L;
		public Mesh armMesh_R;
		public Material armMaterial;
        public int armMaterialsNum = 1;
        public Material[] armMaterials;

        public float wheelDistance = 2.9f;
		public float wheelMass = 100.0f;
		public float wheelRadius = 0.5f;
		public PhysicMaterial physicMaterial;
		public Mesh wheelMesh;
		public Material wheelMaterial;
        public int wheelMaterialsNum = 1;
        public Material[] wheelMaterials;

        public float radiusOffset = 0.0f;

        // For editor script.
        public bool hasChanged;
    }

}