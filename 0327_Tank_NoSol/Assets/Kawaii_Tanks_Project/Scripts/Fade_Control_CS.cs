using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ChobiAssets.KTP
{

    public class Fade_Control_CS : MonoBehaviour
    {
        /*
		 * This script is attached to the image used for 'fade in' and 'fade out'.
		 * The 'fade in' is called at the opening of the scene.
		 * The 'fade out' is called from "Scene_Open_CS" in the buttons before the scene has been changed.
		*/


        public Image fadeImage;
        public float fadeTime = 1.0f;

        [HideInInspector] public static Fade_Control_CS instance;


        void Awake()
        {
            instance = this;
        }


        void Start()
        {
            if (fadeImage == null)
            {
                fadeImage = GetComponent<Image>();
            }

            // Fade in at the start of the scene.
            StartCoroutine("Fade_In");
        }


        IEnumerator Fade_In()
        {
            float count = 0.0f;
            Color currentColor = fadeImage.color;
            while (count < fadeTime)
            {
                // Increase the audio volume.
                AudioListener.volume = count / fadeTime;

                // Decrease the image alpha.
                currentColor.a = 1.0f - (count / fadeTime);
                fadeImage.color = currentColor;

                count += Time.unscaledDeltaTime; ;
                yield return null;
            }

            AudioListener.volume = 1.0f;
            currentColor.a = 0.0f;
            fadeImage.color = currentColor;
        }


        public IEnumerator Fade_Out()
        { // Called from "Scene_Open_CS" attached to buttons in the scene.

            float count = 0.0f;
            Color currentColor = fadeImage.color;
            while (count < fadeTime)
            {
                // Decrease the audio volume.
                AudioListener.volume = 1.0f - (count / fadeTime);

                // Increase the image alpha.
                currentColor.a = count / fadeTime;
                fadeImage.color = currentColor;

                count += Time.unscaledDeltaTime; ;
                yield return null;
            }
        }

    }

}
