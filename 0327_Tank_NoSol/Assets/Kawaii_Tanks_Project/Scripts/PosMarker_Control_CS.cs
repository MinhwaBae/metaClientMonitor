using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;


namespace ChobiAssets.KTP
{

    [System.Serializable]
    public class PositionMarkerProp
    {
        public Image markerImage;
        public Transform markerTransform;
        public Transform rootTransform;
        public Transform bodyTransform;
        public AI_Control_CS aiScript;
    }


    [DefaultExecutionOrder(+2)] // (Note.) This script is executed after the "Camera_Manager_CS", in order to move the marker smoothly.
    public class PosMarker_Control_CS : MonoBehaviour
    {
        /*
		 * This script is attached to "Game_Controller" in the scene..
		 * This script controls the appearance and the position of position-markers.
		 * This script works in combination with "ID_Control_CS" and "AI_Control_CS" in the tank.
		*/

        [Header("Position-Marker settings")]
        [Tooltip("Prefab of the position marker.")] public GameObject markerPrefab;
        [Tooltip("Name of the canvas for 'Marker Prefab'.")] public string canvasName = "Canvas_Images";
        [Tooltip("Friend color")] public Color friendColor = Color.blue;
        [Tooltip("Enemy color")] public Color enemyColor = Color.red;
        [Tooltip("Offset from the upper edge of the screen.")] public float upperOffset = 64.0f;
        [Tooltip("Offset from the side edges of the screen.")] public float sideOffset = 64.0f;
        [Tooltip("Offset from the side edges of the screen.")] public float bottomOffset = 96.0f;


        List<ID_Control_CS> idScriptsList = new List<ID_Control_CS>();
        Dictionary<ID_Control_CS, PositionMarkerProp> markerDictionary = new Dictionary<ID_Control_CS, PositionMarkerProp>();
        Canvas canvas;
        CanvasScaler canvasScaler;
        Quaternion leftRot = Quaternion.Euler(new Vector3(0.0f, 0.0f, -90.0f));
        Quaternion rightRot = Quaternion.Euler(new Vector3(0.0f, 0.0f, 90.0f));


        void Awake()
        { // This function must be exected before "Start()", because the Canvas is required in "Receive_ID_Script()" called in "Start()".
            Initialize();
        }


        void Initialize()
        {
            // Check the prefab.
            if (markerPrefab == null)
            {
                Debug.LogWarning("'Prefab for 'Position Maker' is not assigned.");
                Destroy(this);
                return;
            }

            // Check the canvas name.
            if (string.IsNullOrEmpty(canvasName))
            {
                Debug.LogWarning("Canvas name for 'Position Maker' is not assigned.");
                Destroy(this);
                return;
            }

            // Get the canvas.
            var canvasObject = GameObject.Find(canvasName);
            if (canvasObject)
            {
                canvas = canvasObject.GetComponent<Canvas>();
            }
            if (canvas == null)
            {
                Debug.LogWarning("Canvas for 'Position Maker' cannot be found.");
                Destroy(this);
                return;
            }
            canvasScaler = canvas.GetComponent<CanvasScaler>();
        }


        void Receive_ID_Script(ID_Control_CS idScript)
        { // Called from "ID_Control_CS" in tanks in the scene, when the tank is spawned.

            // Add the "ID_Settings_CS" to the list.
            idScriptsList.Add(idScript);

            // Create a new marker object.
            var markerObject = Instantiate(markerPrefab, canvas.transform);

            // Add the components to the dictionary.
            var newProp = new PositionMarkerProp();
            newProp.markerImage = markerObject.GetComponent<Image>();
            newProp.markerTransform = newProp.markerImage.transform;
            newProp.rootTransform = idScript.transform;
            newProp.bodyTransform = idScript.bodyTransform;
            newProp.aiScript = idScript.aiScript;
            markerDictionary.Add(idScript, newProp);
        }


        void LateUpdate()
        {
            Control_Markers();
        }


        void Control_Markers()
        {
            var mainCamera = Camera.main;
            var resolutionOffset = Screen.width / canvasScaler.referenceResolution.x;

            for (int i = 0; i < idScriptsList.Count; i++)
            {
                // Check the tank is selected now, or has been dead.
                if (idScriptsList[i].isPlayer || markerDictionary[idScriptsList[i]].rootTransform.tag == "Finish")
                {
                    markerDictionary[idScriptsList[i]].markerImage.enabled = false;
                    continue;
                }

                // Set the enabled and the color, according to the relationship and the AI condition.
                switch (idScriptsList[i].relationship)
                {
                    case 0: // Friendly.
                        markerDictionary[idScriptsList[i]].markerImage.enabled = true;
                        if (markerDictionary[idScriptsList[i]].aiScript)
                        { // AI tank.
                            // Set the alpha.
                            switch (markerDictionary[idScriptsList[i]].aiScript.actionType)
                            {
                                case 0: // Defensive.
                                    friendColor.a = 0.25f;
                                    break;

                                case 1: // Offensive.
                                    friendColor.a = 1.0f;
                                    break;
                            }
                            markerDictionary[idScriptsList[i]].markerImage.color = friendColor;
                        }
                        else
                        { // Not AI tank.
                            markerDictionary[idScriptsList[i]].markerImage.enabled = true;
                            markerDictionary[idScriptsList[i]].markerImage.color = friendColor;
                        }
                        break;

                    case 1: // Hostile.
                        if (markerDictionary[idScriptsList[i]].aiScript)
                        { // AI tank.
                            // Set the alpha and the scale.
                            switch (markerDictionary[idScriptsList[i]].aiScript.actionType)
                            {
                                case 0: // Defensive.
                                    markerDictionary[idScriptsList[i]].markerImage.enabled = true;
                                    enemyColor.a = 0.25f;
                                    markerDictionary[idScriptsList[i]].markerImage.color = enemyColor;
                                    markerDictionary[idScriptsList[i]].markerTransform.localScale = Vector3.one;

                                    break;

                                case 1: // Offensive.
                                    markerDictionary[idScriptsList[i]].markerImage.enabled = true;
                                    // Set the alpha.
                                    enemyColor.a = 1.0f;
                                    markerDictionary[idScriptsList[i]].markerImage.color = enemyColor;
                                    markerDictionary[idScriptsList[i]].markerTransform.localScale = Vector3.one * 1.5f;
                                    break;
                            }
                        }
                        else
                        { // Not AI tank.
                            markerDictionary[idScriptsList[i]].markerImage.enabled = true;
                            markerDictionary[idScriptsList[i]].markerImage.color = enemyColor;
                        }
                        break;
                }
                if (markerDictionary[idScriptsList[i]].markerImage.enabled == false)
                {
                    continue;
                }

                // Calculate the position and rotation.
                var dist = Vector3.Distance(mainCamera.transform.position, markerDictionary[idScriptsList[i]].bodyTransform.position);
                var currentPos = mainCamera.WorldToScreenPoint(markerDictionary[idScriptsList[i]].bodyTransform.position);
                if (currentPos.z > 0.0f)
                { // In front of the camera.
                    currentPos.z = 100.0f;
                    if (currentPos.x < sideOffset)
                    { // Over the left end.
                        currentPos.x = sideOffset * resolutionOffset;
                        currentPos.y = Screen.height * Mathf.Lerp(0.2f, 0.9f, dist / 500.0f);
                        markerDictionary[idScriptsList[i]].markerTransform.localRotation = leftRot;
                    }
                    else if (currentPos.x > (Screen.width - sideOffset))
                    { // Over the right end.
                        currentPos.x = Screen.width - (sideOffset * resolutionOffset);
                        currentPos.y = Screen.height * Mathf.Lerp(0.2f, 0.9f, dist / 500.0f);
                        markerDictionary[idScriptsList[i]].markerTransform.localRotation = rightRot;
                    }
                    else
                    { // Within the screen.
                        currentPos.y = Screen.height - (upperOffset * resolutionOffset);
                        markerDictionary[idScriptsList[i]].markerTransform.localRotation = Quaternion.identity;
                    }
                }
                else
                { // Behind of the camera.
                    currentPos.z = -100.0f;
                    if (currentPos.x > (Screen.width - sideOffset))
                    { // Over the left end.
                        currentPos.x = sideOffset * resolutionOffset;
                        currentPos.y = Screen.height * Mathf.Lerp(0.2f, 0.9f, dist / 500.0f);
                        markerDictionary[idScriptsList[i]].markerTransform.localRotation = leftRot;
                    }
                    else if (currentPos.x < sideOffset)
                    { // Over the right end.
                        currentPos.x = Screen.width - (sideOffset * resolutionOffset);
                        currentPos.y = Screen.height * Mathf.Lerp(0.2f, 0.9f, dist / 500.0f);
                        markerDictionary[idScriptsList[i]].markerTransform.localRotation = rightRot;
                    }
                    else
                    { // Within the screen.
                        currentPos.x = Screen.width - currentPos.x;
                        currentPos.y = (bottomOffset * resolutionOffset);
                        markerDictionary[idScriptsList[i]].markerTransform.localRotation = Quaternion.identity;
                    }
                }

                // Set the position.
                markerDictionary[idScriptsList[i]].markerTransform.position = currentPos;
            }
        }


        void Remove_ID(ID_Control_CS idScript)
        { // Called from "ID_Control_CS", just before the tank is removed from the scene.

            // Destroy the marker.
            Destroy(markerDictionary[idScript].markerImage.gameObject);

            // Remove the "ID_Settings_CS" from the list.
            idScriptsList.Remove(idScript);

            // Remove the components from the dictionary.
            markerDictionary.Remove(idScript);
        }

    }

}