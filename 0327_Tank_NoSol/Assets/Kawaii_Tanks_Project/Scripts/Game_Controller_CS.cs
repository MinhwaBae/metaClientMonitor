using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace ChobiAssets.KTP
{

    public class Game_Controller_CS : MonoBehaviour
    {
        /*
		 * This script is attached to the "Game_Controller" in the scene.
		 * This script controls the physics settings, the layers collision settings and the cursor state in the scene.
		 * Also the general functions such as quit and pause are controlled by this script.
		*/

        [Header("General functions settings")]
        [Tooltip("Canvas(es) for the pause screen.")] public Canvas[] pauseCanvases;
        [Tooltip("Prefab for touch controls.")] public GameObject touchControlsPrefab;
        [Tooltip("These gameobjects will be removed on mobile platforms.")] public GameObject[] uselessObjectsOnMobiles;

        //MH
        [Tooltip("Canvas(es) for the metaverse monitoring error test screen.")] public Canvas[] testCanvases;

#if !UNITY_ANDROID && !UNITY_IPHONE
        [Tooltip("Show cursor or not.")] public bool showCursor = false;
#endif


        [HideInInspector] public bool allowPause = true; // Controlled by some events.
        bool isPaused;
        bool storedCursorVisible;
        List<ID_Control_CS> idScriptsList = new List<ID_Control_CS>();


        //MH
        [HideInInspector] public bool allowTest = true; // Controlled by some events.
        bool isForTested;
#if UNITY_ANDROID || UNITY_IPHONE
        bool isPauseButtonDown;
#endif

        [HideInInspector] public static Game_Controller_CS instance;


        void Awake()
        {
            instance = this;

            // Layer settings.
            Layer_Settings_CS.Layers_Collision_Settings();

            // Set the frame rate.
            if (General_Settings_CS.fixFrameRate)
            {
                QualitySettings.vSyncCount = 0;
#if !UNITY_ANDROID && !UNITY_IPHONE
                Application.targetFrameRate = General_Settings_CS.targetFrameRate;
#else
                Application.targetFrameRate = General_Settings_CS.targetFrameRateMobile;
#endif
            }

            // Set the Fixed Timestep in the scene.
#if !UNITY_ANDROID && !UNITY_IPHONE
            Time.fixedDeltaTime = General_Settings_CS.fixedTimestep;
#else
            Time.fixedDeltaTime = General_Settings_CS.fixedTimestepMobile;
#endif

#if UNITY_ANDROID || UNITY_IPHONE
            // Instantiate the touch controls prefab.
            if (touchControlsPrefab)
            {
				Instantiate (touchControlsPrefab);
			}

            // Remove the useless objects on mobile platforms.
            for (int i = 0; i < uselessObjectsOnMobiles.Length; i++)
            {
                Destroy(uselessObjectsOnMobiles[i]);
            }
#else

            // Set the cursor state.
            Switch_Cursor(showCursor);
#endif

            // Disable the pause canvas.
            for (int i = 0; i < pauseCanvases.Length; i++)
            {
                pauseCanvases[i].enabled = false;
            }

            //MH
            // Disable the pause canvas.
            for (int i = 0; i < testCanvases.Length; i++)
            {
                testCanvases[i].enabled = false;
            }
        }


        public void Receive_ID_Script(ID_Control_CS idScript)
        { // Called from "ID_Control_CS" in tanks in the scene, when the tank is spawned.

            // Store the "ID_Control_CS".
            idScriptsList.Add(idScript);
        }


        void Update()
        {
            // Reload the scene.
#if !UNITY_ANDROID && !UNITY_IPHONE
            if (General_Settings_CS.allowReloadScene && Key_Bindings_CS.IsRestartKeyDown())
            {
                Time.timeScale = 1.0f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                return;
            }
#endif

            // Quit.
            if (General_Settings_CS.allowInstantQuit && Key_Bindings_CS.IsQuitKeyDown())
            {
                Application.Quit();
                return;
            }


            // Pause.
#if UNITY_ANDROID || UNITY_IPHONE
            if (allowPause)
            {
                if (isPauseButtonDown == false && Key_Bindings_CS.IsPauseButtonPressing)
                { // Pause button is pressed.
                    isPauseButtonDown = true;
                    Pause();
                    return;
                }

                if (isPauseButtonDown && Key_Bindings_CS.IsPauseButtonPressing == false)
                { // Pause button is released.
                    isPauseButtonDown = false;
                    return;
                }
            }
#else
            if (allowPause && Key_Bindings_CS.IsPauseKeyDown())
            {
                Pause();
                return;
            }
#endif

            // Control the cursor state.
#if !UNITY_ANDROID && !UNITY_IPHONE
            if (General_Settings_CS.allowSwitchCursor && Key_Bindings_CS.IsSwitchCursorModeKeyDown())
            {
                Switch_Cursor(Cursor.visible == false);
            }
#endif

            //MH
            //meta error test
#if UNITY_ANDROID || UNITY_IPHONE
            if (allowTest)
            {
                if (isPauseButtonDown == false && Key_Bindings_CS.IsPauseButtonPressing)
                { // Pause button is pressed.
                    isPauseButtonDown = true;
                    Pause();
                    return;
                }

                if (isPauseButtonDown && Key_Bindings_CS.IsPauseButtonPressing == false)
                { // Pause button is released.
                    isPauseButtonDown = false;
                    return;
                }
            }
#else
            if (allowTest && Key_Bindings_CS.IsTestKeyDown())
            {
                ErrorTestMenu();
                return;
            }
#endif
        }


        public void Switch_Cursor(bool isVisible)
        { // Called also from "Score_Manage_CS".
            if (isVisible)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }


        public void Pause()
        { // Called from "Resume" button.
            isPaused = !isPaused;

            // Set the time scale, and the cursor state.
            if (isPaused)
            {
                Time.timeScale = 0.0f;
                storedCursorVisible = Cursor.visible;
#if !UNITY_ANDROID && !UNITY_IPHONE
                Switch_Cursor(true);
#endif
            }
            else
            {
                Time.timeScale = 1.0f;
#if !UNITY_ANDROID && !UNITY_IPHONE
                Switch_Cursor(storedCursorVisible);
#endif
            }

            // Control the pause canvases.
            for (int i = 0; i < pauseCanvases.Length; i++)
            {
                pauseCanvases[i].enabled = isPaused;
            }

            // Send message to all the tank parts via "ID_Control_CS" in each tank in the scene.
            for (int i = 0; i < idScriptsList.Count; i++)
            {
                if (idScriptsList[i])
                {
                    idScriptsList[i].BroadcastMessage("Pause", isPaused, SendMessageOptions.DontRequireReceiver);
                }
            }

            // Send message to "Camera_Manager_CS" in the scene.
            if (Camera_Manager_CS.instance)
            {
                Camera_Manager_CS.instance.BroadcastMessage("Pause", isPaused, SendMessageOptions.DontRequireReceiver);
            }
        }


        public void Remove_ID(ID_Control_CS idScript)
        { // Called from "ID_Control_CS", when the tank is removed from the scene.

            // Remove the "ID_Control_CS" from the list.
            idScriptsList.Remove(idScript);
        }


        //MH
        //meta error test menu
        public void ErrorTestMenu()
        { // Called from "Resume" button.
            isForTested = !isForTested;

            // Set the time scale, and the cursor state.
            if (isForTested)
            {
                Time.timeScale = 0.0f;
                storedCursorVisible = Cursor.visible;//mouse pointer visible
#if !UNITY_ANDROID && !UNITY_IPHONE
                Switch_Cursor(true);
#endif
            }
            else
            {
                Time.timeScale = 1.0f;
#if !UNITY_ANDROID && !UNITY_IPHONE
                Switch_Cursor(storedCursorVisible);
#endif
            }

            // Control the pause canvases.
            for (int i = 0; i < testCanvases.Length; i++)
            {
                testCanvases[i].enabled = isForTested;
            }

            // Send message to all the tank parts via "ID_Control_CS" in each tank in the scene.
            for (int i = 0; i < idScriptsList.Count; i++)
            {
                if (idScriptsList[i])
                {
                    idScriptsList[i].BroadcastMessage("Pause", isPaused, SendMessageOptions.DontRequireReceiver); //이대로 써도 무방할(Pause)
                }
            }

            // Send message to "Camera_Manager_CS" in the scene.
            if (Camera_Manager_CS.instance)
            {
                Camera_Manager_CS.instance.BroadcastMessage("Pause", isPaused, SendMessageOptions.DontRequireReceiver);
            }
        }

    }

}
