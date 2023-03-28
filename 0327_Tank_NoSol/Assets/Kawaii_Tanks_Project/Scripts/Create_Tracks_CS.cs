using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace ChobiAssets.KTP
{
    [System.Serializable]
    public class CreatingScrollTrackInfo
    {
        public Vector3 position;
        public float radius;
        public float startAngle;
        public float endAngle;
    }


    public class Create_Tracks_CS : MonoBehaviour
    {
        /* 
		 * This script is attached to each track in the tank.
         * This script is used for creating the mesh of the track.
		*/

        public bool isLeft = true;
        public float posX = 1.45f;
        public float width = 0.8f;
        public float height = 0.08f;
        public Material mat;
        public float scale = 0.1f;
        public float lineA = 0.5f;
        public float lineB = 0.49f;
        public float lineC = 0.49f;
        public float lineD = 0.38f;
        public float lineE = 0.38f;
        public float lineF = 0.37f;
        public float lineG = 0.62f;
        public float lineH = 0.5f;
        public int guideCount = 0;
        public float guideHeight = 0.07f;
        public float[] guidePositions;
        public float guideLineTop = 0.0f;
        public float guideLineBottom = 0.0f;

        public Vector3[] positionArray;
        public float[] radiusArray;
        public float[] startAngleArray;
        public float[] endAngleArray;

        public bool showTexture = false;

        public Mesh savedMesh;

        // For editor script.
        public bool hasChanged;


        void Start()
        {
            Check_Mesh();
        }


        void Check_Mesh()
        {
            // Check the mesh had been saved or not.
            MeshFilter thisMeshFilter = GetComponent<MeshFilter>();
            if (thisMeshFilter && thisMeshFilter.sharedMesh)
            {
                if (thisMeshFilter.sharedMesh.name == " Instance")
                {
                    Debug.LogWarning("The mesh of '" + this.name + "' is not saved yet.");
                }
            }
            else
            {
                Debug.LogWarning("The mesh of '" + this.name + "' is not created yet.");
            }
            Destroy(this);
        }

    }

}
