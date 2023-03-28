using UnityEngine;
using System.Collections;
using UnityEditor;

namespace ChobiAssets.KTP
{

    [CustomEditor(typeof(Create_RoadWheels_CS))]
    public class Create_RoadWheels_CSEditor : Editor
    {
        SerializedProperty susDistanceProp;
        SerializedProperty numProp;
        SerializedProperty spacingProp;
        SerializedProperty susLengthProp;
        SerializedProperty susAngleProp;
        SerializedProperty susMassProp;
        SerializedProperty susSpringProp;
        SerializedProperty susDamperProp;
        SerializedProperty susTargetProp;
        SerializedProperty susForwardLimitProp;
        SerializedProperty susBackwardLimitProp;
        SerializedProperty susMesh_LProp;
        SerializedProperty susMesh_RProp;
        SerializedProperty susMaterialProp;
        SerializedProperty susMaterialsNumProp;
        SerializedProperty susMaterialsProp;
        SerializedProperty reinforceRadiusProp;

        SerializedProperty wheelDistanceProp;
        SerializedProperty wheelMassProp;
        SerializedProperty wheelRadiusProp;
        SerializedProperty physicMaterialProp;
        SerializedProperty wheelMeshProp;
        SerializedProperty wheelMaterialProp;
        SerializedProperty wheelMaterialsNumProp;
        SerializedProperty wheelMaterialsProp;

        SerializedProperty hasChangedProp;

        Transform thisTransform;


        void OnEnable()
        {
            susDistanceProp = serializedObject.FindProperty("susDistance");
            numProp = serializedObject.FindProperty("num");
            spacingProp = serializedObject.FindProperty("spacing");
            susLengthProp = serializedObject.FindProperty("susLength");
            susAngleProp = serializedObject.FindProperty("susAngle");
            susMassProp = serializedObject.FindProperty("susMass");
            susSpringProp = serializedObject.FindProperty("susSpring");
            susDamperProp = serializedObject.FindProperty("susDamper");
            susTargetProp = serializedObject.FindProperty("susTarget");
            susForwardLimitProp = serializedObject.FindProperty("susForwardLimit");
            susBackwardLimitProp = serializedObject.FindProperty("susBackwardLimit");
            susMesh_LProp = serializedObject.FindProperty("susMesh_L");
            susMesh_RProp = serializedObject.FindProperty("susMesh_R");
            susMaterialProp = serializedObject.FindProperty("susMaterial");
            susMaterialsNumProp = serializedObject.FindProperty("susMaterialsNum");
            susMaterialsProp = serializedObject.FindProperty("susMaterials");
            reinforceRadiusProp = serializedObject.FindProperty("reinforceRadius");

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

            // Suspension settings
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Suspension settings", MessageType.None, true);
            EditorGUILayout.Slider(susDistanceProp, 0.1f, 10.0f, "Arm Distance");
            EditorGUILayout.IntSlider(numProp, 0, 30, "Number");
            EditorGUILayout.Slider(spacingProp, 0.1f, 10.0f, "Spacing");
            EditorGUILayout.Slider(susLengthProp, -1.0f, 1.0f, "Length");
            EditorGUILayout.Slider(susAngleProp, -180.0f, 180.0f, "Angle");
            EditorGUILayout.Slider(susMassProp, 0.1f, 300.0f, "Mass");
            EditorGUILayout.Slider(susSpringProp, 0.0f, 100000.0f, "Sus Spring Force");
            if (susSpringProp.floatValue == 100000.0f)
            {
                susSpringProp.floatValue = Mathf.Infinity;
            }
            EditorGUILayout.Slider(susDamperProp, 0.0f, 10000.0f, "Sus Damper Force");
            EditorGUILayout.Slider(susTargetProp, -90.0f, 90.0f, "Sus Spring Target Angle");
            EditorGUILayout.Slider(susForwardLimitProp, -90.0f, 90.0f, "Forward Limit Angle");
            susForwardLimitProp.floatValue = Mathf.Clamp(susForwardLimitProp.floatValue , - susBackwardLimitProp.floatValue, 90.0f);
            EditorGUILayout.Slider(susBackwardLimitProp, -90.0f, 90.0f, "Backward Limit Angle");
            susBackwardLimitProp.floatValue = Mathf.Clamp(susBackwardLimitProp.floatValue, -susForwardLimitProp.floatValue, 90.0f);
            EditorGUILayout.Space();
            susMesh_LProp.objectReferenceValue = EditorGUILayout.ObjectField("Left Arm Mesh", susMesh_LProp.objectReferenceValue, typeof(Mesh), false);
            susMesh_RProp.objectReferenceValue = EditorGUILayout.ObjectField("Right Arm Mesh", susMesh_RProp.objectReferenceValue, typeof(Mesh), false);
            EditorGUILayout.IntSlider(susMaterialsNumProp, 1, 10, "Number of Materials");
            susMaterialsProp.arraySize = susMaterialsNumProp.intValue;
            if (susMaterialsNumProp.intValue == 1 && susMaterialProp.objectReferenceValue != null)
            {
                if (susMaterialsProp.GetArrayElementAtIndex(0).objectReferenceValue == null)
                {
                    susMaterialsProp.GetArrayElementAtIndex(0).objectReferenceValue = susMaterialProp.objectReferenceValue;
                }
                susMaterialProp.objectReferenceValue = null;
            }
            EditorGUI.indentLevel++;
            for (int i = 0; i < susMaterialsNumProp.intValue; i++)
            {
                susMaterialsProp.GetArrayElementAtIndex(i).objectReferenceValue = EditorGUILayout.ObjectField("Material " + "(" + i + ")", susMaterialsProp.GetArrayElementAtIndex(i).objectReferenceValue, typeof(Material), false);
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
            EditorGUILayout.Slider(reinforceRadiusProp, 0.1f, 1.0f, "Reinforce Radius");

            // Wheels settings
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Wheels settings", MessageType.None, true);
            EditorGUILayout.Slider(wheelDistanceProp, 0.1f, 10.0f, "Wheel Distance");
            EditorGUILayout.Slider(wheelMassProp, 0.1f, 300.0f, "Mass");
            EditorGUILayout.Slider(wheelRadiusProp, 0.01f, 1.0f, "SphereCollider Radius");
            physicMaterialProp.objectReferenceValue = EditorGUILayout.ObjectField("Physic Material", physicMaterialProp.objectReferenceValue, typeof(PhysicMaterial), false);
            EditorGUILayout.Space();
            wheelMeshProp.objectReferenceValue = EditorGUILayout.ObjectField("Wheel Mesh", wheelMeshProp.objectReferenceValue, typeof(Mesh), false);
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
            // Delete the ols objects.
            int childCount = thisTransform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                DestroyImmediate(thisTransform.GetChild(0).gameObject);
            }

            // Create the suspension arms.
            for (int i = 0; i < numProp.intValue; i++)
            {
                Create_Suspension("L", i + 1);
            }
            for (int i = 0; i < numProp.intValue; i++)
            {
                Create_Suspension("R", i + 1);
            }

            // Create the wheels.
            for (int i = 0; i < numProp.intValue; i++)
            {
                Create_Wheel("L", i + 1);
            }
            for (int i = 0; i < numProp.intValue; i++)
            {
                Create_Wheel("R", i + 1);
            }
        }


        void Create_Suspension(string direction, int number)
        {
            // Create the gameobject.
            GameObject armObject = new GameObject("Suspension_" + direction + "_" + number);

            // Set the parent.
            armObject.transform.parent = thisTransform;

            // Set the position.
            Vector3 pos;
            pos.x = 0.0f;
            pos.z = -spacingProp.floatValue * (number - 1);
            pos.y = susDistanceProp.floatValue / 2.0f;
            if (direction == "R")
            {
                pos.y *= -1.0f;
            }
            armObject.transform.localPosition = pos;

            // Set the rotation.
            armObject.transform.localRotation = Quaternion.Euler(0.0f, susAngleProp.floatValue, -90.0f);

            // Add mesh components.
            if (direction == "L")
            { // Left
                if (susMesh_LProp.objectReferenceValue)
                {
                    MeshFilter meshFilter = armObject.AddComponent<MeshFilter>();
                    meshFilter.mesh = susMesh_LProp.objectReferenceValue as Mesh;
                }
            }
            else
            { // Right
                if (susMesh_RProp.objectReferenceValue)
                {
                    MeshFilter meshFilter = armObject.AddComponent<MeshFilter>();
                    meshFilter.mesh = susMesh_RProp.objectReferenceValue as Mesh;
                }
            }
            MeshRenderer meshRenderer = armObject.AddComponent<MeshRenderer>();
            Material[] materials = new Material[susMaterialsNumProp.intValue];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = susMaterialsProp.GetArrayElementAtIndex(i).objectReferenceValue as Material;
            }
            meshRenderer.materials = materials;

            // Add Rigidbody.
            Rigidbody rigidbody = armObject.AddComponent<Rigidbody>();
            rigidbody.mass = susMassProp.floatValue;

            // Add HingeJoint.
            HingeJoint hingeJoint = armObject.AddComponent<HingeJoint>();
            hingeJoint.connectedBody = thisTransform.parent.GetComponent<Rigidbody>(); //MainBody's Rigidbody.
            hingeJoint.anchor = Vector3.zero;
            hingeJoint.axis = new Vector3(1.0f, 0.0f, 0.0f);
            hingeJoint.useSpring = true;
            JointSpring jointSpring = hingeJoint.spring;
            jointSpring.spring = susSpringProp.floatValue;
            jointSpring.damper = susDamperProp.floatValue;
            jointSpring.targetPosition = susTargetProp.floatValue + susAngleProp.floatValue;
            hingeJoint.spring = jointSpring;
            hingeJoint.useLimits = true;
            JointLimits jointLimits = hingeJoint.limits;
            jointLimits.max = susForwardLimitProp.floatValue + susAngleProp.floatValue;
            jointLimits.min = -(susBackwardLimitProp.floatValue - susAngleProp.floatValue);
            hingeJoint.limits = jointLimits;

            // Add SphereCollider for reinforcement.
            SphereCollider sphereCollider = armObject.AddComponent<SphereCollider>();
            sphereCollider.radius = reinforceRadiusProp.floatValue;

            // Set the layer.
            armObject.layer = Layer_Settings_CS.Reinforce_Layer; // Ignore all collision.
        }


        void Create_Wheel(string direction, int number)
        {
            // Create the gameobject.
            GameObject wheelObject = new GameObject("RoadWheel_" + direction + "_" + number);

            // Set the parent.
            wheelObject.transform.parent = thisTransform;

            // Set the position.
            Vector3 pos;
            pos.x = Mathf.Sin(Mathf.Deg2Rad * (180.0f + susAngleProp.floatValue)) * susLengthProp.floatValue;
            pos.z = Mathf.Cos(Mathf.Deg2Rad * (180.0f + susAngleProp.floatValue)) * susLengthProp.floatValue;
            pos.z -= spacingProp.floatValue * (number - 1);
            pos.y = wheelDistanceProp.floatValue / 2.0f;
            if (direction == "R")
            {
                pos.y *= -1.0f;
            }
            wheelObject.transform.localPosition = pos;

            // Set the rotation.
            if (direction == "L")
            { // Left
                wheelObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
            }
            else
            { // Right
                wheelObject.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 180);
            }

            // Add mesh components.
            if (wheelMeshProp.objectReferenceValue)
            {
                MeshFilter meshFilter = wheelObject.AddComponent<MeshFilter>();
                meshFilter.mesh = wheelMeshProp.objectReferenceValue as Mesh;
                MeshRenderer meshRenderer = wheelObject.AddComponent<MeshRenderer>();
                var materials = new Material[wheelMaterialsNumProp.intValue];
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = wheelMaterialsProp.GetArrayElementAtIndex(i).objectReferenceValue as Material;
                }
                meshRenderer.materials = materials;
            }

            // Add Rigidbody.
            Rigidbody rigidbody = wheelObject.AddComponent<Rigidbody>();
            rigidbody.mass = wheelMassProp.floatValue;

            // Add HingeJoint.
            HingeJoint hingeJoint = wheelObject.AddComponent<HingeJoint>();
            hingeJoint.anchor = Vector3.zero;
            hingeJoint.axis = new Vector3(0.0f, 1.0f, 0.0f);
            hingeJoint.connectedBody = thisTransform.Find("Suspension_" + direction + "_" + number).GetComponent<Rigidbody>();
            
            // Add SphereCollider.
            SphereCollider sphereCollider = wheelObject.AddComponent<SphereCollider>();
            sphereCollider.radius = wheelRadiusProp.floatValue;
            sphereCollider.material = physicMaterialProp.objectReferenceValue as PhysicMaterial;
            
            // Add "Wheel_Rotate_CS" script.
            wheelObject.AddComponent<Wheel_Rotate_CS>();
            
            // Set the layer.
            wheelObject.layer = Layer_Settings_CS.Wheels_Layer;
        }


        void OnSceneGUI()
        { // Visualize the angles settings.
            for (int i = 0; i < thisTransform.childCount; i++)
            {
                Transform childTransform = thisTransform.GetChild(i);
                if (childTransform.gameObject.layer == Layer_Settings_CS.Reinforce_Layer)
                { // Suspension Arm.
                    HingeJoint joint = childTransform.GetComponent<HingeJoint>();
                    Transform bodyTransform = joint.connectedBody.transform;
                    Vector3 anchorPos = bodyTransform.position + (bodyTransform.right * joint.connectedAnchor.x) + (bodyTransform.up * joint.connectedAnchor.y) + (bodyTransform.forward * joint.connectedAnchor.z);
                    float currentAng = joint.limits.max - susForwardLimitProp.floatValue;
                    Vector3 tempPos;
                    tempPos.x = 0.0f;
                    // Limits Min
                    tempPos.y = Mathf.Sin(Mathf.Deg2Rad * (joint.limits.min - currentAng)) * susLengthProp.floatValue;
                    tempPos.z = -Mathf.Cos(Mathf.Deg2Rad * (joint.limits.min - currentAng)) * susLengthProp.floatValue;
                    Handles.color = Color.green;
                    Handles.DrawLine(anchorPos, anchorPos + (bodyTransform.up * tempPos.y) + (bodyTransform.forward * tempPos.z));
                    // Limits Max
                    tempPos.y = Mathf.Sin(Mathf.Deg2Rad * (joint.limits.max - currentAng)) * susLengthProp.floatValue;
                    tempPos.z = -Mathf.Cos(Mathf.Deg2Rad * (joint.limits.max - currentAng)) * susLengthProp.floatValue;
                    Handles.color = Color.green;
                    Handles.DrawLine(anchorPos, anchorPos + (bodyTransform.up * tempPos.y) + (bodyTransform.forward * tempPos.z));
                    // Target
                    tempPos.y = Mathf.Sin(Mathf.Deg2Rad * (joint.spring.targetPosition - currentAng)) * susLengthProp.floatValue;
                    tempPos.z = -Mathf.Cos(Mathf.Deg2Rad * (joint.spring.targetPosition - currentAng)) * susLengthProp.floatValue;
                    Handles.color = Color.red;
                    Handles.DrawLine(anchorPos, anchorPos + (bodyTransform.up * tempPos.y) + (bodyTransform.forward * tempPos.z));
                    // Current
                    currentAng = childTransform.localEulerAngles.y;
                    tempPos.y = Mathf.Sin(Mathf.Deg2Rad * -currentAng) * susLengthProp.floatValue;
                    tempPos.z = -Mathf.Cos(Mathf.Deg2Rad * -currentAng) * susLengthProp.floatValue;
                    Handles.color = Color.yellow;
                    Handles.DrawLine(anchorPos, anchorPos + (bodyTransform.up * tempPos.y) + (bodyTransform.forward * tempPos.z));
                }
            }
        }
    }

}