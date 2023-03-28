using UnityEngine;
using System.Collections;
using UnityEngine.AI;


namespace ChobiAssets.KTP
{

    public class AI_Control_CS : MonoBehaviour
    {
        /*
		 * This script is attached to the "AI_Core" in the AI tank.
		 * This script controls the behavior of the AI tank.
		 * This script works in combination with "AI_Hand_CS" in the child object.
		 * This script requires also "AI_Headquaters_CS" in the scene to get the taget information.
		*/

        [Header("AI settings")]
        [Header("Patrol settings")]
        [Tooltip("Set 'Waypoint_Pack' in the scene.")] public GameObject waypointPack;
        [Tooltip("Allowable range of each waypoint.")] public float waypointRadius = 5.0f;
        [Tooltip("0 = Order, 1 = Random"), Range(0, 1)] public int patrolType = 1; // 0 = Order, 1= Random.

        [Header("Combat settings")]
        [Tooltip("Do not attack anything.")] public bool noAttack = false;
        [Tooltip("Do not chase the target.")] public bool breakthrough = false;
        [Tooltip("Visibility radius of this AI.")] public float visibilityRadius = 512.0f;
        [Tooltip("Angle range from the rear for the blind spot.")] public float deadAngle = 30.0f;
        [Tooltip("Distance that this AI will approach the target.")] public float approachDistance = 128.0f;
        [Tooltip("Time required for losing sight of the target.")] public float lostCount = 10.0f;

        [Header("Basic settings")]
        [Tooltip("Set 'AI_Eye' under this object.")] public Transform eyeTransform;
        [Tooltip("Set 'AI_Hand' under this object.")]  public AI_Hand_Control_CS handScript;
        [Tooltip("Minimum angle for pivot-turning.")] public float pivotTurnAngle = 30.0f;
        [Tooltip("Distance to the waypoint that the tank starts to slow down.")] public float slowDownRange = 20.0f;
        [Tooltip("Assign 'AI_Obstacle_Object' prefab.")] public GameObject obstaclePrefab;
        [Tooltip("Assign 'AI_NavMeshAgent_Object' Prefab.'")] public GameObject navMeshAgentPrefab;
        [Tooltip("Distance from the 'NavMeshAgent_Object'.")] public float agentDistance = 5.0f;
        [Tooltip("Additional distance changed according to the tank speed.")] public float agentAdditionalDistance = 10.0f;


        Transform thisTransform;
        NavMeshAgent navAgent;
        Transform navAgentTransform;
        Aiming_Control_CS aimingScript;
        float navAgentDistance;
        float updateDestinationCount;
        const float updateDestinationPeriod = 1.0f;
        bool ceaseFire;

        // Target informations.
        Transform targetTransform;
        Transform targetRootTransform;
        float targetUpperOffset;

        // For actions.
        [HideInInspector] public int actionType; // 0 = Defensive, 1 = Offensive. // Referred to from "UI_PosMarker_Control_CS".
        Transform[] wayPoints;
        int nextWayPoint = -1;
        Vector3 lookAtPosition;
        bool isStaying = false;

        // For searching the target.
        float searchingCount;
        const float searchingInterval = 1.0f;
        float targetDistance;
        [HideInInspector] public bool detectFlag; // Referred to from "AI_Headquaters_CS" and "Fire_Control_CS".
        float losingCount;
        bool wakefulFlag;
        int wakefulFlagTimerID;

        // For offensive actions.
        Fire_Spawn_CS fireSpawnScript;
        float castRayCount;
        bool hasApproached;

        // For driving.
        bool isEscapingFromStuck = false;
        float drivingTargetAngle;
        float drivingTargetDistance;
        float drivingNextCornerAngle;
        [HideInInspector] public float speedOrder; // Referred to from "Wheel_Control_CS".
        [HideInInspector] public float turnOrder; // Referred to from "Wheel_Control_CS".

        // For navigation.
        Wheel_Control_CS wheelControlScript;


        void Start()
        {
            Initialize();
        }


        void Initialize()
        {
            thisTransform = transform;

            // Get the references via "ID_Control_CS".
            var idScript = GetComponentInParent<ID_Control_CS>();
            aimingScript = idScript.aimingScript;
            fireSpawnScript = idScript.fireSpawnScript;
            wheelControlScript = idScript.wheelControlScript;

            // Get "AI_Eye"
            if (eyeTransform == null)
            {
                eyeTransform = thisTransform.Find("AI_Eye");
            }

            // Get "AI_Hand_Control_CS" script.
            if (handScript == null)
            {
                handScript = GetComponentInChildren<AI_Hand_Control_CS>();
            }

            // Instantiate NavMeshAgent object.
            if (navMeshAgentPrefab == null)
            {
                Debug.LogError("'Nav Mesh Agent Prefab' is not assigned.");
                Destroy(this);
                return;
            }
            var agentObject = Instantiate(navMeshAgentPrefab, thisTransform.position, thisTransform.rotation, thisTransform.parent.parent) as GameObject;
            navAgent = agentObject.GetComponent<NavMeshAgent>();
            navAgentTransform = agentObject.transform;
            navAgent.acceleration = 120.0f;

            // Set up the waypoints.
            Set_WayPoints();
            
            // Set the first waypoint.
            Update_Next_WayPoint();
        }


        void Set_WayPoints()
        {
            if (waypointPack)
            {
                int childCount = waypointPack.transform.childCount;
                if (childCount > 1)
                { // The waypoint pack has more than two waypoints.
                    wayPoints = new Transform[childCount];
                    for (int i = 0; i < childCount; i++)
                    {
                        wayPoints[i] = waypointPack.transform.GetChild(i);
                    }
                    return;
                }
                else if (childCount == 1)
                { // The waypoint pack has only one waypoint.
                    wayPoints = new Transform[1];
                    wayPoints[0] = waypointPack.transform.GetChild(0);
                    return;
                }
            }
            // The waypoint pack has no point, or is not assigined.

            // Create a new waypoint.
            wayPoints = new Transform[1];
            GameObject newWayPoint = new GameObject("Waypoint (1)");
            newWayPoint.transform.parent = thisTransform.root;
            newWayPoint.transform.position = thisTransform.position;
            newWayPoint.transform.rotation = thisTransform.rotation;
            wayPoints[0] = newWayPoint.transform;
        }


        void Update_Next_WayPoint()
        {
            switch (patrolType)
            {
                case 0: // Order
                    nextWayPoint += 1;
                    if (nextWayPoint >= wayPoints.Length)
                    {
                        nextWayPoint = 0;
                    }
                    break;

                case 1: // Random
                    nextWayPoint = Random.Range(0, wayPoints.Length);
                    break;
            }

            // Update the destination of the NavMeshAgent.
            navAgent.SetDestination(wayPoints[nextWayPoint].position);
        }


        void Update()
        {
            // Control the NavMeshAgent.
            var navAgentDistance = Vector3.Distance(navAgentTransform.position, thisTransform.position);
            if (navAgentDistance > agentDistance + agentAdditionalDistance)
            { // The NavMeshAgent is too far.
                // Reset the position.
                navAgent.nextPosition = thisTransform.position;
            }
            else
            {
                // Control the speed to keep the distance from the tank.
                float tempRate = Mathf.Pow(wheelControlScript.currentVelocityMag / wheelControlScript.maxSpeed, 2.0f);
                tempRate = Mathf.Clamp(tempRate, 0.0f, 1.0f);
                navAgent.speed = Mathf.Lerp(64.0f, 0.0f, navAgentDistance / (agentDistance + (agentAdditionalDistance * tempRate)));
            }
            
            // Search the target.
            if (targetTransform)
            {
                Search_Target();
            }

            // Action
            switch (actionType)
            {
                case 0: // Defensive.
                    WayPoint_Mode();
                    break;

                case 1: // Offensive.
                    if (targetTransform)
                    { // The target exists.
                        if (breakthrough == true)
                        {
                            // Keep going around the waypoaints while attacking the target.
                            Breakthrough_Mode();
                        }
                        else
                        {
                            // Chase the target while attaking it.
                            Chase_Mode();
                        }
                    }
                    else
                    { // The target does not exist. >> The target might be respawned, or be removed from the scene.
                        Lost_Target();
                        return;
                    }
                    break;
            }

            // Auto brake function.
            if ((handScript && handScript.isWorking) || ceaseFire)
            { // An obstacle is detected in fornt of the tank, or cease fire now.
                drivingTargetDistance = 0.0f; // The tank will stop, and will be allowed only to turn.
            }

            // Calculate the "speedOrder" and "turnOrder" for driving.
            Auto_Drive();
        }


        void Search_Target()
        {
            // Check the interval.
            searchingCount -= Time.deltaTime;
            if (searchingCount > 0.0f)
            {
                return;
            }
            searchingCount = searchingInterval;

            // Check the target state.
            if (targetRootTransform.tag == "Finish")
            { // The target is already dead.
                Lost_Target();
                // Call "AI_Headquaters_CS" to get a new order.
                if (AI_Headquaters_CS.instance)
                {
                    AI_Headquaters_CS.instance.Give_Order();
                }
                return;
            }

            // Detect the target.
            Vector3 targetPosition = targetTransform.position + (targetTransform.up * targetUpperOffset);
            targetDistance = Vector3.Distance(eyeTransform.position, targetPosition);
            if (targetDistance < visibilityRadius)
            { // The target is within the visibility range.
                if (Cast_Line_For_Searching(targetPosition) == true)
                { // The target is detected from the "AI_Eye".
                    detectFlag = true;
                    switch (actionType)
                    {
                        case 0: // Defensive.
                            actionType = 1;
                            if (breakthrough == false)
                            { // Chase the target.
                                isStaying = false;
                                // Move the NavMeshAgent to this position, so that the NavMeshAgent can find a new path smoothly.
                                navAgent.Warp(thisTransform.position);
                                // Set the 'updateDestinationCount', so that the destination of the NavMeshAgent can be updated soon.
                                updateDestinationCount = Mathf.Infinity;
                            }

                            // Call the "Aiming_Control_CS" to lock on the target.
                            if (aimingScript)
                            {
                                aimingScript.AI_Lock_On(targetTransform);
                            }
                            break;

                        case 1: // Offensive..
                            // Continue to chase the target.
                            losingCount = lostCount;
                            break;
                    }
                    return;
                }
                // The target is not detected from the "AI_Eye".

            }
            // The target is out of the visibility range.

            // The target cannot be detected.
            detectFlag = false;
            switch (actionType)
            {
                case 0: // Defensive.
                    break;

                case 1: // Offensive..
                    losingCount -= Time.deltaTime + searchingInterval;
                    if (losingCount < 0.0f)
                    { // The AI has lost the target.
                        Lost_Target();
                    }
                    break;
            }
        }


        bool Cast_Line_For_Searching(Vector3 targetPosition)
        {
            // Check the dead angle.
            if (actionType == 0 && wakefulFlag == false)
            { // Defensive && not attacked.
                // Get the angle to the target.
                float theta = Mathf.Abs(Calculate_2D_Angle(targetPosition));
                if (180.0f - theta < deadAngle)
                { // The target is in the dead angle.
                    return false;
                }
            }

            // Cast a line from the "AI_Eye" to the target.
            RaycastHit raycastHit;
            if (Physics.Linecast(eyeTransform.position, targetPosition, out raycastHit, Layer_Settings_CS.Layer_Mask))
            {
                if (raycastHit.transform.root == targetRootTransform)
                { // The ray hits the target.
                    return true;
                }
                else
                { // The ray hits other object.
                    return false;
                }
            }
            else
            { // The ray does not hit anything. >> There is no obstacle between the eye and the target.
                return true;
            }
        }


        public IEnumerator Wake_Up_Timer()
        { // Called from "Damage_Control_CS", when the AI tank is attacked.
            wakefulFlag = true;
            wakefulFlagTimerID += 1;
            int thisTimerID = wakefulFlagTimerID;

            float count = 0.0f;
            while (count < 5.0f)
            {
                if (thisTimerID != wakefulFlagTimerID)
                { // The tank should be attacked again.
                    yield break;
                }

                count += Time.deltaTime;
                yield return null;
            }

            wakefulFlagTimerID = 0;
            wakefulFlag = false;
        }


        void WayPoint_Mode()
        {
            // Update the destination.
            updateDestinationCount += Time.deltaTime;
            if (updateDestinationCount > updateDestinationPeriod)
            {
                navAgent.SetDestination(wayPoints[nextWayPoint].position);
                updateDestinationCount = 0.0f;
            }

            // Check the distance to the waypoint.
            if (wayPoints.Length > 1)
            { // There are more than two waypoints.
                if (Vector3.Distance(thisTransform.position, wayPoints[nextWayPoint].position) < waypointRadius)
                { // The tank has arriveded at the next waypoint.
                    // Update the next waypoint.
                    Update_Next_WayPoint();
                }
                else
                { // The tank does not arrive at the next waypoint.
                    // Move to the NavMeshAgent.
                    Set_Driving_Target_Angle_And_Distance();
                }
            }
            else
            { // There is only one waypoint.
                float distanceToWaypoint = Vector3.Distance(thisTransform.position, wayPoints[0].position);
                if (isStaying)
                { // The tank is staying now.
                    if (distanceToWaypoint > waypointRadius + 5.0f)
                    { // The tank has moved away from the waypoint.
                        isStaying = false;
                    }
                }
                else
                { // The tank is not staying now.
                    if (distanceToWaypoint < waypointRadius)
                    { // The tank has arrived at the waypoint.
                        // Set the "lookAtPosition".
                        lookAtPosition = wayPoints[0].position + (wayPoints[0].forward * 100.0f);
                        isStaying = true;
                    }
                }

                // Set the "drivingTargetAngle" and "drivingTargetDistance".
                if (isStaying)
                { // The tank is near the waypoint.
                    // Face the same direction as the waypoint.
                    drivingTargetAngle = Calculate_2D_Angle(lookAtPosition);
                    // Stay.
                    drivingTargetDistance = 0.0f;
                }
                else
                { // The tank is away from the waypoint.
                    // Move to the NavMeshAgent.
                    Set_Driving_Target_Angle_And_Distance();
                }
            }
        }


        void Breakthrough_Mode()
        {
            // Keep patrolling the waypoints.
            WayPoint_Mode();

            // Check the gun can aim the target or not.
            if (detectFlag == true)
            { // The target is detected.
                castRayCount += Time.fixedDeltaTime;
                if (castRayCount > 2.0f)
                {
                    castRayCount = 0.0f;
                    fireSpawnScript.canAim = Set_Can_Aim();
                }

            }
            else
            { // The target is not detected.
                fireSpawnScript.canAim = false;
            }
        }


        void Chase_Mode()
        {
            // Update the destination.
            updateDestinationCount += Time.deltaTime;
            if (updateDestinationCount > updateDestinationPeriod)
            {
                navAgent.SetDestination(targetTransform.position);
                updateDestinationCount = 0.0f;
            }

            // Check the gun can aim the target or not.
            if (detectFlag == true)
            { // The target is detected.
                castRayCount += Time.fixedDeltaTime;
                if (castRayCount > 2.0f)
                {
                    castRayCount = 0.0f;
                    fireSpawnScript.canAim = Set_Can_Aim();
                }

            }
            else
            { // The target is not detected.
                fireSpawnScript.canAim = false;
            }

            // Check the tank is within the approach distance or not.
            if (targetDistance < approachDistance)
            { // The target is within the approach distance.
                if (fireSpawnScript.canAim || approachDistance == Mathf.Infinity)
                { // The gun can aim the target, or the approach distance is set to infinity.
                    hasApproached = true;
                }
                else
                { // The gun can not aim the tagert.
                    hasApproached = false;
                }
            }
            else
            { // The target is out of the approach distance.
                hasApproached = false;
            }

            // Set the "drivingTargetAngle" and "drivingTargetDistance".
            if (hasApproached)
            { // The tank has approached the target and can aim it.
                // Stay.
                drivingTargetDistance = 0.0f;

                // Get the angle to the target.
                drivingTargetAngle = Calculate_2D_Angle(targetTransform.position);
                if (approachDistance != Mathf.Infinity && Mathf.Abs(drivingTargetAngle) < 60.0f)
                { // The target is almost in front.
                    // Need not to face the target.
                    drivingTargetAngle = 0.0f;
                } // Face the target.
            }
            else
            { // The tank has not approached the target yet, or cannot aim it.
                // Move to the NavMeshAgent.
                Set_Driving_Target_Angle_And_Distance();
            }
        }


        bool Set_Can_Aim()
        {
            // Set "canAim" in the "Fire_Spawn_CS" referred to from "Fire_Control_CS".
            // And check the guns can aim the target or not.
            bool flag;

            // Cast a line from the "Fire_Point" to the target.
            RaycastHit raycastHit;
            if (Physics.Linecast(fireSpawnScript.transform.position, aimingScript.targetPosition, out raycastHit, Layer_Settings_CS.Layer_Mask))
            {
                if (raycastHit.transform.root == targetRootTransform)
                { // The line hits the target.
                    flag = true;
                }
                else
                { // The line hits other object.
                    flag = false;
                }
            }
            else
            { // The line does not hit anyhing. >> There is no obstacle between the muzzle and the target.
                flag = true;
            }
            return flag;
        }


        float Calculate_2D_Angle(Vector3 targetPosition)
        {
            // Calculate the angle to the target for driving.
            Vector3 localPosition3D = thisTransform.InverseTransformPoint(targetPosition);
            Vector2 localPosition2D;
            localPosition2D.x = localPosition3D.x;
            localPosition2D.y = localPosition3D.z;
            return Vector2.Angle(Vector2.up, localPosition2D) * Mathf.Sign(localPosition2D.x);
        }


        void Set_Driving_Target_Angle_And_Distance()
        {
            // Check the state of the NavMeshAgent.
            if (navAgent.path.corners.Length == 0)
            { // Something wrong in the NavMeshAgent.
                // Do not move.
                drivingTargetAngle = 0.0f;
                drivingTargetDistance = 0.0f;
                return;
            }

            // Get the angle to the NavMeshAgent.
            drivingTargetAngle = Calculate_2D_Angle(navAgentTransform.position);

            // Get the distance to the next corner.
            if (navAgent.path.corners.Length > 1)
            { // The corners [0] should be the NavMeshAgent it self.
                drivingTargetDistance = Vector3.Distance(thisTransform.position, navAgent.path.corners[1]);
                if (navAgent.path.corners.Length > 2)
                { // The next corner (corners [1]) should not be the destination.
                    // Get the angle of the corners [1]. (this position >> corners [1] >> corners [2]")
                    Vector3 vecA = thisTransform.position - navAgent.path.corners[1];
                    Vector3 vecB = navAgent.path.corners[2] - navAgent.path.corners[1];
                    float theta = Mathf.Acos(Vector3.Dot(vecA, vecB) / (vecA.magnitude * vecB.magnitude)) * Mathf.Rad2Deg;
                    drivingNextCornerAngle = 180.0f - theta;
                }
                else
                { // The next corner (corners [1]) should be the destination (the next waypoint).
                    drivingNextCornerAngle = 180.0f;
                }
            }
            else
            { // The corners [0] should be the destination (the next waypoint).
                drivingTargetDistance = Vector3.Distance(thisTransform.position, navAgent.path.corners[0]);
                drivingNextCornerAngle = 180.0f;
            }
        }


        public void Set_Target(AI_Helper_CS targetAIHelperScript)
        { // Called from "AI_Headquaters_CS" in the scene.
            if (targetTransform == targetAIHelperScript.bodyTransform)
            { // The sent target is the same as the current target.
                return;
            }

            // Reset the values.
            Lost_Target();
            targetTransform = targetAIHelperScript.bodyTransform;
            targetRootTransform = targetAIHelperScript.bodyTransform.root;
            targetUpperOffset = targetAIHelperScript.visibilityUpperOffset;
        }


        void Lost_Target()
        {
            if (breakthrough == false)
            { // Not breakthrough >> The tank is chasing the target until now.
                // Move the NavMeshAgent to this position, so that the NavMeshAgent can find a new path smoothly.
                navAgent.Warp(thisTransform.position);
            }

            // Reset the values.
            actionType = 0;
            updateDestinationCount = 0.0f;
            searchingCount = 0.0f;
            targetTransform = null;
            targetRootTransform = null;
            detectFlag = false;
            losingCount = lostCount;
            hasApproached = false;
            isStaying = false;

            // Call the "Aiming_Control_CS" to lock off the target.
            if (aimingScript)
            {
                aimingScript.Lock_Off();
            }

            // Update the destination of the NavMeshAgent.
            navAgent.SetDestination(wayPoints[nextWayPoint].position);
        }


        void Auto_Drive()
        {
            // Calculate "speedOrder" and "turnOrder".
            float sign = Mathf.Sign(drivingTargetAngle);
            drivingTargetAngle = Mathf.Abs(drivingTargetAngle);
            if (drivingTargetAngle > 2.0f)
            { // Turn.
                if (drivingTargetAngle > pivotTurnAngle)
                { // Pivot turn.
                    turnOrder = 1.0f * sign;
                    speedOrder = 0.0f;
                    return;
                }
                // Brake turn.
                turnOrder = Mathf.Lerp(0.0f, 1.0f, drivingTargetAngle / pivotTurnAngle) * sign;
                if (drivingTargetDistance == 0.0f)
                {
                    speedOrder = 0.0f;
                    return;
                }
                speedOrder = 1.0f - turnOrder;
                float currentMinSpeedRate = Mathf.Lerp(1.0f, 0.0f, drivingNextCornerAngle / 45.0f);
                speedOrder *= Mathf.Lerp(currentMinSpeedRate, 1.0f, drivingTargetDistance / slowDownRange);
                speedOrder = Mathf.Clamp(speedOrder, 0.0f, 1.0f);
                return;
            }
            else
            { // No turn.
                turnOrder = 0.0f;
                if (drivingTargetDistance == 0.0f)
                {
                    speedOrder = 0.0f;
                    return;
                }
                float currentMinSpeedRate = Mathf.Lerp(1.0f, 0.0f, drivingNextCornerAngle / 30.0f);
                speedOrder = Mathf.Lerp(currentMinSpeedRate, 1.0f, drivingTargetDistance / slowDownRange);
                return;
            }
        }


        public void Escape_From_Stuck()
        { // Called from "AI_Hand_Control_CS" when the tank gets stuck.
            // Move the NavMeshAgent to this position, so that the NavMeshAgent can find a new path smoothly.
            navAgent.Warp(thisTransform.position);

            switch (actionType)
            {
                case 0: // Defensive.
                    if (isEscapingFromStuck == false && Random.Range(0, 3) == 0)
                    {
                        StartCoroutine("Create_NavMeshObstacle_Object");
                    }
                    else
                    {
                        Update_Next_WayPoint();
                    }
                    break;

                case 1: // Offensive.
                    if (breakthrough == true)
                    { // The AI tank never chase the target.
                        if (isEscapingFromStuck == false && Random.Range(0, 3) == 0)
                        {
                            StartCoroutine("Create_NavMeshObstacle_Object");
                        }
                        else
                        {
                            Update_Next_WayPoint();
                        }
                        break;

                    }
                    else
                    { // The AI tank is chasing the target.
                        if (isEscapingFromStuck == false && Random.Range(0, 2) == 0)
                        {
                            StartCoroutine("Create_NavMeshObstacle_Object");
                        }
                    }
                    break;
            }
        }


        IEnumerator Create_NavMeshObstacle_Object()
        {
            if (obstaclePrefab == null)
            {
                yield break;
            }
            isEscapingFromStuck = true;

            // Spawn the "Obstacle_Prefab".
            Instantiate(obstaclePrefab, thisTransform.position, thisTransform.rotation);

            // Wait.
            yield return new WaitForSeconds(20.0f);

            isEscapingFromStuck = false;
        }


        public bool Check_for_Assigning(AI_Helper_CS targetAIHelperScript)
        { // Called from "AI_Headquaters_CS" in the scene.
            // Check this tank can detect the target or not, by casting a line.
            Vector3 tempTargetPosition = targetAIHelperScript.bodyTransform.position + (targetAIHelperScript.bodyTransform.up * targetAIHelperScript.visibilityUpperOffset);
            RaycastHit raycastHit;
            if (Physics.Linecast(eyeTransform.position, tempTargetPosition, out raycastHit, Layer_Settings_CS.Layer_Mask))
            { // The line hits anything.
                if (raycastHit.transform.root == targetAIHelperScript.bodyTransform.root)
                { // The line hits the target.
                    return true;
                }
                else
                { // The line hits other object.
                    return false;
                }
            }
            else
            { // The line does not hit anything. >> There is no obstacle between this eye and the target.
                return true;
            }
        }


        void Destroyed_Linkage()
        { // Called from "Damage_Control_CS".
            Destroy(navAgent.gameObject);
            Destroy(this.gameObject);
        }


        public void Cease_Fire()
        { // Called from "AI_Headquaters_CS".

            // Stop the tank at the current position.
            ceaseFire = true;

            // Stop attacking.
            noAttack = true;

            // Reset the behavior.
            Lost_Target();
        }


        void Pause(bool isPaused)
        { // Called from "Game_Controller_CS".
            this.enabled = !isPaused;
        }

    }

}