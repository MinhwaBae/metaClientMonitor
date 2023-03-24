using System.Collections;
using UnityEngine;
using UnityEngine.UI;


namespace ChobiAssets.KTP
{

    public class Warning_Image_CS : MonoBehaviour
    {

        [Header("Image settings")]
        [Tooltip("Set this image.")] public Image warningImage;
        [Tooltip("Duration of warning.")] public float warningDuration = 1.0f;
        [Tooltip("Alpha of the warning image.")] public float warningAlpha = 0.3f;

        [HideInInspector] public static Warning_Image_CS instance;


        void Awake()
        {
            instance = this;
        }


        void Start()
        {
            if (warningImage == null)
            {
                warningImage = GetComponent<Image>();
            }
        }


        int warningID;
        public IEnumerator Hit_Warning()
        { // Called from "Damage_Control_CS" in the player's tank.

            warningImage.enabled = true;

            warningID += 1;
            int thisID = warningID;
            float count = 0.0f;
            Color currentColor = warningImage.color;
            while (count < warningDuration)
            {
                if (thisID < warningID)
                {
                    yield break;
                }

                currentColor.a = Mathf.Lerp(warningAlpha, 0.0f, count / warningDuration);
                warningImage.color = currentColor;
                count += Time.deltaTime;
                yield return null;
            }
            warningID = 0;

            warningImage.enabled = false;
        }

    }

}
