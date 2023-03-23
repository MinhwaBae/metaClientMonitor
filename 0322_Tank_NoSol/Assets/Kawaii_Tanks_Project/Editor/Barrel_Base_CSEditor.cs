using UnityEngine;
using UnityEditor;

namespace ChobiAssets.KTP
{

    [CustomEditor(typeof(Barrel_Base_CS))]
    public class Barrel_Base_CSEditor : Editor
    {

        SerializedProperty partMeshProp;

        SerializedProperty collidersNumProp;
        SerializedProperty collidersMeshProp;

        SerializedProperty materialsNumProp;
        SerializedProperty materialsProp;

        SerializedProperty offsetXProp;
        SerializedProperty offsetYProp;
        SerializedProperty offsetZProp;

        SerializedProperty hasChangedProp;

        Transform thisTransform;


        void OnEnable()
        {
            partMeshProp = serializedObject.FindProperty("partMesh");

            collidersNumProp = serializedObject.FindProperty("collidersNum");
            collidersMeshProp = serializedObject.FindProperty("collidersMesh");

            materialsNumProp = serializedObject.FindProperty("materialsNum");
            materialsProp = serializedObject.FindProperty("materials");

            offsetXProp = serializedObject.FindProperty("offsetX");
            offsetYProp = serializedObject.FindProperty("offsetY");
            offsetZProp = serializedObject.FindProperty("offsetZ");

            hasChangedProp = serializedObject.FindProperty("hasChanged");

            if (Selection.activeGameObject)
            {
                thisTransform = Selection.activeGameObject.transform;
            }
        }


        public override void OnInspectorGUI()
        {
            Set_Inspector();
        }


        void Set_Inspector()
        {
            // Check the Prefab Mode.
            if (PrefabUtility.IsPartOfPrefabInstance(thisTransform))
            {
                GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
                EditorGUILayout.HelpBox("\n'Barrel can be modified only in the Prefab Mode.\nPlease go to the Prefab Mode, or Unpack the prefab.\n", MessageType.Warning, true);
                return;
            }

            serializedObject.Update();

            GUI.backgroundColor = new Color(1.0f, 1.0f, 0.5f, 1.0f);

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Fold out above 'Transform' window when you move this object.", MessageType.Warning, true);

            // Mesh settings
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Mesh settings", MessageType.None, true);
            partMeshProp.objectReferenceValue = EditorGUILayout.ObjectField("Mesh", partMeshProp.objectReferenceValue, typeof(Mesh), false);
            EditorGUILayout.Space();
            EditorGUILayout.IntSlider(materialsNumProp, 1, 10, "Number of Materials");
            materialsProp.arraySize = materialsNumProp.intValue;
            EditorGUI.indentLevel++;
            for (int i = 0; i < materialsNumProp.intValue; i++)
            {
                materialsProp.GetArrayElementAtIndex(i).objectReferenceValue = EditorGUILayout.ObjectField("Material " + "(" + i + ")", materialsProp.GetArrayElementAtIndex(i).objectReferenceValue, typeof(Material), false);
            }
            EditorGUI.indentLevel--;

            // Position settings
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Position settings", MessageType.None, true);
            EditorGUILayout.Slider(offsetXProp, -5.0f, 5.0f, "Offset X");
            EditorGUILayout.Slider(offsetYProp, -5.0f, 5.0f, "Offset Y");
            EditorGUILayout.Slider(offsetZProp, -10.0f, 10.0f, "Offset Z");

            // Collider settings
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Collider settings", MessageType.None, true);
            EditorGUILayout.IntSlider(collidersNumProp, 0, 10, "Number of Colliders");
            collidersMeshProp.arraySize = collidersNumProp.intValue;
            EditorGUI.indentLevel++;
            for (int i = 0; i < collidersNumProp.intValue; i++)
            {
                collidersMeshProp.GetArrayElementAtIndex(i).objectReferenceValue = EditorGUILayout.ObjectField("MeshCollider " + "(" + i + ")", collidersMeshProp.GetArrayElementAtIndex(i).objectReferenceValue, typeof(Mesh), false);
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

            // Call "Create()" while the object is moving.
            if (thisTransform.hasChanged)
            {
                thisTransform.hasChanged = false;
                Create();
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
        }


        void Create()
        {
            Transform oldTransform = thisTransform.Find("Barrel"); // Find the old object.
            int childCount;
            Transform[] childTransforms;
            if (oldTransform)
            {
                childCount = oldTransform.transform.childCount;
                childTransforms = new Transform[childCount];
                for (int i = 0; i < childCount; i++)
                {
                    childTransforms[i] = oldTransform.GetChild(0); // Get the child object such as "Armor_Collider".
                    childTransforms[i].parent = thisTransform; // Change the parent of the child object.
                }
                DestroyImmediate(oldTransform.gameObject); // Delete old object.
            }
            else
            {
                childCount = 0;
                childTransforms = null;
            }

            // Create new Gameobject & Set Transform.
            GameObject newObject = new GameObject("Barrel");
            newObject.transform.parent = thisTransform;
            newObject.transform.localPosition = -thisTransform.localPosition + new Vector3(offsetXProp.floatValue, offsetYProp.floatValue, offsetZProp.floatValue);
            newObject.transform.localRotation = Quaternion.identity;

            // Mesh settings.
            MeshRenderer meshRenderer = newObject.AddComponent<MeshRenderer>();
            Material[] materials = new Material[materialsNumProp.intValue];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = materialsProp.GetArrayElementAtIndex(i).objectReferenceValue as Material;
            }
            meshRenderer.materials = materials;
            MeshFilter meshFilter = newObject.AddComponent<MeshFilter>();
            meshFilter.mesh = partMeshProp.objectReferenceValue as Mesh;

            // Collider settings.
            for (int i = 0; i < collidersNumProp.intValue; i++)
            {
                MeshCollider meshCollider = newObject.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = collidersMeshProp.GetArrayElementAtIndex(i).objectReferenceValue as Mesh;
                meshCollider.convex = true;
            }

            // Set the layer
            newObject.layer = 0;

            // Return the child objects.
            if (childCount > 0)
            {
                for (int i = 0; i < childCount; i++)
                {
                    childTransforms[i].transform.parent = newObject.transform;
                }
            }
        }

    }

}