using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace ChobiAssets.KTP
{

    [DefaultExecutionOrder(+1)] // (Note.) This script is executed after other scripts, in order to detect the target certainly.
    public class Aiming_Control_CS : MonoBehaviour
    {
        /*
		 * This script is attached to the "MainBody" of the tank.
		 * This script controls the aiming of the tank.
		 * "Turret_Control_CS" and "Cannon_Control_CS" scripts rotate the turret and cannon referring to this variables.
		*/

		
        [Tooltip("The turret leads the target automatically.")] public bool useAutoLead = false;


        Turret_Control_CS turretControlScript;
        Cannon_Control_CS cannonControlScript;
        [HideInInspector] public bool useAutoTurn; // Referred to from "Turret_Control_CS" and "Cannon_Control_CS".

        // For auto-turn.
        [HideInInspector] public int mode; // Referred to from "AimMarker_Control_CS". // 0 => Keep the initial positon, 1 => Free aiming,  2 => Locking on.
        Transform rootTransform;
        Rigidbody bodyRigidbody;
        [HideInInspector] public Vector3 targetPosition; // Referred to from "Turret_Control_CS", "Cannon_Control_CS", "AimMarker_Control_CS".
        [HideInInspector] public Transform targetTransform; // Referred to from "AimMarker_Control_CS".
        Vector3 targetOffset;
        [HideInInspector] public Rigidbody targetRigidbody; // Referred to from "Turret_Control_CS".
        [HideInInspector] public Vector3 adjustAngle; // Referred to from "Turret_Control_CS" and "Cannon_Control_CS".
        const float spherecastRadius = 3.0f;
        const float angleRange = 10.0f;
        int thisRelationship;

        // For manual-turn.
        [HideInInspector] public float turretTurnRate; // Referred to from "Turret_Control_CS".
        [HideInInspector] public float cannonTurnRate; // Referred to from "Cannon_Control_CS".

        Aiming_Control_Input_00_Base_CS inputScript;
        bool isPlayer;
        bool isAI;


        void Start()
        {
            Initialize();
        }


        void Initialize()
        {
            // Get the references via "ID_Control_CS".
            var idScript = GetComponentInParent<ID_Control_CS>();
            rootTransform = idScript.transform;
            isPlayer = idScript.isPlayer;
            thisRelationship = idScript.relationship;
            isAI = idScript.aiScript;
            bodyRigidbody = idScript.bodyRigidbody;

            // Get the "Turret_Horizontal_CS" and "Cannon_Vertical_CS" scripts in the tank.
            turretControlScript = GetComponentInChildren<Turret_Control_CS>();
            cannonControlScript = GetComponentInChildren<Cannon_Control_CS>();

            // Send this reference to "AimMarker_Control_CS" in the scene.
            if (isPlayer && AimMarker_Control_CS.instance)
            {
                AimMarker_Control_CS.instance.Get_Aiming_Control_Script(this);
            }

            // Send this reference to "LeadMarker_Control_CS" in the scene.
            if (isPlayer && LeadMarker_Control_CS.instance)
            {
                LeadMarker_Control_CS.instance.Get_Aiming_Control_Script(this);
            }

            // Check the AI.
            if (isAI)
            {
                useAutoTurn = true;
                useAutoLead = true;
                return;
            }

            // Set the input script.
            Set_Input_Script();

            // Prepare the input script.
            inputScript.Prepare(this);
        }


        protected virtual void Set_Input_Script()
        {
#if !UNITY_ANDROID && !UNITY_IPHONE
            inputScript = gameObject.AddComponent<Aiming_Control_Input_01_Desktop_CS>();
#else
            inputScript = gameObject.AddComponent<Aiming_Control_Input_02_Mobile_CS>();
#endif
        }


        void Update()
        {
            if (isPlayer && inputScript)
            {
                inputScript.Get_Input();
            }
        }


        void FixedUpdate()
        {
            // Update the target position.
            if (targetTransform)
            {
                Update_Target_Position();
            }
            else if (mode == 1)
            { // Free aiming.
                targetPosition += bodyRigidbody.velocity * Time.fixedDeltaTime;
            }
        }


        void Update_Target_Position()
        {
            // Check the target is living.
            if (targetTransform.root.tag == "Finish")
            { // The target has been destroyed.
                targetTransform = null;
                targetRigidbody = null;
                return;
            }

            // Update the target position.
            targetPosition = targetTransform.position + (targetTransform.forward * targetOffset.z) + (targetTransform.right * targetOffset.x) + (targetTransform.up * targetOffset.y);
        }


        public void Switch_Mode()
        { // Called also from "Aiming_Control_Input_##_###".
            switch (mode)
            {
                case 0: // Keep the initial positon.
                    targetTransform = null;
                    targetRigidbody = null;
                    turretControlScript.Stop_Tracking();
                    cannonControlScript.Stop_Tracking();
                    break;

                case 1: // Free aiming.
                    targetTransform = null;
                    targetRigidbody = null;
                    turretControlScript.Start_Tracking();
                    cannonControlScript.Start_Tracking();
                    break;

                case 2: // Locking on.
                    turretControlScript.Start_Tracking();
                    cannonControlScript.Start_Tracking();
                    break;
            }
        }


        public void Cast_Ray_Free(Vector3 screenPos)
        { // Called from "Aiming_Control_Input_##_###".
            // Find a target by casting a ray from the camera.
            var mainCamera = Camera.main;
            var ray = mainCamera.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out RaycastHit raycastHit, 2048.0f, Layer_Settings_CS.Aiming_Layer_Mask))
            {
                var colliderTransform = raycastHit.collider.transform;
                if (colliderTransform.root != rootTransform && colliderTransform.root.tag != "Finish")
                { // The hit collider is not itself, and is not destroyed.

                    // Check the rigidbody.
                    // (Note.) When the hit collider has no rigidbody and its parent has a rigidbody, the parent's transform is set as 'RaycastHit.transform'.
                    var tempTargetRigidbody = raycastHit.rigidbody;
                    if (tempTargetRigidbody)
                    { // The target has a rigidbody.
                        // Set the hit collider as the target.
                        targetTransform = colliderTransform;
                        targetOffset = targetTransform.InverseTransformPoint(raycastHit.point);
                        if (targetTransform.localScale != Vector3.one)
                        { // for "Armor_Collider".
                            targetOffset.x *= targetTransform.localScale.x;
                            targetOffset.y *= targetTransform.localScale.y;
                            targetOffset.z *= targetTransform.localScale.z;
                        }
                        targetRigidbody = tempTargetRigidbody;
                        targetPosition = raycastHit.point;
                        return;
                    } // The target does not have rigidbody.
                } // The ray hits itself, or the target is already dead.
            } // The ray does not hit anythig.

            // Set the position through this tank.
            targetTransform = null;
            targetRigidbody = null;
            screenPos.z = 128.0f;
            targetPosition = mainCamera.ScreenToWorldPoint(screenPos);
        }


        public void Reticle_Aiming(Vector3 screenPos)
        { // Called from "Aiming_Control_Input_##_###".

            // Find a target by casting a sphere from the camera.
            var ray = Camera.main.ScreenPointToRay(screenPos);
            var raycastHits = Physics.SphereCastAll(ray, spherecastRadius, 2048.0f, Layer_Settings_CS.Aiming_Layer_Mask);
            for (int i = 0; i < raycastHits.Length; i++)
            {
                Transform colliderTransform = raycastHits[i].collider.transform;
                if (colliderTransform.root != rootTransform && colliderTransform.root.tag != "Finish")
                { // The hit collider is not itself, and is not destroyed.

                    // Check the rigidbody.
                    // (Note.) When the hit collider has no rigidbody and its parent has a rigidbody, the parent's transform is set as 'RaycastHits.transform'.
                    var tempRigidbody = raycastHits[i].rigidbody;
                    if (tempRigidbody == null)
                    {
                        continue;
                    }

                    // Check the layer.
                    if (tempRigidbody.gameObject.layer != Layer_Settings_CS.Body_Layer)
                    { // The target is not a MainBody.
                        continue;
                    }

                    // Check the relationship.
                    var idScript = raycastHits[i].transform.GetComponentInParent<ID_Control_CS>();
                    if (idScript && idScript.relationship == thisRelationship)
                    { // The target is a friend.
                        continue;
                    }

                    // Check the obstacle.
                    if (Physics.Linecast(ray.origin, raycastHits[i].point, out RaycastHit raycastHit, Layer_Settings_CS.Aiming_Layer_Mask))
                    { // The target is obstructed by anything.
                        if (raycastHit.transform.root != rootTransform)
                        { // The obstacle is not itself.
                            continue;
                        }
                    }

                    // Set the MainBody as the target.
                    targetTransform = raycastHits[i].transform;
                    targetOffset = Vector3.zero;
                    targetOffset.y = 0.5f;
                    targetRigidbody = tempRigidbody;
                    targetPosition = raycastHits[i].point;
                    adjustAngle = Vector3.zero;
                    return;
                }
            } // Target with a rigidbody cannot be found.
        }


        public void Auto_Lock(int direction)
        { // 0 = Left, 1 = Right, 2 = Front.

            // Check the "AI_Headquaters_CS" is set in the scene.
            if (AI_Headquaters_CS.instance == null)
            {
                return;
            }

            // Get the base angle to detect the new target.
            float baseAng;
            var mainCamera = Camera.main;
            if (direction != 2 && targetTransform)
            {
                Vector3 currentLocalPos = mainCamera.transform.InverseTransformPoint(targetPosition);
                baseAng = Vector2.Angle(Vector2.up, new Vector2(currentLocalPos.x, currentLocalPos.z)) * Mathf.Sign(currentLocalPos.x);
            }
            else
            {
                baseAng = 0.0f;
            }

            // Get the tank list from the "AI_Headquaters_Helper_CS".
            List<AI_Helper_CS> enemyTankList;
            if (thisRelationship == 0)
            {
                enemyTankList = AI_Headquaters_CS.instance.hostileTanksList;
            }
            else
            {
                enemyTankList = AI_Headquaters_CS.instance.friendlyTanksList;
            }

            // Find a new target.
            int targetIndex = 0;
            int oppositeTargetIndex = 0;
            float minAng = 180.0f;
            float maxAng = 0.0f;
            for (int i = 0; i < enemyTankList.Count; i++)
            {
                if (enemyTankList[i].bodyTransform == null)
                { // The tank has been removed from the scene.
                    continue;
                }

                if (enemyTankList[i].bodyTransform.root.tag == "Finish" || (targetTransform && targetTransform == enemyTankList[i].bodyTransform))
                { // The tank is dead, or the tank is the same as the current target.
                    continue;
                }
                Vector3 localPos = mainCamera.transform.InverseTransformPoint(enemyTankList[i].bodyTransform.position);
                float tempAng = Vector2.Angle(Vector2.up, new Vector2(localPos.x, localPos.z)) * Mathf.Sign(localPos.x);
                float deltaAng = Mathf.Abs(Mathf.DeltaAngle(tempAng, baseAng));
                if ((direction == 0 && tempAng > baseAng) || (direction == 1 && tempAng < baseAng))
                { // On the opposite side.
                    if (deltaAng > maxAng)
                    {
                        oppositeTargetIndex = i;
                        maxAng = deltaAng;
                    }
                    continue;
                }
                if (deltaAng < minAng)
                {
                    targetIndex = i;
                    minAng = deltaAng;
                }
            }

            if (minAng != 180.0f)
            { // Target is found on the indicated side.
                targetTransform = enemyTankList[targetIndex].bodyTransform;
                targetRigidbody = targetTransform.GetComponent<Rigidbody>();
                targetOffset = Vector3.zero;
                targetOffset.y = 0.5f;
                mode = 2; // Lock on.
                Switch_Mode();
            }
            else if (maxAng != 0.0f)
            { // Target is not found on the indicated side, but it could be found on the opposite side.
                targetTransform = enemyTankList[oppositeTargetIndex].bodyTransform;
                targetRigidbody = targetTransform.GetComponent<Rigidbody>();
                targetOffset = Vector3.zero;
                targetOffset.y = 0.5f;
                mode = 2; // Lock on.
                Switch_Mode();
            }

            if (targetTransform)
            {
                StartCoroutine("Send_Target_Position");
            }
        }

       
        public void Touch_Lock(Vector3 screenPos)
        { // Find a target near the touch position.

            // Check the "AI_Headquaters_CS" is set in the scene.
            if (AI_Headquaters_CS.instance == null)
            {
                return;
            }

            // Get the tank list from the "AI_Headquaters_Helper_CS".
            List<AI_Helper_CS> enemyTankList;
            if (thisRelationship == 0)
            {
                enemyTankList = AI_Headquaters_CS.instance.hostileTanksList;
            }
            else
            {
                enemyTankList = AI_Headquaters_CS.instance.friendlyTanksList;
            }

            // Find a new target.
            var mainCamera = Camera.main;
            var ray = mainCamera.ScreenPointToRay(screenPos);
            var camVector = ray.direction;
            var camPos = mainCamera.transform.position;
            var targetIndex = 0;
            var minAng = angleRange;
            for (int i = 0; i < enemyTankList.Count; i++)
            {
                if (enemyTankList[i].bodyTransform == null)
                { // The tank has been removed from the scene.
                    continue;
                }

                if (enemyTankList[i].bodyTransform.root.tag == "Finish" || (targetTransform && targetTransform == enemyTankList[i].bodyTransform))
                { // The tank is dead, or the tank is the same as the current target.
                    continue;
                }

                // Get the angle of the both vectors.
                var tankVector = enemyTankList[i].bodyTransform.position - camPos;
                var deltaAng = Mathf.Acos(Vector3.Dot(tankVector, camVector) / (tankVector.magnitude * camVector.magnitude)) * Mathf.Rad2Deg;
                if (deltaAng < minAng)
                {
                    targetIndex = i;
                    minAng = deltaAng;
                }
            }

            if (minAng != angleRange)
            { // Target is found.
                targetTransform = enemyTankList[targetIndex].bodyTransform;
                targetRigidbody = targetTransform.GetComponent<Rigidbody>();
                targetOffset = Vector3.zero;
                targetOffset.y = 0.5f;
                mode = 2; // Lock on.
                Switch_Mode();
                StartCoroutine("Send_Target_Position");
            }
            else
            { // Target cannot be found.
                // Get the angle to this tank.
                var tankVector = transform.position - camPos;
                var deltaAng = Mathf.Acos(Vector3.Dot(tankVector, camVector) / (tankVector.magnitude * camVector.magnitude)) * Mathf.Rad2Deg;
                if (deltaAng < minAng)
                { // This tank is near the touch position.
                    // Lock off.
                    mode = 0; // Keep the initial position.
                    Switch_Mode();
                }
            }
        }


        public IEnumerator Send_Target_Position()
        {
            // Send the target position to the "Camera_Rotation_CS" in the "Camera_Manager" object in the scene.
            yield return new WaitForFixedUpdate();
            if (Camera_Rotate_CS.instance)
            {
                Camera_Rotate_CS.instance.Look_At_Target(targetTransform.position);
            }
        }


        public void AI_Lock_On(Transform tempTransform)
        { // Called from "AI_Control_CS".
            targetTransform = tempTransform;
            targetRigidbody = targetTransform.GetComponent<Rigidbody>();
            mode = 2;
            Switch_Mode();
        }


        public void Lock_Off()
        { // Called from "Aiming_Control_Input_##_###_CS" and "AI_Control_CS".
            mode = 0;
            Switch_Mode();
        }


        public void AI_Random_Offset()
        { // Called from "Fire_Control_CS".

            // Set the new offset.
            Vector3 newOffset;
            newOffset.x = Random.Range(-0.5f, 0.5f);
            newOffset.y = Random.Range(0.0f, 1.0f);
            newOffset.z = Random.Range(-1.0f, 1.0f);
            targetOffset = newOffset;
        }


        void Destroyed_Linkage()
        { // Called from "Damage_Control_CS".
            Destroy(inputScript as Object);
            Destroy(this);
        }


        void Pause(bool isPaused)
        { // Called from "Game_Controller_CS".
            this.enabled = !isPaused;
        }
    }

}