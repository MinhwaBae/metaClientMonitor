using UnityEngine;
using System.Collections;

namespace ChobiAssets.KTP
{

	[ System.Serializable]
	public class RoadWheelsProp
	{
		public string parentName;
		public float baseRadius;
		public float [] angles;
	}


	public class Create_RoadWheels_CS : MonoBehaviour
	{
        /* 
		 * This script is attached to the "RoadWheels" object in the tank.
		 * This script is used for creating and setting the road wheels and the suspension arms by the editor script.
		*/

        public float susDistance = 2.0f;
		public int num = 3;
		public float spacing = 1.05f;
		public float susLength = 0.4f;
		public float susAngle = 18.0f;
		public float susMass = 100.0f;
		public float susSpring = 15000.0f;
		public float susDamper = 500.0f;
		public float susTarget = -30.0f;
		public float susForwardLimit = 10.0f;
		public float susBackwardLimit = 30.0f;
		public Mesh susMesh_L;
		public Mesh susMesh_R;
		public Material susMaterial;
        public int susMaterialsNum = 1;
        public Material[] susMaterials;
        public float reinforceRadius = 0.5f;

		public float wheelDistance = 2.9f;
		public float wheelMass = 100.0f;
		public float wheelRadius = 0.5f;
		public PhysicMaterial physicMaterial;
		public Mesh wheelMesh;
		public Material wheelMaterial;
        public int wheelMaterialsNum = 1;
        public Material[] wheelMaterials;

        // For editor script.
        public bool hasChanged;

    }

}