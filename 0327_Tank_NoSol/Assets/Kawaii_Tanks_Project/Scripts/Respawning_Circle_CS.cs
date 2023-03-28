using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace ChobiAssets.KTP
{

    public class Respawning_Circle_CS : MonoBehaviour
    {
        /*
		 * This script is attached to "Respawning" image in the scene.
		 * The appearance of the image are controlled by this script.
		 * This script works in combination with the "Spawner_CS".
		*/

        [Header("Timer Circle settings")]
        [Tooltip("Image of the timer circle.")] public Image timerCircleImage;

        Transform thisTransform;
        Image thisImage;

        [HideInInspector] public static Respawning_Circle_CS instance;


        void Awake()
        {
            instance = this;
        }


        void Start()
        {
            thisTransform = GetComponent<Transform>();
            thisImage = GetComponent<Image>();
            thisImage.enabled = false;
            timerCircleImage.enabled = false;
        }


        public IEnumerator Respawn(float time)
        { // Called from "Spawner_CS".
            thisImage.enabled = true;
            timerCircleImage.enabled = true;

            var count = 0.0f;
            while(count <= time)
            {
                timerCircleImage.fillAmount = count / time;
                count += Time.deltaTime;
                yield return null;
            }

            thisImage.enabled = false;
            timerCircleImage.enabled = false;
        }

    }

}