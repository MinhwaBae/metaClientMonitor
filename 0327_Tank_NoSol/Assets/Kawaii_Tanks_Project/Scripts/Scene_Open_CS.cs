using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace ChobiAssets.KTP
{

    public class Scene_Open_CS : MonoBehaviour
    {
        /*
		 * This script is attached to buttons in the scene for opening the specified scene.
		*/


        public string sceneName;


        public void Button_Push()
        { // Called from the button.

            // Disable all the Button.
            var thisButton = GetComponent<Button>();
            thisButton.enabled = false;
            var buttons = FindObjectsOfType<Button>();
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] == thisButton)
                { // The button is itself.
                    continue;
                }

                // Disable the button.
                if (buttons[i].targetGraphic)
                {
                    buttons[i].targetGraphic.enabled = false;
                }
                var tempText = buttons[i].GetComponentInChildren<Text>();
                if (tempText)
                {
                    tempText.enabled = false;
                }
                buttons[i].enabled = false;
            }
           
            // Open scene.
            StartCoroutine("Open_Scene");
        }
       

        IEnumerator Open_Scene()
        {
            // Stop the time.
            Time.timeScale = 0.0f;

            // Disallow the pause.
            if (Game_Controller_CS.instance)
            {
                Game_Controller_CS.instance.allowPause = false;
            }

            // Call "Fade_Control_CS".
            if (Fade_Control_CS.instance)
            {
                Fade_Control_CS.instance.StartCoroutine("Fade_Out");

                // Wait.
                var count = 0.0f;
                while (count < Fade_Control_CS.instance.fadeTime)
                {
                    count += Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            // Set the scene name.
            if (string.IsNullOrEmpty(sceneName))
            { // The scene name is not assigned.
                // Get the current scene name.
                sceneName = SceneManager.GetActiveScene().name;
            }

            // Load the scene.
            SceneManager.LoadSceneAsync(sceneName);

            // Start the time.
            Time.timeScale = 1.0f;
        }

    }

}
