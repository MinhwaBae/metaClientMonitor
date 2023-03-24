using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;


namespace ChobiAssets.KTP
{

    [System.Serializable]
    class ScoreProp
    {
        public float currentDurability;
        public int killsCount;
        public int killedCount;
    }


    public class Score_Manager_CS : MonoBehaviour
    {
        /* 
		 * This script is attached to "Game_Controller" in the scene.
		 * This script manages the killed counts of each team.
         * And control the messages and the music according to the game situation.
		*/

        [Header("Stage settings")]
        [Tooltip("The number of kills to win or lose the game.")] public int requiredKillCount = 10;
        [Tooltip("The tank is destroyed when it falls under this height.")] public float killHight = -10.0f;
        [Tooltip("Set the text for displaying the messages.")] public Message_Motion_CS messageScript;
        [Tooltip("Message for mission clear.")] public string clearString = "Mission Clear";
        [Tooltip("Message for mission failed.")] public string failedString = "Mission Failed";
        [Tooltip("Color of the messages. (Shadow & Outline)")] public Color friendColor = Color.blue;
        [Tooltip("Color of the messages. (Shadow & Outline)")] public Color enemyColor = Color.red;
        [Tooltip("Cease fire after the mission is over.")] public bool ceaseFire = true;
        [Tooltip("Canvas(es) displayed when the mission is started.")] public Canvas[] startCanvases;
        [Tooltip("Canvas(es) displayed when the mission is finished.")] public Canvas[] resultCanvases;


        int friendTotalKilled;
        int enemyTotalKilled;
        bool isFinished = false;

        Dictionary<Spawner_CS, ScoreProp> tanksDictionary = new Dictionary<Spawner_CS, ScoreProp>();

        [HideInInspector] public static Score_Manager_CS instance;


        void Awake()
        {
            instance = this;
        }


        void Start()
        {
            Initialize();
        }


        void Initialize()
        {
            // Call "KillCounter_CS" in the scene to initialize the values.
            if (KillCounter_CS.instance)
            {
                KillCounter_CS.instance.Update_KillCounter(true, requiredKillCount, 0, 0);
                KillCounter_CS.instance.Update_KillCounter(false, requiredKillCount, 0, 0);
            }

            // Start the opening effects.
            StartCoroutine("Opening_Effects");
        }


        public void Get_Spawner(Spawner_CS spawnerScript)
        { // Called from "Spawner_CS" at the start.

            // Add the "Spawner_CS" to the Dictionary.
            var tempScoreProp = new ScoreProp();
            tanksDictionary.Add(spawnerScript, tempScoreProp);

            // Call "Tank_List_CS" in the scene to add the "Spawner_CS" into the list.
            if (Tank_List_CS.instance)
            {
                Tank_List_CS.instance.Add_To_List(spawnerScript);
            }
        }


        IEnumerator Opening_Effects()
        {
            // Disallow the pause.
            if (Game_Controller_CS.instance)
            {
                Game_Controller_CS.instance.allowPause = false;
            }

            // Wait for the fade-in.
            yield return new WaitForSeconds(0.5f);

            // Show the opening message.
            if (messageScript)
            {
                messageScript.Show_Message("Mission Start", 1.0f, friendColor);
            }

            // Start BGM.
            if (Audio_Manager_CS.instance)
            {
                Audio_Manager_CS.instance.StartCoroutine("Start_Music", 0.5f);
            }

            // Wair for the message scroll.
            yield return new WaitForSeconds(1.5f);

            // Enable the start canvases.
            for (int i = 0; i < startCanvases.Length; i++)
            {
                startCanvases[i].enabled = true;
            }

            // Wait.
            yield return new WaitForSeconds(2.0f);

            // Disable the start canvases.
            for (int i = 0; i < startCanvases.Length; i++)
            {
                startCanvases[i].enabled = false;
            }

            // Allow the pause.
            if (Game_Controller_CS.instance)
            {
                Game_Controller_CS.instance.allowPause = true;
            }
        }


        public void Update_Current_Durability(Spawner_CS spwanerScript, float currentDurability, float initialDurability)
        { // Called from "Damage_Control_CS" in each tank, when the durability is changed.

            // Check the mission is finished.
            if (isFinished)
            {
                return;
            }

            // Update the dictionary.
            tanksDictionary[spwanerScript].currentDurability = currentDurability;

            // Call "Tank_List_CS" to update the list.
            if (Tank_List_CS.instance)
            {
                var valueString = Mathf.Ceil(currentDurability).ToString() + " / " + initialDurability.ToString();
                Tank_List_CS.instance.Update_Tank_List(spwanerScript, 0, valueString);
            }
        }


        public void Update_Kills_Count (Spawner_CS spwanerScript)
        { // Called from "Bullet_Nav_CS" in each bullet, when the bullet kills the target.
            
            // Check the mission is finished.
            if (isFinished)
            {
                return;
            }

            // Update the dictionary.
            tanksDictionary[spwanerScript].killsCount += 1;

            // Call "Tank_List_CS" to update the list.
            if (Tank_List_CS.instance)
            {
                var valueString = tanksDictionary[spwanerScript].killsCount.ToString();
                Tank_List_CS.instance.Update_Tank_List(spwanerScript, 1, valueString);
            }
        }


        public void Update_Killed_Count(Spawner_CS spwanerScript)
        { // Called from "ID_Control_CS" when the tank is destroyed.

            // Check the mission is finished.
            if (isFinished)
            {
                return;
            }

            // Update the dictionary.
            tanksDictionary[spwanerScript].killedCount += 1;

            // Call "Tank_List_CS" to update the list.
            if (Tank_List_CS.instance)
            {
                var valueString = tanksDictionary[spwanerScript].killedCount.ToString();
                Tank_List_CS.instance.Update_Tank_List(spwanerScript, 2, valueString);
            }

            // Update the total killed counts.
            Update_Total_Killed(spwanerScript.relationship, spwanerScript.cost);
        }


        public void Update_Total_Killed(int relationship, int count)
        { // Called also from "Event_Collider_CS".
            // Update the total killed counts.
            switch (relationship)
            {
                case 0: // Friend.
                    friendTotalKilled += count;
                    break;

                case 1: // Enemy.
                    enemyTotalKilled += count;
                    break;
            }

            // Call "KillCounter_CS" in the scene to update the values.
            if (KillCounter_CS.instance)
            {
                KillCounter_CS.instance.Update_KillCounter((relationship == 0), requiredKillCount, enemyTotalKilled, friendTotalKilled);
            }

            // Check the total killed counts.
            if (enemyTotalKilled >= requiredKillCount)
            { // Player has won.
                StartCoroutine("Ending_Effects", true);
                return;
            }
            if (friendTotalKilled >= requiredKillCount)
            { // Player has lost.
                StartCoroutine("Ending_Effects", false);
                return;
            }

            // Show message.
            if (messageScript)
            {
                switch (relationship)
                {
                    case 0: // Friend.
                        messageScript.Show_Message("Lost " + count + " pts", 1.0f, enemyColor);
                        break;

                    case 1: // Enemy.
                        messageScript.Show_Message("Got " + count + " pts", 1.0f, friendColor);
                        break;
                }
            }
        }


        IEnumerator Ending_Effects(bool hasWon)
        {
            // Wait a frame to get the last kill count.
            yield return null;

            // Set the finished flag.
            isFinished = true;

            // Call the "Game_Controller_CS" in the scene.
            if (Game_Controller_CS.instance)
            {
                // Disallow the paues.
                Game_Controller_CS.instance.allowPause = false;

                // Show the cursor.
                Game_Controller_CS.instance.Switch_Cursor(true);
            }

            // Disable all the "Spawner_CS" in the scene.
            var spawnerScripts = FindObjectsOfType<Spawner_CS>();
            for (int i = 0; i < spawnerScripts.Length; i++)
            {
                spawnerScripts[i].enabled = false;
            }

            // Make the AI tanks cease fire.
            if (ceaseFire && AI_Headquaters_CS.instance)
            {
                AI_Headquaters_CS.instance.Cease_Fire();
            }

            // Show message.
            if (messageScript)
            {
                if (hasWon)
                {
                    messageScript.Show_Message(clearString, 3.0f, friendColor);
                }
                else
                {
                    messageScript.Show_Message(failedString, 3.0f, enemyColor);
                }
            }

            // Play the victory music.
            if (Audio_Manager_CS.instance)
            {
                if (hasWon)
                {
                    Audio_Manager_CS.instance.StartCoroutine("Play_Victory", 0.5f);
                }
                else
                {
                    Audio_Manager_CS.instance.StartCoroutine("Play_Defeat", 0.5f);
                }
            }

            // Wair for the message scroll.
            yield return new WaitForSeconds(3.0f);

            // Enable the result canvases.
            for (int i = 0; i < resultCanvases.Length; i++)
            {
                resultCanvases[i].enabled = true;
            }
            
        }

    }

}
