using UnityEngine;
using System.Collections;
using UnityEditor;

namespace ChobiAssets.KTP
{

    [CustomEditor(typeof(Create_SupportWheels_CS))]
    public class Create_SupportWheels_CSEditor : Editor
    {

        SerializedProperty wheelDistanceProp;
        SerializedProperty numProp;
        SerializedProperty spacingProp;
        SerializedProperty wheelMassProp;
        SerializedProperty wheelMeshProp;
        SerializedProperty wheelMaterialProp;
        SerializedProperty wheelMaterialsNumProp;
        SerializedProperty wheelMaterialsProp;
        SerializedProperty radiusOffsetProp;

        SerializedProperty hasChangedProp;

        Transform thisTransform;


        void OnEnable()
        {
            wheelDistanceProp = serializedObject.FindProperty("wheelDistance");
            numProp = serializedObject.FindProperty("num");
            spacingProp = serializedObject.FindProperty("spacing");
            wheelMeshProp = serializedObject.FindProperty("wheelMesh");
            wheelMaterialProp = serializedObject.FindProperty("wheelMaterial");
            wheelMaterialsNumProp = serializedObject.FindProperty("wheelMaterialsNum");
            wheelMaterialsProp = serializedObject.FindProperty("wheelMaterials");
            radiusOffsetProp = serializedObject.FindProperty("radiusOffset");

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
                // Keep rotation.
                Vector3 localAngles = thisTransform.localEulerAngles;
                localAngles.z = 90.0f;
                thisTransform.localEulerAngles = localAngles;

                // Set Inspector window.
                Set_Inspector();
            }
        }


        void Set_Inspector()
        {
            // Check the Prefab Mode.
            if (PrefabUtility.IsPartOfPrefabInstance(thisTransform))
            {
                GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
                EditorGUILayout.HelpBox("\n'The wheels can be modified only in the Prefab Mode.\nPlease go to the Prefab Mode, or Unpack the prefab.\n", MessageType.Warning, true);
                return;
            }

            serializedObject.Update();

            GUI.backgroundColor = new Color(1.0f, 1.0f, 0.5f, 1.0f);

            // Wheels settings
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Wheels settings", MessageType.None, true);
            EditorGUILayout.Slider(wheelDistanceProp, 0.1f, 10.0f, "Distance");
            EditorGUILayout.IntSlider(numProp, 0, 30, "Number");
            EditorGUILayout.Slider(spacingProp, 0.1f, 10.0f, "Spacing");
            EditorGUILayout.Slider(radiusOffsetProp, -0.5f, 0.5f, "Radius Offset");
            EditorGUILayout.Space();
            wheelMeshProp.objectReferenceValue = EditorGUILayout.ObjectField("Mesh", wheelMeshProp.objectReferenceValue, typeof(Mesh), false);
            EditorGUILayout.IntSlider(wheelMaterialsNumProp, 1, 10, "Number of Materials");
            wheelMaterialsProp.arraySize = wheelMaterialsNumProp.intValue;
            if (wheelMaterialsNumProp.intValue == 1 && wheelMaterialProp.objectReferenceValue != null)
            {
                if (wheelMaterialsProp.GetArrayElementAtIndex(0).objectReferenceValue == null)
                {
                    wheelMaterialsProp.GetArrayElementAtIndex(0).objectReferenceValue = wheelMaterialProp.objectReferenceValue;
                }
                wheelMaterialProp.objectReferenceValue = null;
            }
            EditorGUI.indentLevel++;
            for (int i = 0; i < wheelMaterialsNumProp.intValue; i++)
            {
                wheelMaterialsProp.GetArrayElementAtIndex(i).objectReferenceValue = EditorGUILayout.ObjectField("Material " + "(" + i + ")", wheelMaterialsProp.GetArrayElementAtIndex(i).objectReferenceValue, typeof(Material), false);
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            // Update Value
            if (GUI.changed || GUILayout.Button("Update Values") || Event.current.commandName == "UndoRedoPerformed")
            {
                hasChangedProp.boolValue = !hasChangedProp.boolValue;
                Create();
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
        }


        void Create()
        {
            // Delete the old objects.
            int childCount = thisTransform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                DestroyImmediate(thisTransform.GetChild(0).gameObject);
            }

            // Create the wheels.	
            Vector3 pos;
            for (int i = 0; i < numProp.intValue; i++)
            {
                pos.x = 0.0f;
                pos.y = wheelDistanceProp.floatValue / 2.0f;
                pos.z = -spacingProp.floatValue * i;
                Create_Wheel("L", pos, i + 1);
            }
            for (int i = 0; i < numProp.intValue; i++)
            {
                pos.x = 0.0f;
                pos.y = -wheelDistanceProp.floatValue / 2.0f;
                pos.z = -spacingProp.floatValue * i;
                Create_Wheel("R", pos, i + 1);
            }
        }


        void Create_Wheel(string direction, Vector3 position, int number)
        {
            //Create the gameobject, and set the transform.
            GameObject wheelObject = new GameObject("Supportwheel" + direction + "_" + number);
            wheelObject.transform.parent = thisTransform;
            wheelObject.transform.localPosition = position;
            if (direction == "L")
            {
                wheelObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
            }
            else
            {
                wheelObject.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 180.0f);
            }           
            
            // Add mesh components.
            MeshFilter meshFilter = wheelObject.AddComponent<MeshFilter>();
            meshFilter.mesh = wheelMeshProp.objectReferenceValue as Mesh;
            MeshRenderer meshRenderer = wheelObject.AddComponent<MeshRenderer>();
            var materials = new Material[wheelMaterialsNumProp.intValue];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = wheelMaterialsProp.GetArrayElementAtIndex(i).objectReferenceValue as Material;
            }
            meshRenderer.materials = materials;

            // Add "Wheel_Sync_CS" script.
            Wheel_Sync_CS syncScript = wheelObject.AddComponent<Wheel_Sync_CS>();
            syncScript.radiusOffset = radiusOffsetProp.floatValue;

            // Set the layer.
            wheelObject.layer = Layer_Settings_CS.Wheels_Layer;
        }

    }

}