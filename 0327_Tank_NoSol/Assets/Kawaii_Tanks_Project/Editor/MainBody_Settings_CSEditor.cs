using UnityEngine;
using System.Collections;
using UnityEditor;


namespace ChobiAssets.KTP
{

    [CustomEditor(typeof(MainBody_Settings_CS))]
    public class MainBody_Settings_CSEditor : Editor
    {
        SerializedProperty bodyMassProp;
        SerializedProperty bodyMeshProp;
        SerializedProperty materialsNumProp;
        SerializedProperty materialsProp;
        SerializedProperty collidersNumProp;
        SerializedProperty collidersMeshProp;
        SerializedProperty solverIterationCountProp;

        SerializedProperty hasChangedProp;

        GameObject thisGameObject;


        void OnEnable()
        {
            bodyMassProp = serializedObject.FindProperty("bodyMass");
            bodyMeshProp = serializedObject.FindProperty("bodyMesh");
            materialsNumProp = serializedObject.FindProperty("materialsNum");
            materialsProp = serializedObject.FindProperty("materials");
            collidersNumProp = serializedObject.FindProperty("collidersNum");
            collidersMeshProp = serializedObject.FindProperty("collidersMesh");
            solverIterationCountProp = serializedObject.FindProperty("solverIterationCount");

            hasChangedProp = serializedObject.FindProperty("hasChanged");

            thisGameObject = Selection.activeGameObject;
        }


        public override void OnInspectorGUI()
        {
            if (EditorApplication.isPlaying)
            {
                return;
            }

            Set_Inspector();
        }


        void Set_Inspector()
        {
            serializedObject.Update();

            GUI.backgroundColor = new Color(1.0f, 1.0f, 0.5f, 1.0f);

            // Basic Settings
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Basic Settings", MessageType.None, true);
            EditorGUILayout.Slider(bodyMassProp, 1.0f, 100000.0f, "Mass");
            EditorGUILayout.Space();
            bodyMeshProp.objectReferenceValue = EditorGUILayout.ObjectField("Mesh", bodyMeshProp.objectReferenceValue, typeof(Mesh), false);
            EditorGUILayout.Space();
            EditorGUILayout.IntSlider(materialsNumProp, 1, 99, "Number of Materials");
            materialsProp.arraySize = materialsNumProp.intValue;
            EditorGUI.indentLevel++;
            for (int i = 0; i < materialsNumProp.intValue; i++)
            {
                materialsProp.GetArrayElementAtIndex(i).objectReferenceValue = EditorGUILayout.ObjectField("Material " + "(" + i + ")", materialsProp.GetArrayElementAtIndex(i).objectReferenceValue, typeof(Material), false);
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            // Collider settings
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Collider settings", MessageType.None, true);
            EditorGUILayout.IntSlider(collidersNumProp, 1, 10, "Number of Colliders");
            collidersMeshProp.arraySize = collidersNumProp.intValue;
            EditorGUI.indentLevel++;
            for (int i = 0; i < collidersNumProp.intValue; i++)
            {
                collidersMeshProp.GetArrayElementAtIndex(i).objectReferenceValue = EditorGUILayout.ObjectField("MeshCollider " + "(" + i + ")", collidersMeshProp.GetArrayElementAtIndex(i).objectReferenceValue, typeof(Mesh), false);
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            // Physics settings
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Physics settings", MessageType.None, true);
            EditorGUILayout.IntSlider(solverIterationCountProp, 1, 100, "Solver Iteration Count");
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
            // Rigidbody settings.
            Rigidbody thisRigidbody = thisGameObject.GetComponent<Rigidbody>();
            thisRigidbody.mass = bodyMassProp.floatValue;

            // Mesh settings.
            thisGameObject.GetComponent<MeshFilter>().mesh = bodyMeshProp.objectReferenceValue as Mesh;
            Material[] materials = new Material[materialsNumProp.intValue];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = materialsProp.GetArrayElementAtIndex(i).objectReferenceValue as Material;
            }
            thisGameObject.GetComponent<MeshRenderer>().materials = materials;

            // Collider settings.
            MeshCollider[] oldMeshColliders = thisGameObject.GetComponents<MeshCollider>();
            for (int i = 0; i < oldMeshColliders.Length; i++)
            {
                var oldCollider = oldMeshColliders[i];
                EditorApplication.delayCall += () => DestroyImmediate(oldCollider);
            }
            for (int i = 0; i < collidersNumProp.intValue; i++)
            {
                MeshCollider meshCollider = thisGameObject.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = collidersMeshProp.GetArrayElementAtIndex(i).objectReferenceValue as Mesh;
                meshCollider.convex = true;
            }

            // Set the Layer.
            thisGameObject.layer = Layer_Settings_CS.Body_Layer;
        }
    }

}
