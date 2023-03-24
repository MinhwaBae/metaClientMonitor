using UnityEngine;
using System.Collections.Generic;


namespace ChobiAssets.KTP
{

	public class AI_Headquaters_CS : MonoBehaviour
	{
        /*
		 * This script is attached to the "Game_Controller" in the scene.
		 * This script works in combination with "AI_Helper_CS" and "AI_Control_CS" in the tank.
		 * This script gives an oder that assign the target tank to "AI_Control_CS" in the tank.
		*/

        [Header("AI Headquaters settings")]
        [Tooltip("Interval for giving order to the AI tanks.")] public float orderInterval = 2.0f;


        float orderCount;

        // Referred to from "Aiming_Control_CS".
        public List<AI_Helper_CS> friendlyTanksList = new List<AI_Helper_CS>();
        public  List<AI_Helper_CS> hostileTanksList = new List<AI_Helper_CS>();

        [HideInInspector] public static AI_Headquaters_CS instance;


        void Awake()
        {
            instance = this;
        }


        public void Receive_Helpers(AI_Helper_CS helperScript)
        { // Called from "AI_Helper_CS", when the tank is spawend.

            // Add the script into the lists according to the relationship.
            switch (helperScript.idScript.relationship)
            {
                case 0:
                    friendlyTanksList.Add(helperScript);
                    break;
                case 1:
                    hostileTanksList.Add(helperScript);
                    break;
            }
        }


        void Update()
        {
            Count_Order_Interval();
        }


        void Count_Order_Interval()
        {
            orderCount += Time.deltaTime;
            if (orderCount > orderInterval)
            {
                orderCount = 0.0f;
                Give_Order();
            }
        }


        public void Give_Order()
        { // This function is called also from "AI_Control_CS" in the tanks, when its target has been destroyed.
            // Assign the target to all the AI tanks in the scene.
            Assign_Target(friendlyTanksList, hostileTanksList);
            Assign_Target(hostileTanksList, friendlyTanksList);
        }


        void Assign_Target(List<AI_Helper_CS> teamA, List<AI_Helper_CS> teamB)
        {
            // Assign the target to all the AI tanks in the scene.
            for (int i = 0; i < teamA.Count; i++)
            {
                var shortestDistance = Mathf.Infinity;
                var targetIsFound = false;
                var tempTargetIndex = 0;
                var aiScript = teamA[i].aiScript;
                if (aiScript == null || aiScript.noAttack || aiScript.detectFlag)
                { // The tank is not AI, or is set not to attack, or is detecting the current target.
                    continue;
                }
                for (int j = 0; j < teamB.Count; j++)
                {
                    if (teamB[j].bodyTransform.root.tag == "Finish")
                    { // The target is alredy destroyed.
                        continue;
                    } // The target is living.

                    // Find the closest enemy tank.
                    var distance = Vector3.Distance(teamA[i].bodyTransform.position, teamB[j].bodyTransform.position);
                    if (distance > aiScript.visibilityRadius || distance > shortestDistance)
                    { // The target is out of the visibility range, or is not the closest enemy.
                        continue;
                    } // The target is within the visibility range, and is the closest enemy.

                    // Check the AI can detect the target.
                    if (aiScript.Check_for_Assigning(teamB[j]) == true)
                    { // The target can be detected by the AI.
                        // Store the distance and the index.
                        shortestDistance = distance;
                        targetIsFound = true;
                        tempTargetIndex = j;
                    } // The target can not be detected by the AI.
                }

                if (targetIsFound)
                {
                    // Send the new target to "AI_Control_CS".
                    aiScript.Set_Target(teamB[tempTargetIndex]);
                }
            }
        }


        public void Remove_From_List(AI_Helper_CS helperScript)
        { // Called from "AI_Helper_CS", when the tank is destroyed.

            // Remove the script from the lists.
            switch (helperScript.idScript.relationship)
            {
                case 0:
                    friendlyTanksList.Remove(helperScript);
                    break;
                case 1:
                    hostileTanksList.Remove(helperScript);
                    break;
            }
        }


        public void Cease_Fire()
        { // Called from "Score_Manager_CS", when the mission is finished.

            // Call all the "AI_Control_CS" in the scene.
            for (int i = 0; i < friendlyTanksList.Count; i++)
            {
                if (friendlyTanksList[i].aiScript)
                {
                    friendlyTanksList[i].aiScript.Cease_Fire();
                }
            }
            for (int i = 0; i < hostileTanksList.Count; i++)
            {
                if (hostileTanksList[i].aiScript)
                {
                    hostileTanksList[i].aiScript.Cease_Fire();
                }
            }
        }
    }

}