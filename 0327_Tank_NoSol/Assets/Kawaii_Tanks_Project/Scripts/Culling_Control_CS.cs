using UnityEngine;

namespace ChobiAssets.KTP
{

    [DefaultExecutionOrder (+1)] 
    public class Culling_Control_CS : MonoBehaviour
    {
        /*
         * This script attached to the "MainBody" of the tank.
         * This script controls the enabling of the scripts attached to the tracks, in order to improve the performance.
        */
        
        public float Threshold = 30.0f;


        bool isVisible;
        Transform thisTransform;
        Track_Deform_CS[] deformScripts;
        Track_Scroll_CS[] scrollScripts;


        void Start()
        {
            Initialize();
        }


        void Initialize()
        {
            thisTransform = transform;
            deformScripts = GetComponentsInChildren<Track_Deform_CS>();
            scrollScripts = GetComponentsInChildren<Track_Scroll_CS>();
        }


        void Update()
        {
            Switch_Tracks_Enabling();
        }


        void Switch_Tracks_Enabling()
        {
            bool enabled;
            if (isVisible)
            { // The tank is visible from the main camera.
                Camera currentCam = Camera.main;
                var lodValue = 2.0f * Vector3.Distance(thisTransform.position, currentCam.transform.position) * Mathf.Tan(currentCam.fieldOfView * 0.5f * Mathf.Deg2Rad);
                enabled = (lodValue < Threshold);
            }
            else
            { // The tank is invisible from the main camera.
                enabled = false;
            }

            // Switch the enabling of the track scripts.
            for(int i = 0; i < deformScripts.Length; i++)
            {
                deformScripts[i].enabled = enabled;
            }
            for (int i = 0; i < scrollScripts.Length; i++)
            {
                scrollScripts[i].enabled = enabled;
            }
        }


        void OnBecameVisible()
        {
            isVisible = true;
        }


        void OnBecameInvisible()
        {
            isVisible = false;
        }

    }
}
