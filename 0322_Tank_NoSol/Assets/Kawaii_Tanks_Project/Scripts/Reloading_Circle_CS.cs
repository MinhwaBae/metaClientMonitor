using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace ChobiAssets.KTP
{

    public class Reloading_Circle_CS : MonoBehaviour
    {
        /*
		 * This script is attached to "Reloading" image in the scene.
		 * The appearance of the image are controlled by this script.
		 * This script works in combination with the "Fire_Control_CS".
		*/

        [Header("Timer Circle settings")]
        [Tooltip("Image of the timer circle.")] public Image timerCircleImage;

        Transform thisTransform;
        Image thisImage;
        Fire_Control_CS fireControlScript;

        [HideInInspector] public static Reloading_Circle_CS instance;


        void Awake()
        {
            instance = this;
        }


        void Start()
        {
            Initialize();
        }


        void Initialize()
        {
            thisTransform = GetComponent<Transform>();
            thisImage = GetComponent<Image>();
        }


        public void Get_Fire_Control_Script(Fire_Control_CS fireControlScript)
        { // Called from "Fire_Control_CS" in the player's tank.
            this.fireControlScript = fireControlScript;
        }


        void Update()
        {
            if (fireControlScript == null)
            { // The tank has been destroyed.
                Enable_Images(false);
                return;
            }

            if (fireControlScript.isLoaded)
            {
                Enable_Images(false);
            }
            else
            {
                Enable_Images(true);
                Control_Circle();
            }
        }


        void Enable_Images(bool enabled)
        {
            if (thisImage.enabled != enabled)
            {
                thisImage.enabled = enabled;
                timerCircleImage.enabled = enabled;
            }
        }


        void Control_Circle()
        {
            timerCircleImage.fillAmount = fireControlScript.loadingCount / fireControlScript.reloadTime;
        }

    }

}