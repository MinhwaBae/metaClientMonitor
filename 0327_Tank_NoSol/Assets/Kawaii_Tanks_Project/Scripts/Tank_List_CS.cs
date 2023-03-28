using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


namespace ChobiAssets.KTP
{

    [System.Serializable]
    class TextListProp
    {
        public Text hpText;
        public Text killsText;
        public Text killedText;
    }


    [DefaultExecutionOrder(+1)] // (Note.) This script is executed after other scripts, in order to sort the lists created in Start().
    public class Tank_List_CS : MonoBehaviour
    {
        /* 
		 * This script is attached to "Canvas_Tank_List" in the scene.
		 * This script creates and displays the list of tanks in the scene.
		 * This script refers to the score values in "Spawner_CS" in the scene.
		*/

        [Tooltip("Text for the name.")] public Text friendNameText;
        [Tooltip("Text for the cost(Kill Count).")] public Text friendCostText;
        [Tooltip("Text for the AP(Attack Force).")] public Text friendAPText;
        [Tooltip("Text for the HP(Durability).")] public Text friendHPText;
        [Tooltip("Text for the Kills.")] public Text friendKillsText;
        [Tooltip("Text for the Killed.")] public Text friendKilledText;

        [Tooltip("Text for the name.")] public Text enemyNameText;
        [Tooltip("Text for the cost(Kill Count).")] public Text enemyCostText;
        [Tooltip("Text for the AP(Attack Force).")] public Text enemyAPText;
        [Tooltip("Text for the HP(Durability).")] public Text enemyHPText;
        [Tooltip("Text for the Kills.")] public Text enemyKillsText;
        [Tooltip("Text for the Killed.")] public Text enemyKilledText;


        List<Spawner_CS> friendSpawnerList = new List<Spawner_CS>();
        List<Spawner_CS> enemySpawnerList = new List<Spawner_CS>();
        List<TextListProp> friendTextList = new List<TextListProp>();
        List<TextListProp> enemyTextList = new List<TextListProp>();
        Canvas thisCanvas;

        [HideInInspector] public static Tank_List_CS instance;


        void Awake()
        {
            instance = this;

            thisCanvas = GetComponent<Canvas>();

            // Hide the original texts.
            friendNameText.enabled = false;
            friendCostText.enabled = false;
            friendAPText.enabled = false;
            friendHPText.enabled = false;
            friendKillsText.enabled = false;
            friendKilledText.enabled = false;
            enemyNameText.enabled = false;
            enemyCostText.enabled = false;
            enemyAPText.enabled = false;
            enemyHPText.enabled = false;
            enemyKillsText.enabled = false;
            enemyKilledText.enabled = false;
        }


        public void Add_To_List(Spawner_CS spawnerScript)
        { // Called from "Score_Manager_CS" in the Start().

            // Add the "Spawner_CS" to the list.
            switch (spawnerScript.relationship)
            {
                case 0: // Friend
                    friendSpawnerList.Add(spawnerScript);
                    break;

                case 1: // Enemy
                    enemySpawnerList.Add(spawnerScript);
                    break;
            }
        }


        void Start()
        { // Called after all the "Spawner_CS" scripts are added to the list.

            // Sort the spawner lists by order in the Hierarchy window.
            friendSpawnerList.Sort(delegate (Spawner_CS a, Spawner_CS b) { return a.transform.GetSiblingIndex() - b.transform.GetSiblingIndex(); });
            enemySpawnerList.Sort(delegate (Spawner_CS a, Spawner_CS b) { return a.transform.GetSiblingIndex() - b.transform.GetSiblingIndex(); });

            // Setup the texts.
            Setup_Texts();
        }


        void Setup_Texts()
        {
            
            // Duplicate the original texts, and set them into the text lists.
            for (int i = 0; i < friendSpawnerList.Count; i++)
            {
                Duplicate_Text(friendNameText, i, friendSpawnerList[i].name);
                Duplicate_Text(friendCostText, i, friendSpawnerList[i].cost.ToString());
                Duplicate_Text(friendAPText, i, friendSpawnerList[i].attackForce.ToString());

                var tempTextListProp = new TextListProp();
                tempTextListProp.hpText = Duplicate_Text(friendHPText, i, friendSpawnerList[i].durability.ToString());
                tempTextListProp.killsText = Duplicate_Text(friendKillsText, i, "0");
                tempTextListProp.killedText = Duplicate_Text(friendKilledText, i, "0");
                friendTextList.Add(tempTextListProp);
            }

            for (int i = 0; i < enemySpawnerList.Count; i++)
            {
                Duplicate_Text(enemyNameText, i, enemySpawnerList[i].name);
                Duplicate_Text(enemyCostText, i, enemySpawnerList[i].cost.ToString());
                Duplicate_Text(enemyAPText, i, enemySpawnerList[i].attackForce.ToString());

                var tempTextListProp = new TextListProp();
                tempTextListProp.hpText = Duplicate_Text(enemyHPText, i, enemySpawnerList[i].durability.ToString());
                tempTextListProp.killsText = Duplicate_Text(enemyKillsText, i, "0");
                tempTextListProp.killedText = Duplicate_Text(enemyKilledText, i, "0");
                enemyTextList.Add(tempTextListProp);
            }
        }


        Text Duplicate_Text(Text text, int count, string value)
        {
            // Duplicate the original text, and setup the new one.
            Vector3 pos = text.rectTransform.position;
            pos.y -= text.rectTransform.rect.height * count * thisCanvas.scaleFactor;
            Text tempText = Instantiate(text, pos, text.rectTransform.rotation, text.rectTransform.parent) as Text;
            tempText.text = value;
            tempText.enabled = true;

            // Set the height of the content.
            RectTransform content = text.transform.parent.GetComponent<RectTransform>();
            Vector2 tempSize = content.sizeDelta;
            tempSize.y = text.rectTransform.rect.height * (count + 1);
            content.sizeDelta = tempSize;

            // Return the new Text.
            return tempText;
        }


        public void Update_Tank_List(Spawner_CS spawnerScript, int scoreType, string valueString)
        { // Called from "Score_Manager_CS", when the score values are changed.
            var index = 0;
            switch(spawnerScript.relationship)
            {
                case 0: // Friend
                    index = friendSpawnerList.IndexOf(spawnerScript);
                    Update_Text_Value(friendTextList[index], scoreType, valueString);
                    break;

                case 1: // Enemy
                    index = enemySpawnerList.IndexOf(spawnerScript);
                    Update_Text_Value(enemyTextList[index], scoreType, valueString);
                    break;
            }
        }


        void Update_Text_Value(TextListProp tempTextListProp, int scoreType, string valueString)
        {
            switch (scoreType)
            {
                case 0: // Current durability.
                    tempTextListProp.hpText.text = valueString;
                    break;

                case 1: // Kills count.
                    tempTextListProp.killsText.text = valueString;
                    break;

                case 2: // Killed count.
                    tempTextListProp.killedText.text = valueString;
                    break;
            }
        }

    }

}
