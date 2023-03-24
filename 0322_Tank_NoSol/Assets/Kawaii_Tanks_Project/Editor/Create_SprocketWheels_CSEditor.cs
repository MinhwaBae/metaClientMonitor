using UnityEngine;
using System.Collections;
using UnityEditor;

namespace ChobiAssets.KTP
{

    [CustomEditor(typeof(Create_SprocketWheels_CS))]
    public class Create_SprocketWheels_CSEditor : Editor
    {

        SerializedProperty useArmProp;
        SerializedProperty armDistanceProp;
        SerializedProperty armLengthProp;
        SerializedProperty armAngleProp;
        SerializedProperty armMesh_LProp;
        SerializedProperty armMesh_RProp;
        SerializedProperty armMaterialProp;
        SerializedProperty armMaterialsNumProp;
        SerializedProperty armMaterialsProp;

        SerializedProperty wheelDistanceProp;
        SerializedProperty wheelMassProp;
        SerializedProperty wheelRadiusProp;
        SerializedProperty physicMaterialProp;
        SerializedProperty wheelMeshProp;
        SerializedProperty wheelMaterialProp;
        SerializedProperty wheelMaterialsNumProp;
        SerializedProperty wheelMaterialsProp;

        SerializedProperty radiusOffsetProp;

        SerializedProperty hasChangedProp;

        Transform thisTransform;


        void OnEnable()
        {
            radiusOffsetProp = serializedObject.FindProperty("radiusOffset");

            useArmProp = serializedObject.FindProperty("useArm");
            armDistanceProp = serializedObject.FindProperty("armDistance");
            armLengthProp = serializedObject.FindProperty("armLength");
            armAngleProp = serializedObject.FindProperty("armAngle");
            armMesh_LProp = serializedObject.FindProperty("armMesh_L");
            armMesh_RProp = serializedObject.FindProperty("armMesh_R");
            armMaterialProp = serializedObject.FindProperty("armMaterial");
            armMaterialsNumProp = serializedObject.FindProperty("armMaterialsNum");
            armMaterialsProp = serializedObject.FindProperty("armMaterials");

            wheelDistanceProp = serializedObject.FindProperty("wheelDistance");
            wheelMassProp = serializedObject.FindProperty("wheelMass");
            wheelRadiusProp = serializedObject.FindProperty("wheelRadius");
            physicMaterialProp = serializedObject.FindProperty("physicMaterial");
            wheelMeshProp = serializedObject.FindProperty("wheelMesh");
            wheelMaterialProp = serializedObject.FindProperty("wheelMaterial");
            wheelMaterialsNumProp = serializedObject.FindProperty("wheelMaterialsNum");
            wheelMaterialsProp = serializedObject.FindProperty("wheelMaterials");

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

            // Tensioner Arms settings
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Arms settings", MessageType.None, true);
            useArmProp.boolValue = EditorGUILayout.Toggle("Use Arm", useArmProp.boolValue);
            if (useArmProp.boolValue)
            {
                EditorGUILayout.Slider(armDistanceProp, 0.1f, 10.0f, "Distance");
                EditorGUILayout.Slider(armLengthProp, -1.0f, 1.0f, "Length");
                EditorGUILayout.Slider(armAngleProp, -180.0f, 180.0f, "Angle");
                armMesh_LProp.objectReferenceValue = EditorGUILayout.ObjectField("Mesh of Left", armMesh_LProp.objectReferenceValue, typeof(Mesh), false);
                armMesh_RProp.objectReferenceValue = EditorGUILayout.ObjectField("Mesh of Right", armMesh_RProp.objectReferenceValue, typeof(Mesh), false);
                EditorGUILayout.IntSlider(armMaterialsNumProp, 1, 10, "Number of Materials");
                armMaterialsProp.arraySize = armMaterialsNumProp.intValue;
                if (armMaterialsNumProp.intValue == 1 && armMaterialProp.objectReferenceValue != null)
                {
                    if (armMaterialsProp.GetArrayElementAtIndex(0).objectReferenceValue == null)
                    {
                        armMaterialsProp.GetArrayElementAtIndex(0).objectReferenceValue = armMaterialProp.objectReferenceValue;
                    }
                    armMaterialProp.objectReferenceValue = null;
                }
                EditorGUI.indentLevel++;
                for (int i = 0; i < armMaterialsNumProp.intValue; i++)
                {
                    armMaterialsProp.GetArrayElementAtIndex(i).objectReferenceValue = EditorGUILayout.ObjectField("Material " + "(" + i + ")", armMaterialsProp.GetArrayElementAtIndex(i).objectReferenceValue, typeof(Material), false);
                }
            }

            // Wheels settings
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Wheels settings", MessageType.None, true);
            EditorGUILayout.Slider(wheelDistanceProp, 0.1f, 10.0f, "Distance");
            EditorGUILayout.Slider(wheelMassProp, 0.1f, 300.0f, "Mass");
            EditorGUILayout.Slider(wheelRadiusProp, 0.01f, 1.0f, "SphereCollider Radius");
            EditorGUILayout.Slider(radiusOffsetProp, -0.5f, 0.5f, "Radius Offset");

            physicMaterialProp.objectReferenceValue = EditorGUILayout.ObjectField("Physic Material", physicMaterialProp.objectReferenceValue, typeof(PhysicMaterial), false);
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

            // Create the arms and wheels.
            Vector3 pos;
            if (useArmProp.boolValue)
            { //With arms.
                // Create the arms.
                Create_Arm("L", new Vector3(0.0f, armDistanceProp.floatValue / 2.0f, 0.0f));
                Create_Arm("R", new Vector3(0.0f, -armDistanceProp.floatValue / 2.0f, 0.0f));
                
                // Set the wheels position.
                pos.x = Mathf.Sin(Mathf.Deg2Rad * (180.0f + armAngleProp.floatValue)) * armLengthProp.floatValue;
                pos.y = wheelDistanceProp.floatValue / 2.0f;
                pos.z = Mathf.Cos(Mathf.Deg2Rad * (180.0f + armAngleProp.floatValue)) * armLengthProp.floatValue;
            }
            else
            { // No arm.
                // Set the wheels position.
                pos.x = 0.0f;
                pos.y = wheelDistanceProp.floatValue / 2.0f;
                pos.z = 0.0f;
            }

            // Create the wheels.
            Create_Apparent_Wheel("L", new Vector3(pos.x, pos.y, pos.z));
            Create_Apparent_Wheel("R", new Vector3(pos.x, -pos.y, pos.z));
            Create_Invisible_Wheel("L", new Vector3(pos.x, pos.y, pos.z));
            Create_Invisible_Wheel("R", new Vector3(pos.x, -pos.y, pos.z));
        }


        void Create_Arm(string direction, Vector3 position)
        {
            // Create the gameobject, and set the transform.
            GameObject armObject = new GameObject("Arm" + direction);
            armObject.transform.parent = thisTransform;
            armObject.transform.localPosition = position;
            armObject.transform.localRotation = Quaternion.Euler(0.0f, armAngleProp.floatValue, -90.0f);
            
            // Add mesh components.
            if (direction == "L")
            { // Left
                if (armMesh_LProp.objectReferenceValue)
                {
                    MeshFilter meshFilter = armObject.AddComponent<MeshFilter>();
                    meshFilter.mesh = armMesh_LProp.objectReferenceValue as Mesh;
                }
            }
            else
            { //Right
                if (armMesh_RProp.objectReferenceValue)
                {
                    MeshFilter meshFilter = armObject.AddComponent<MeshFilter>();
                    meshFilter.mesh = armMesh_RProp.objectReferenceValue as Mesh;
                }
            }
            MeshRenderer meshRenderer = armObject.AddComponent<MeshRenderer>();
            Material[] materials = new Material[armMaterialsNumProp.intValue];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = armMaterialsProp.GetArrayElementAtIndex(i).objectReferenceValue as Material;
            }
            meshRenderer.materials = materials;
        }


        void Create_Apparent_Wheel(string direction, Vector3 position)
        {
            // Create the gameobject.
            GameObject wheelObject = Create_GameObject("Apparent_Wheel", direction, position);

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
        }


        void Create_Invisible_Wheel(string direction, Vector3 position)
        {
            // Create the gameobject.
            GameObject wheelObject = Create_GameObject("Invisible_Wheel", direction, position);

            // Add mesh components.
            MeshFilter meshFilter = wheelObject.AddComponent<MeshFilter>(); // Set only MeshFilter in order to get the mesh size.
            meshFilter.mesh = wheelMeshProp.objectReferenceValue as Mesh;

            // Add SphereCollider.
            SphereCollider sphereCollider = wheelObject.AddComponent<SphereCollider>();
            sphereCollider.radius = wheelRadiusProp.floatValue;
            sphereCollider.material = physicMaterialProp.objectReferenceValue as PhysicMaterial;
            
            // Add Rigidbody.
            Rigidbody rigidbody = wheelObject.AddComponent<Rigidbody>();
            rigidbody.mass = wheelMassProp.floatValue;
            
            // Add HingeJoint.
            HingeJoint hingeJoint;
            hingeJoint = wheelObject.AddComponent<HingeJoint>();
            hingeJoint.anchor = Vector3.zero;
            hingeJoint.axis = new Vector3(0.0f, 1.0f, 0.0f);
            hingeJoint.connectedBody = thisTransform.parent.gameObject.GetComponent<Rigidbody>();
            
            // Add "Wheel_Rotate_CS" script.
            wheelObject.AddComponent<Wheel_Rotate_CS>();
        }


        GameObject Create_GameObject(string name, string direction, Vector3 position)
        {
            // Create the gameobject, and set the transform.
            GameObject gameObject = new GameObject(name + "_" + direction);
            gameObject.transform.parent = thisTransform;
            gameObject.transform.localPosition = position;
            if (direction == "L")
            {
                gameObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
            }
            else
            {
                gameObject.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 180.0f);
            }

            // Set the layer.
            gameObject.layer = Layer_Settings_CS.Wheels_Layer;

            return gameObject;
        }

    }

}