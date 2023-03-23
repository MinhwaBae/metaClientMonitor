using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace ChobiAssets.KTP
{

    public class Message_Motion_CS : MonoBehaviour
    {
        /*
         * This script is attached to "Text_Large" object placed under the "Canvas_Texts" in the scene.
         * This script controls the appearance and the movement of the text.
        */

        [Tooltip("Curve for moving the text.")] public AnimationCurve motionCurve;


        Text thisText;
        RectTransform textTransform;
        Shadow shadowScript;
        Outline outlineScript;
        Vector3 initialPos;
        bool isWorking;


        void Start()
        {
            thisText = GetComponent<Text>();
            thisText.enabled = false;
            textTransform = thisText.rectTransform;
            shadowScript = GetComponent<Shadow>();
            outlineScript = GetComponent<Outline>();
            initialPos = textTransform.position;
        }


        public void Show_Message(string message, float displayingTime, Color color)
        { // Called from "Score_Manager_CS" in the scene.
            thisText.enabled = true;
            thisText.text = message;

            // Set the colors.
            if (shadowScript)
            {
                shadowScript.effectColor = color;
            }
            if (outlineScript)
            {
                outlineScript.effectColor = color;
            }

            if (displayingTime == Mathf.Infinity)
            { // Display the text simply.
                Simple();
            }
            else
            { // Display the text with motion.
                StartCoroutine("Motion", displayingTime);
            }
        }


        void Simple()
        {
            isWorking = false;
            textTransform.position = initialPos;
        }


        public IEnumerator Motion(float displayingTime)
        {
            if (isWorking)
            { // The previous message is displayed now.
                // Cancel the previous message.
                isWorking = false;
                yield return null;
            }
            isWorking = true;

            // Motion
            var currentPos = textTransform.position;
            var totalWidth = Screen.width + textTransform.rect.size.x;
            var adjustSize = textTransform.rect.size.x * 0.5f;
            var count = 0.0f;
            while (count < displayingTime)
            {
                if (isWorking == false)
                {
                    yield break;
                }
                currentPos.x = totalWidth * motionCurve.Evaluate(count / displayingTime) - adjustSize;
                textTransform.position = currentPos;
                count += Time.deltaTime;
                yield return null;
            }

            // Finish
            thisText.enabled = false;
        }

    }
}
