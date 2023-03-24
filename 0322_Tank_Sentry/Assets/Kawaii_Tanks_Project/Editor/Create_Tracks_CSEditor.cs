using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;

namespace ChobiAssets.KTP
{

    [CustomEditor(typeof(Create_Tracks_CS))]
    public class Create_Tracks_CSEditor : Editor
    {

        SerializedProperty isLeftProp;
        SerializedProperty posXProp;
        SerializedProperty widthProp;
        SerializedProperty heightProp;
        SerializedProperty matProp;
        SerializedProperty scaleProp;
        SerializedProperty lineAProp;
        SerializedProperty lineBProp;
        SerializedProperty lineCProp;
        SerializedProperty lineDProp;
        SerializedProperty lineEProp;
        SerializedProperty lineFProp;
        SerializedProperty lineGProp;
        SerializedProperty lineHProp;
        SerializedProperty guideCountProp;
        SerializedProperty guideHeightProp;
        SerializedProperty guidePositionsProp;
        SerializedProperty guideLineBottomProp;
        SerializedProperty guideLineTopProp;

        SerializedProperty positionArrayProp;
        SerializedProperty radiusArrayProp;
        SerializedProperty startAngleArrayProp;
        SerializedProperty endAngleArrayProp;

        SerializedProperty showTextureProp;

        SerializedProperty savedMeshProp;

        SerializedProperty hasChangedProp;

        Transform thisTransform;
        List<Transform> roadWheelsList;
        List<Transform> supportWheelsList;
        List<Transform> idlerSprocketWheelsList;
        List<CreatingScrollTrackInfo> wheelsInfoList;
        float snapAngle = 30.0f;
        Mesh mesh;
        int baseCount;


        void OnEnable()
        {
            isLeftProp = serializedObject.FindProperty("isLeft");
            posXProp = serializedObject.FindProperty("posX");
            widthProp = serializedObject.FindProperty("width");
            heightProp = serializedObject.FindProperty("height");
            matProp = serializedObject.FindProperty("mat");
            scaleProp = serializedObject.FindProperty("scale");
            lineAProp = serializedObject.FindProperty("lineA");
            lineBProp = serializedObject.FindProperty("lineB");
            lineCProp = serializedObject.FindProperty("lineC");
            lineDProp = serializedObject.FindProperty("lineD");
            lineEProp = serializedObject.FindProperty("lineE");
            lineFProp = serializedObject.FindProperty("lineF");
            lineGProp = serializedObject.FindProperty("lineG");
            lineHProp = serializedObject.FindProperty("lineH");
            guideCountProp = serializedObject.FindProperty("guideCount");
            guideHeightProp = serializedObject.FindProperty("guideHeight");
            guidePositionsProp = serializedObject.FindProperty("guidePositions");
            guideLineBottomProp = serializedObject.FindProperty("guideLineBottom");
            guideLineTopProp = serializedObject.FindProperty("guideLineTop");

            positionArrayProp = serializedObject.FindProperty("positionArray");
            radiusArrayProp = serializedObject.FindProperty("radiusArray");
            startAngleArrayProp = serializedObject.FindProperty("startAngleArray");
            endAngleArrayProp = serializedObject.FindProperty("endAngleArray");

            showTextureProp = serializedObject.FindProperty("showTexture");

            savedMeshProp = serializedObject.FindProperty("savedMesh");

            hasChangedProp = serializedObject.FindProperty("hasChanged");

            if (Selection.activeGameObject)
            {
                thisTransform = Selection.activeGameObject.transform;
            }
        }


        public override void OnInspectorGUI()
        {
            bool isPrepared;
            if (Application.isPlaying || thisTransform.parent == null || thisTransform.parent.gameObject.GetComponent<Rigidbody>() == null)
            {
                isPrepared = false;
            }
            else
            {
                isPrepared = true;
            }

            if (isPrepared)
            {
                // Keep posotion and rotation.
                thisTransform.localPosition = Vector3.zero;
                thisTransform.localEulerAngles = Vector3.zero;

                // Set Inspector window.
                Set_Inspector();
            }
        }


        void Set_Inspector()
        {
            // Check this is the root of the prefab or not.
            if (PrefabUtility.IsAnyPrefabInstanceRoot(Selection.activeGameObject))
            {
                GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
                EditorGUILayout.HelpBox("We need to unpack this prefab to create the track.", MessageType.Warning, true);
                if (GUILayout.Button("Unpack Prefab", GUILayout.Width(200)))
                {
                    PrefabUtility.UnpackPrefabInstance(Selection.activeGameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                }
                return;
            }

            // Check the Prefab Mode.
            if (PrefabUtility.IsPartOfPrefabInstance(thisTransform))
            {
                GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
                EditorGUILayout.HelpBox("Track can be created only in the Prefab Mode.\nPlease go to the Prefab Mode, or Unpack the prefab.", MessageType.Warning, true);
                return;
            }

            serializedObject.Update();

            GUI.backgroundColor = new Color(1.0f, 1.0f, 0.5f, 1.0f);

            EditorGUI.BeginChangeCheck();

            // Main Settings.
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Basic Settings", MessageType.None, true);
            EditorGUILayout.Space();

            Show_Message();
            EditorGUILayout.Space();

            // Get Default Settings
            if (GUILayout.Button("Get Default Settings", GUILayout.Width(200)))
            {
                Get_Default_Settings();
            }
            EditorGUILayout.Space();
            isLeftProp.boolValue = EditorGUILayout.Toggle("Left", isLeftProp.boolValue);
            EditorGUILayout.Space();
            EditorGUILayout.Slider(posXProp, 0.0f, 10.0f, "Position X");
            EditorGUILayout.Slider(widthProp, 0.1f, 3.0f, "Width");
            EditorGUILayout.Slider(heightProp, 0.01f, 0.5f, "Thickness");
            EditorGUILayout.Space();
            EditorGUILayout.IntSlider(guideCountProp, 0, 4, "Number of Guides");
            if (guideCountProp.intValue != 0)
            {
                EditorGUILayout.Slider(guideHeightProp, 0.01f, 0.5f, "Guide height");
                guidePositionsProp.arraySize = guideCountProp.intValue;
                for (int i = 0; i < guideCountProp.intValue; i++)
                {
                    EditorGUILayout.Slider(guidePositionsProp.GetArrayElementAtIndex(i), -widthProp.floatValue / 2.0f, widthProp.floatValue / 2.0f, "Guide [" + i + "] Position");
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // Shape Settings.
            EditorGUILayout.HelpBox("Shape Settings", MessageType.None, true);
            for (int i = 0; i < radiusArrayProp.arraySize; i++)
            {
                EditorGUILayout.HelpBox("Wheel [" + i + "]", MessageType.None, true);
                EditorGUILayout.Slider(radiusArrayProp.GetArrayElementAtIndex(i), 0.0f, 1.0f, "Radius");
                EditorGUILayout.Slider(startAngleArrayProp.GetArrayElementAtIndex(i), -360.0f, 360.0f, "Start Angle");
                EditorGUILayout.Slider(endAngleArrayProp.GetArrayElementAtIndex(i), -360.0f, 360.0f, "End Angle");
                startAngleArrayProp.GetArrayElementAtIndex(i).floatValue = Mathf.Ceil(startAngleArrayProp.GetArrayElementAtIndex(i).floatValue / snapAngle) * snapAngle;
                endAngleArrayProp.GetArrayElementAtIndex(i).floatValue = Mathf.Ceil(endAngleArrayProp.GetArrayElementAtIndex(i).floatValue / snapAngle) * snapAngle;
                EditorGUILayout.Space();
            }

            // UV Settings.
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("UV Settings", MessageType.None, true);
            matProp.objectReferenceValue = EditorGUILayout.ObjectField("Material", matProp.objectReferenceValue, typeof(Material), false);
            if (matProp.objectReferenceValue == null)
            {
                MeshRenderer thisRenderer = thisTransform.GetComponent<MeshRenderer>();
                if (thisRenderer)
                {
                    matProp.objectReferenceValue = thisRenderer.sharedMaterial as Material;
                }
            }
            EditorGUILayout.Space();
            if (EditorGUI.EndChangeCheck())
            {
                Create();
            }
            showTextureProp.boolValue = EditorGUILayout.Toggle("Show Texture", showTextureProp.boolValue);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Slider(scaleProp, -1.0f, 1.0f, "Scale");
            EditorGUILayout.Space();
            EditorGUILayout.Slider(lineAProp, 0.0f, 1.0f, "Line A (Inner of Inside)");
            EditorGUILayout.Slider(lineBProp, 0.0f, 1.0f, "Line B (Outer of Inside)");
            EditorGUILayout.Space();
            EditorGUILayout.Slider(lineCProp, 0.0f, 1.0f, "Line C (Inner of Outer-Surface)");
            EditorGUILayout.Slider(lineDProp, 0.0f, 1.0f, "Line D (Outer of Outer-Surface)");
            EditorGUILayout.Space();
            EditorGUILayout.Slider(lineEProp, 0.0f, 1.0f, "Line E (Outer of Outside)");
            EditorGUILayout.Slider(lineFProp, 0.0f, 1.0f, "Line F (Inner of Outside)");
            EditorGUILayout.Space();
            EditorGUILayout.Slider(lineGProp, 0.0f, 1.0f, "Line G (Outer of Inner-Surface)");
            EditorGUILayout.Slider(lineHProp, 0.0f, 1.0f, "Line H (Inner of Inner-Surface)");
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            if (guideCountProp.intValue != 0)
            {
                EditorGUILayout.Slider(guideLineTopProp, 0.0f, 1.0f, "Line (Top of Guide)");
                EditorGUILayout.Slider(guideLineBottomProp, 0.0f, 1.0f, "Line (Bottom of Guide)");
            }

            if (EditorGUI.EndChangeCheck())
            {
                Create();
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            Show_Message();

            EditorGUILayout.Space();
            GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
            if (GUILayout.Button("Save as a New Mesh", GUILayout.Width(200)))
            {
                Create_New_Mesh();
            }
            EditorGUILayout.Space();

            if (savedMeshProp.objectReferenceValue)
            {
                if (GUILayout.Button("Overwrite Saved Mesh", GUILayout.Width(200)))
                {
                    Overwrite_Saved_Mesh();
                }
            }

            EditorGUI.indentLevel++;
            savedMeshProp.objectReferenceValue = EditorGUILayout.ObjectField("Saved Mesh", savedMeshProp.objectReferenceValue, typeof(Mesh), false);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            if (GUI.changed)
            {
                hasChangedProp.boolValue = !hasChangedProp.boolValue;
            }

            if (Event.current.commandName == "UndoRedoPerformed")
            {
                Debug.LogWarning("Please save the mesh again after using Undo or Redo.");
                Create();
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
        }


        void Show_Message()
        {
            MeshFilter thisMeshFilter = thisTransform.GetComponent<MeshFilter>();
            if (thisMeshFilter && thisMeshFilter.sharedMesh)
            {
                if (string.IsNullOrEmpty(thisMeshFilter.sharedMesh.name))
                {
                    GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
                    EditorGUILayout.HelpBox("The mesh is not saved yet.", MessageType.None, true);
                    GUI.backgroundColor = new Color(1.0f, 1.0f, 0.5f, 1.0f);
                }
                else
                {
                    EditorGUILayout.HelpBox("The mesh is saved as '" + thisMeshFilter.sharedMesh.name + "'", MessageType.None, true);
                }
            }
            else
            {
                GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
                EditorGUILayout.HelpBox("The mesh is not created yet.", MessageType.None, true);
                GUI.backgroundColor = new Color(1.0f, 1.0f, 0.5f, 1.0f);
            }
        }


        void Create()
        {
            thisTransform.localPosition = Vector3.zero;

            // Check the wheels.
            if (radiusArrayProp.arraySize == 0)
            {
                return;
            }

            // Set MeshFilter and MeshRenderer.
            MeshFilter thisFilter = thisTransform.GetComponent<MeshFilter>();
            if (thisFilter == null)
            {
                thisFilter = thisTransform.gameObject.AddComponent<MeshFilter>();
            }
            MeshRenderer thisRenderer = thisTransform.GetComponent<MeshRenderer>();
            if (thisRenderer == null)
            {
                thisRenderer = thisTransform.gameObject.AddComponent<MeshRenderer>();
            }
            thisRenderer.material = matProp.objectReferenceValue as Material;
            
            // Create Mesh.
            mesh = new Mesh();
            Set_Vertices();
            Set_Triangles();
            Set_UV();
            mesh.RecalculateNormals();

            //TangentSolver (ref mesh);
            Set_Tangent();

            thisFilter.sharedMesh = mesh;
        }


        void Set_Vertices()
        {
            List<Vector3> verticesList = new List<Vector3>();
            float xSign;
            if (isLeftProp.boolValue)
            { // Left
                xSign = -1.0f;
            }
            else
            { // Right
                xSign = 1.0f;
            }
            // Outer Line of Inner-Surface.
            for (int i = 0; i < positionArrayProp.arraySize; i++)
            {
                float deltaAngle = Mathf.Abs(startAngleArrayProp.GetArrayElementAtIndex(i).floatValue - endAngleArrayProp.GetArrayElementAtIndex(i).floatValue);
                float tempSign = Mathf.Sign(endAngleArrayProp.GetArrayElementAtIndex(i).floatValue - startAngleArrayProp.GetArrayElementAtIndex(i).floatValue);
                int cornerCount = (int)(deltaAngle / snapAngle + 1.0f);
                for (int j = 0; j < cornerCount; j++)
                {
                    float radius = radiusArrayProp.GetArrayElementAtIndex(i).floatValue;
                    Vector3 tempPos;
                    tempPos.x = (posXProp.floatValue * xSign) + ((widthProp.floatValue / 2.0f) * xSign);
                    tempPos.y = Mathf.Sin(Mathf.Deg2Rad * (90.0f + startAngleArrayProp.GetArrayElementAtIndex(i).floatValue + (snapAngle * j * tempSign)));
                    tempPos.y *= radius;
                    tempPos.y += positionArrayProp.GetArrayElementAtIndex(i).vector3Value.y;
                    tempPos.z = Mathf.Cos(Mathf.Deg2Rad * (90.0f + startAngleArrayProp.GetArrayElementAtIndex(i).floatValue + (snapAngle * j * tempSign)));
                    tempPos.z *= radius;
                    tempPos.z += positionArrayProp.GetArrayElementAtIndex(i).vector3Value.z;
                    verticesList.Add(tempPos);
                }
            }
            verticesList.Add(verticesList[0]); // Last one.
            baseCount = verticesList.Count;
            // Inner Line of Inner-Surface.
            for (int i = 0; i < baseCount; i++)
            {
                Vector3 tempPos = verticesList[i];
                tempPos.x -= widthProp.floatValue * xSign;
                verticesList.Add(tempPos);
            }
            // Outer Line of Outer-Surface.
            for (int i = 0; i < baseCount - 1; i++)
            {
                Vector3 previousPos;
                if (i == 0)
                {
                    previousPos = verticesList[baseCount - 2];
                }
                else
                {
                    previousPos = verticesList[i - 1];
                }
                Vector3 currentPos = verticesList[i];
                Vector3 nextPos = verticesList[i + 1];
                float previousAng = Mathf.Atan2(previousPos.y - currentPos.y, previousPos.z - currentPos.z) * Mathf.Rad2Deg;
                float nextAng = Mathf.Atan2(nextPos.y - currentPos.y, nextPos.z - currentPos.z) * Mathf.Rad2Deg;
                if (previousAng > nextAng)
                {
                    nextAng = 360.0f + nextAng;
                }
                float targetAng = (nextAng + previousAng) / 2.0f;
                Vector3 tempPos;
                tempPos.x = (posXProp.floatValue * xSign) + ((widthProp.floatValue / 2.0f) * xSign);
                tempPos.y = Mathf.Sin(Mathf.Deg2Rad * targetAng) * heightProp.floatValue;
                tempPos.y += currentPos.y;
                tempPos.z = Mathf.Cos(Mathf.Deg2Rad * targetAng) * heightProp.floatValue;
                tempPos.z += currentPos.z;
                verticesList.Add(tempPos);
            }
            verticesList.Add(verticesList[baseCount * 2]); // Last one.
                                                           // Inner Line of Outer-Surface.
            for (int i = 0; i < baseCount; i++)
            {
                Vector3 tempPos = verticesList[(baseCount * 2) + i];
                tempPos.x -= widthProp.floatValue * xSign;
                verticesList.Add(tempPos);
            }
            // Outer Side.
            for (int i = 0; i < baseCount; i++)
            {
                verticesList.Add(verticesList[i]);
            }
            for (int i = 0; i < baseCount; i++)
            {
                verticesList.Add(verticesList[(baseCount * 2) + i]);
            }
            // Inner Side.
            for (int i = 0; i < baseCount; i++)
            {
                verticesList.Add(verticesList[baseCount + i]);
            }
            for (int i = 0; i < baseCount; i++)
            {
                verticesList.Add(verticesList[(baseCount * 3) + i]);
            }
            // Center-Guide
            for (int i = 0; i < guideCountProp.intValue; i++)
            {
                for (int j = 0; j < baseCount; j++)
                {
                    Vector3 tempPos = verticesList[j];
                    tempPos.x = (posXProp.floatValue * xSign) + (guidePositionsProp.GetArrayElementAtIndex(i).floatValue * xSign);
                    verticesList.Add(tempPos);
                }
                for (int j = 0; j < baseCount - 1; j++)
                {
                    Vector3 previousPos;
                    if (j == 0)
                    {
                        previousPos = verticesList[baseCount - 2];
                    }
                    else
                    {
                        previousPos = verticesList[j - 1];
                    }
                    Vector3 currentPos = verticesList[j];
                    Vector3 nextPos = verticesList[j + 1];
                    float previousAng = Mathf.Atan2(previousPos.y - currentPos.y, previousPos.z - currentPos.z) * Mathf.Rad2Deg;
                    float nextAng = Mathf.Atan2(nextPos.y - currentPos.y, nextPos.z - currentPos.z) * Mathf.Rad2Deg;
                    if (previousAng > nextAng)
                    {
                        nextAng = 360.0f + nextAng;
                    }
                    float targetAng = (nextAng + previousAng) / 2.0f;
                    Vector3 tempPos;
                    tempPos.x = (posXProp.floatValue * xSign) + (guidePositionsProp.GetArrayElementAtIndex(i).floatValue * xSign);
                    tempPos.y = Mathf.Sin(Mathf.Deg2Rad * targetAng) * -guideHeightProp.floatValue;
                    tempPos.y += currentPos.y;
                    tempPos.z = Mathf.Cos(Mathf.Deg2Rad * targetAng) * -guideHeightProp.floatValue;
                    tempPos.z += currentPos.z;
                    verticesList.Add(tempPos);
                }
                verticesList.Add(verticesList[baseCount * (9 + (i * 2))]); // Last one.
            }

            //	Set Vertices.
            mesh.vertices = verticesList.ToArray();
        }


        void Set_Triangles()
        {
            List<int> trianglesList = new List<int>();
            // Inner-Surface.
            for (int i = 0; i < baseCount - 1; i++)
            {
                trianglesList.Add(i);
                if (isLeftProp.boolValue)
                {
                    trianglesList.Add(i + 1);
                    trianglesList.Add(baseCount + i);
                }
                else
                {
                    trianglesList.Add(baseCount + i);
                    trianglesList.Add(i + 1);
                }
            }
            for (int i = 0; i < baseCount - 1; i++)
            {
                trianglesList.Add(baseCount + i);
                if (isLeftProp.boolValue)
                {
                    trianglesList.Add(i + 1);
                    trianglesList.Add(baseCount + i + 1);
                }
                else
                {
                    trianglesList.Add(baseCount + i + 1);
                    trianglesList.Add(i + 1);
                }
            }
            // Outer-Surface.
            for (int i = 0; i < baseCount - 1; i++)
            {
                trianglesList.Add((baseCount * 2) + i);
                if (isLeftProp.boolValue)
                {
                    trianglesList.Add((baseCount * 3) + i);
                    trianglesList.Add((baseCount * 2) + i + 1);
                }
                else
                {
                    trianglesList.Add((baseCount * 2) + i + 1);
                    trianglesList.Add((baseCount * 3) + i);
                }
            }
            for (int i = 0; i < baseCount - 1; i++)
            {
                trianglesList.Add((baseCount * 2) + i + 1);
                if (isLeftProp.boolValue)
                {
                    trianglesList.Add((baseCount * 3) + i);
                    trianglesList.Add((baseCount * 3) + i + 1);
                }
                else
                {
                    trianglesList.Add((baseCount * 3) + i + 1);
                    trianglesList.Add((baseCount * 3) + i);
                }
            }
            // Outer Side.
            for (int i = 0; i < baseCount - 1; i++)
            {
                trianglesList.Add((baseCount * 4) + i);
                if (isLeftProp.boolValue)
                {
                    trianglesList.Add((baseCount * 5) + i);
                    trianglesList.Add((baseCount * 4) + i + 1);
                }
                else
                {
                    trianglesList.Add((baseCount * 4) + i + 1);
                    trianglesList.Add((baseCount * 5) + i);
                }
            }
            for (int i = 0; i < baseCount - 1; i++)
            {
                trianglesList.Add((baseCount * 4) + i + 1);
                if (isLeftProp.boolValue)
                {
                    trianglesList.Add((baseCount * 5) + i);
                    trianglesList.Add((baseCount * 5) + i + 1);
                }
                else
                {
                    trianglesList.Add((baseCount * 5) + i + 1);
                    trianglesList.Add((baseCount * 5) + i);
                }
            }
            // Inner Side.
            for (int i = 0; i < baseCount - 1; i++)
            {
                trianglesList.Add((baseCount * 6) + i);
                if (isLeftProp.boolValue)
                {
                    trianglesList.Add((baseCount * 6) + i + 1);
                    trianglesList.Add((baseCount * 7) + i);
                }
                else
                {
                    trianglesList.Add((baseCount * 7) + i);
                    trianglesList.Add((baseCount * 6) + i + 1);
                }
            }
            for (int i = 0; i < baseCount - 1; i++)
            {
                trianglesList.Add((baseCount * 6) + i + 1);
                if (isLeftProp.boolValue)
                {
                    trianglesList.Add((baseCount * 7) + i + 1);
                    trianglesList.Add((baseCount * 7) + i);
                }
                else
                {
                    trianglesList.Add((baseCount * 7) + i);
                    trianglesList.Add((baseCount * 7) + i + 1);
                }
            }
            // Center-Guide
            for (int i = 0; i < guideCountProp.intValue; i++)
            {
                for (int j = 0; j < baseCount - 1; j++)
                {
                    trianglesList.Add((baseCount * (8 + (i * 2))) + j);
                    if (isLeftProp.boolValue)
                    {
                        trianglesList.Add((baseCount * (8 + (i * 2))) + j + 1);
                        trianglesList.Add((baseCount * (9 + (i * 2))) + j);
                    }
                    else
                    {
                        trianglesList.Add((baseCount * (9 + (i * 2))) + j);
                        trianglesList.Add((baseCount * (8 + (i * 2))) + j + 1);
                    }
                }
                for (int j = 0; j < baseCount - 1; j++)
                {
                    trianglesList.Add((baseCount * (8 + (i * 2))) + j + 1);
                    if (isLeftProp.boolValue)
                    {
                        trianglesList.Add((baseCount * (9 + (i * 2))) + j + 1);
                        trianglesList.Add((baseCount * (9 + (i * 2))) + j);
                    }
                    else
                    {
                        trianglesList.Add((baseCount * (9 + (i * 2))) + j);
                        trianglesList.Add((baseCount * (9 + (i * 2))) + j + 1);
                    }
                }
            }
            // Set Triangles.
            mesh.triangles = trianglesList.ToArray();
        }


        void Set_UV()
        {
            List<Vector2> uvList = new List<Vector2>();
            List<float> vList = new List<float>() {
                lineGProp.floatValue,
                lineHProp.floatValue,
                lineDProp.floatValue,
                lineCProp.floatValue,
                lineFProp.floatValue,
                lineEProp.floatValue,
                lineAProp.floatValue,
                lineBProp.floatValue,
            };
            for (int i = 0; i < guideCountProp.intValue; i++)
            {
                vList.Add(guideLineBottomProp.floatValue);
                vList.Add(guideLineTopProp.floatValue);
            }
            for (int i = 0; i < vList.Count; i++)
            {
                float currentU = 0.0f;
                for (int j = 0; j < baseCount; j++)
                {
                    float dist;
                    if (j == 0)
                    {
                        dist = 0.0f;
                    }
                    else
                    {
                        dist = Vector3.Distance(mesh.vertices[j - 1], mesh.vertices[j]);
                    }
                    currentU += dist * scaleProp.floatValue;
                    Vector2 tempUV = new Vector2(currentU, vList[i]);
                    uvList.Add(tempUV);
                }
            }
            // Set UV.
            mesh.uv = uvList.ToArray();
        }


        void Set_Tangent()
        {
            List<Vector4> tangentList = new List<Vector4>();
            float tempW;
            if (isLeftProp.boolValue)
            {
                tempW = -1.0f;
            }
            else
            {
                tempW = 1.0f;
            }
            // Inner-Surface
            for (int i = 0; i < (baseCount * 2); i++)
            {
                Vector3 tempTangent = Vector3.Cross(mesh.normals[i], Vector3.left);
                tangentList.Add(new Vector4(tempTangent.x, tempTangent.y, tempTangent.z, tempW));
            }
            // Outer-Surface
            for (int i = (baseCount * 2); i < (baseCount * 4); i++)
            {
                Vector3 tempTangent = Vector3.Cross(mesh.normals[i], Vector3.right);
                tangentList.Add(new Vector4(tempTangent.x, tempTangent.y, tempTangent.z, tempW));
            }
            // Outer Side
            for (int i = (baseCount * 4); i < (baseCount * 5); i++)
            {
                tangentList.Add(tangentList[i - (baseCount * 4)]);
            }
            for (int i = (baseCount * 5); i < (baseCount * 6); i++)
            {
                tangentList.Add(tangentList[i - (baseCount * 3)]);
            }
            // Inner Side
            for (int i = (baseCount * 6); i < (baseCount * 7); i++)
            {
                tangentList.Add(tangentList[i - (baseCount * 5)]);
            }
            for (int i = (baseCount * 7); i < (baseCount * 8); i++)
            {
                tangentList.Add(tangentList[i - (baseCount * 4)]);
            }
            // Center-Guide
            for (int i = 0; i < guideCountProp.intValue; i++)
            {
                for (int j = (baseCount * (8 + (i * 2))); j < (baseCount * (10 + (i * 2))); j++)
                {
                    tangentList.Add(tangentList[j - (baseCount * (8 + (i * 2)))]);
                }
            }
            // Set Tangents.
            mesh.SetTangents(tangentList);
        }


        void Get_Default_Settings()
        {
            Create_RoadWheelsList();
            Create_SupportWheelsLisT();
            Create_IdlerSprocketWheelsList();
            Create_WheelsInfoList();
            Set_Arrays();
        }


        void Create_RoadWheelsList()
        {
            // Find RoadWheels.
            roadWheelsList = new List<Transform>();
            List<Transform> tempList = new List<Transform>();
            Transform bodyTransform = thisTransform.parent;
            Wheel_Rotate_CS[] wheelScripts = bodyTransform.GetComponentsInChildren<Wheel_Rotate_CS>();
            foreach (Wheel_Rotate_CS driveScript in wheelScripts)
            {
                Transform connectedTransform = driveScript.GetComponent<HingeJoint>().connectedBody.transform;
                MeshFilter meshFilter = driveScript.GetComponent<MeshFilter>();
                if (connectedTransform != bodyTransform && meshFilter && meshFilter.sharedMesh && driveScript.transform.localEulerAngles.z == 0.0f)
                { // connected to Suspension && visible && Left.
                    tempList.Add(driveScript.transform);
                }
            }
            // Sort (rear >> front)
            int tempCount = tempList.Count;
            for (int i = 0; i < tempCount; i++)
            {
                float maxPosZ = Mathf.Infinity;
                int targetIndex = 0;
                for (int j = 0; j < tempList.Count; j++)
                {
                    if (tempList[j].position.z < maxPosZ)
                    {
                        maxPosZ = tempList[j].position.z;
                        targetIndex = j;
                    }
                }
                roadWheelsList.Add(tempList[targetIndex]);
                tempList.RemoveAt(targetIndex);
            }
        }


        void Create_SupportWheelsLisT()
        {
            // Find SupportWheels.
            supportWheelsList = new List<Transform>();
            List<Transform> tempList = new List<Transform>();
            Create_SupportWheels_CS[] createScripts = thisTransform.parent.GetComponentsInChildren<Create_SupportWheels_CS>();
            foreach (Create_SupportWheels_CS createScript in createScripts)
            {
                MeshFilter[] tempFilters = createScript.GetComponentsInChildren<MeshFilter>();
                foreach (MeshFilter tempFilter in tempFilters)
                {
                    if (tempFilter.gameObject.layer == 9 && tempFilter.transform.localEulerAngles.z == 0.0f && tempFilter.sharedMesh)
                    { // Layer 9 == Wheel, && Left.
                        tempList.Add(tempFilter.transform);
                    }
                }
            }
            // Sort (front >> rear)
            int tempCount = tempList.Count;
            for (int i = 0; i < tempCount; i++)
            {
                float minPosZ = -Mathf.Infinity;
                int targetIndex = 0;
                for (int j = 0; j < tempList.Count; j++)
                {
                    if (tempList[j].position.z > minPosZ)
                    {
                        minPosZ = tempList[j].position.z;
                        targetIndex = j;
                    }
                }
                supportWheelsList.Add(tempList[targetIndex]);
                tempList.RemoveAt(targetIndex);
            }
        }


        void Create_IdlerSprocketWheelsList()
        {
            idlerSprocketWheelsList = new List<Transform>();
            List<Transform> tempList = new List<Transform>();
            // Find IdlerWheels and SprocketWheels.
            Create_SprocketWheels_CS[] createScripts = thisTransform.parent.GetComponentsInChildren<Create_SprocketWheels_CS>();
            foreach (Create_SprocketWheels_CS createScript in createScripts)
            {
                MeshFilter[] tempFilters = createScript.GetComponentsInChildren<MeshFilter>();
                foreach (MeshFilter tempFilter in tempFilters)
                {
                    if (tempFilter.gameObject.layer == 9 && tempFilter.transform.localEulerAngles.z == 0.0f && tempFilter.sharedMesh)
                    { // Layer 9 == Wheel, && Left.
                        tempList.Add(tempFilter.transform);
                        break;
                    }
                }
            }
            // Sort (front >> rear)
            int tempCount = tempList.Count;
            if (tempCount != 2)
            {
                Debug.LogError("SprocketWheel and IdlerWheel cannot be found.");
                return;
            }
            for (int i = 0; i < tempCount; i++)
            {
                float minPosZ = -Mathf.Infinity;
                int targetIndex = 0;
                for (int j = 0; j < tempList.Count; j++)
                {
                    if (tempList[j].position.z > minPosZ)
                    {
                        minPosZ = tempList[j].position.z;
                        targetIndex = j;
                    }
                }
                idlerSprocketWheelsList.Add(tempList[targetIndex]);
                tempList.RemoveAt(targetIndex);
            }
        }


        void Create_WheelsInfoList()
        {
            wheelsInfoList = new List<CreatingScrollTrackInfo>();
            // Add RoadWheels.
            for (int i = 0; i < roadWheelsList.Count; i++)
            {
                CreatingScrollTrackInfo tempInfo = new CreatingScrollTrackInfo();
                tempInfo.position = roadWheelsList[i].position;
                tempInfo.radius = roadWheelsList[i].GetComponent<MeshFilter>().sharedMesh.bounds.extents.x;
                tempInfo.startAngle = 180.0f;
                tempInfo.endAngle = 180.0f;
                wheelsInfoList.Add(tempInfo);
            }
            // Add Front Wheel (Sprocket or Idler).
            CreatingScrollTrackInfo tempFrontInfo = new CreatingScrollTrackInfo();
            tempFrontInfo.position = idlerSprocketWheelsList[0].position;
            tempFrontInfo.radius = idlerSprocketWheelsList[0].GetComponent<MeshFilter>().sharedMesh.bounds.extents.x;
            tempFrontInfo.startAngle = -180.0f;
            tempFrontInfo.endAngle = 0.0f;
            wheelsInfoList.Add(tempFrontInfo);
            // Add SupportWheels.
            if (supportWheelsList.Count != 0)
            { // This tank has SupportWheels.
                for (int i = 0; i < supportWheelsList.Count; i++)
                {
                    CreatingScrollTrackInfo tempInfo = new CreatingScrollTrackInfo();
                    tempInfo.position = supportWheelsList[i].position;
                    tempInfo.radius = supportWheelsList[i].GetComponent<MeshFilter>().sharedMesh.bounds.extents.x;
                    tempInfo.startAngle = 0.0f;
                    tempInfo.endAngle = 0.0f;
                    wheelsInfoList.Add(tempInfo);
                }
            }
            else
            { // No SupportWheel >> Add RoadWheels (front >> rear).
                for (int i = (roadWheelsList.Count - 1); i >= 0; i--)
                {
                    CreatingScrollTrackInfo tempInfo = new CreatingScrollTrackInfo();
                    tempInfo.position = roadWheelsList[i].position;
                    tempInfo.radius = roadWheelsList[i].GetComponent<MeshFilter>().sharedMesh.bounds.extents.x;
                    tempInfo.startAngle = 0.0f;
                    tempInfo.endAngle = 0.0f;
                    wheelsInfoList.Add(tempInfo);
                }
            }
            // Add Rear Wheel (Sprocket or Idler).
            CreatingScrollTrackInfo tempRearInfo = new CreatingScrollTrackInfo();
            tempRearInfo.position = idlerSprocketWheelsList[1].position;
            tempRearInfo.radius = idlerSprocketWheelsList[1].GetComponent<MeshFilter>().sharedMesh.bounds.extents.x;
            tempRearInfo.startAngle = 0.0f;
            tempRearInfo.endAngle = 180.0f;
            wheelsInfoList.Add(tempRearInfo);
        }


        void Set_Arrays()
        {
            positionArrayProp.arraySize = wheelsInfoList.Count;
            radiusArrayProp.arraySize = wheelsInfoList.Count;
            startAngleArrayProp.arraySize = wheelsInfoList.Count;
            endAngleArrayProp.arraySize = wheelsInfoList.Count;
            for (int i = 0; i < wheelsInfoList.Count; i++)
            {
                positionArrayProp.GetArrayElementAtIndex(i).vector3Value = wheelsInfoList[i].position;
                radiusArrayProp.GetArrayElementAtIndex(i).floatValue = wheelsInfoList[i].radius;
                startAngleArrayProp.GetArrayElementAtIndex(i).floatValue = wheelsInfoList[i].startAngle;
                endAngleArrayProp.GetArrayElementAtIndex(i).floatValue = wheelsInfoList[i].endAngle;
            }
        }


        void Create_New_Mesh()
        {
            // Check the Prefab Mode.
            if (PrefabUtility.IsPartOfPrefabInstance(thisTransform))
            {
                Debug.LogWarning("The track can be created only in the Prefab Mode. Please go to the Prefab Mode, or Unpack the prefab.");
                return;
            }

            Create();

            // Check the mesh.
            MeshFilter thisMeshFilter = thisTransform.GetComponent<MeshFilter>();
            Mesh tempMesh = thisMeshFilter.sharedMesh;
            if (tempMesh == null)
            {
                Debug.LogWarning("Mesh does not created yet.");
                return;
            }

            // Check the "User" folder.
            if (Directory.Exists("Assets/Kawaii_Tanks_Project/User") == false)
            {
                AssetDatabase.CreateFolder("Assets/Kawaii_Tanks_Project", "User");
            }

            // Create a new path.
            string newPath;
            if (isLeftProp.boolValue)
            { // Left
                newPath = "Assets/Kawaii_Tanks_Project/User/" + thisTransform.root.name + "_Track_L " + DateTime.Now.ToString("yyMMdd_HHmmss") + ".asset";
            }
            else
            { // Right
                newPath = "Assets/Kawaii_Tanks_Project/User/" + thisTransform.root.name + "_Track_R " + DateTime.Now.ToString("yyMMdd_HHmmss") + ".asset";
            }

            // Create the new asset.
            AssetDatabase.CreateAsset(tempMesh, newPath);

            // Set the new mesh to the MeshFilter.
            savedMeshProp.objectReferenceValue = AssetDatabase.LoadAssetAtPath(newPath, typeof(Mesh)) as Mesh;
            thisMeshFilter.sharedMesh = savedMeshProp.objectReferenceValue as Mesh;

            Debug.Log("New mesh has been created in 'User' folder.");
        }


        void Overwrite_Saved_Mesh()
        {
            // Check the Prefab Mode.
            if (PrefabUtility.IsPartOfPrefabInstance(thisTransform))
            {
                Debug.LogWarning("The track can be created only in the Prefab Mode. Please go to the Prefab Mode, or Unpack the prefab.");
                return;
            }

            Create();

            // Check the mesh.
            MeshFilter thisMeshFilter = thisTransform.GetComponent<MeshFilter>();
            Mesh tempMesh = thisMeshFilter.sharedMesh;
            if (tempMesh == null)
            {
                Debug.LogWarning("Mesh does not created yet.");
                return;
            }

            // Get the path of the "Saved_Mesh".
            string savedMeshPath = AssetDatabase.GetAssetPath(savedMeshProp.objectReferenceValue);

            // Overwrite the "Saved_Mesh".
            AssetDatabase.CreateAsset(tempMesh, savedMeshPath);

            // Set the new mesh to the MeshFilter.
            savedMeshProp.objectReferenceValue = AssetDatabase.LoadAssetAtPath(savedMeshPath, typeof(Mesh)) as Mesh;
            thisMeshFilter.sharedMesh = savedMeshProp.objectReferenceValue as Mesh;

            Debug.Log("'Saved Mesh' has been overwritten.");
        }


        void OnSceneGUI()
        {
            MeshFilter thisFilter = thisTransform.GetComponent<MeshFilter>();
            if (thisFilter && thisFilter.sharedMesh)
            {
                // Draw vertices.
                Handles.matrix = Matrix4x4.TRS(Vector3.zero, thisTransform.rotation, thisTransform.localScale);
                Vector3[] vertices = thisFilter.sharedMesh.vertices;
                for (int i = 0; i < vertices.Length; i++)
                {
                    Handles.color = Color.red;
                    Handles.DrawWireCube(vertices[i] + thisTransform.position, Vector3.one * 0.05f);
                }
                // Draw UV lines.
                if (showTextureProp.boolValue)
                {
                    Texture tex = thisTransform.GetComponent<MeshRenderer>().sharedMaterial.mainTexture;
                    float size = 4.0f;
                    Graphics.DrawTexture(new Rect(size / 2.0f, size / 2.0f, -size, -size), tex);
                    float[] heightArray = {
                        lineAProp.floatValue,
                        lineBProp.floatValue,
                        lineCProp.floatValue,
                        lineDProp.floatValue,
                        lineEProp.floatValue,
                        lineFProp.floatValue,
                        lineGProp.floatValue,
                        lineHProp.floatValue,
                        guideLineTopProp.floatValue,
                        guideLineBottomProp.floatValue
                    };
                    Handles.color = Color.yellow;
                    for (int i = 0; i < heightArray.Length; i++)
                    {
                        Vector3 tempPos = new Vector3(size / 2.0f, -size / 2.0f + (heightArray[i] * size), 0.0f);
                        Handles.DrawLine(tempPos, tempPos + Vector3.left * size);
                    }
                }
            }
        }
    }

}