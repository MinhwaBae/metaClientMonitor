using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace ChobiAssets.KTP
{

    public class KillCounter_CS : MonoBehaviour
    {
        /* 
		 * This script is attached to "Kill Counter" in the scene.
		 * This script controls the appearance of the Kill Counter.
         * This script works in combination with "Score_Manager_CS" in the "Game_Controller" in the scene.
		*/

        public Image friendBarImage;
        public Image enemyBarImage;
        public Text goalText;
        public Text friendText;
        public Text enemyText;
        [Tooltip("Duration of the changing effects.")] public float duration = 0.5f;
        [Tooltip("Color alpha of the bars.")] public float alpha = 0.5f;


        [HideInInspector] public static KillCounter_CS instance;


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
            // Minimize the bars.
            friendBarImage.fillAmount = 0.0f;
            enemyBarImage.fillAmount = 0.0f;
        }


        public void Update_KillCounter(bool isFriend, int goal, int countFriend, int countEnemy)
        { // Called from "Score_Manager_CS".

            // Update the texts.
            goalText.text = goal.ToString();
            friendText.text = countFriend.ToString();
            enemyText.text = countEnemy.ToString();

            // Control the bars.
            if (isFriend)
            { // Friend is killed.
                StartCoroutine(Bar_Effect(!isFriend, enemyBarImage, (float)countEnemy / goal));
            }
            else
            { // Enemy is killed.
                StartCoroutine(Bar_Effect(!isFriend, friendBarImage, (float)countFriend / goal));
            }
        }


        int friendEffectID;
        int enemyEffectID;
        IEnumerator Bar_Effect(bool isFriend, Image tempBar, float targetFillAmount)
        {
            int thisID;
            if (isFriend)
            {
                friendEffectID += 1;
                thisID = friendEffectID;
            }
            else
            {
                enemyEffectID += 1;
                thisID = enemyEffectID;
            }

            float count = 0.0f;
            Color currentColor = tempBar.color;
            while (count < duration)
            {
                if (isFriend)
                {
                    if (thisID < friendEffectID)
                    {
                        tempBar.fillAmount = targetFillAmount;
                        yield break;
                    }
                }
                else
                {
                    if (thisID < enemyEffectID)
                    {
                        tempBar.fillAmount = targetFillAmount;
                        yield break;
                    }
                }

                tempBar.fillAmount = Mathf.Lerp(1.0f, targetFillAmount, count / duration);
                currentColor.a = Mathf.Lerp(1.0f, alpha, count / duration);
                tempBar.color = currentColor;

                count += Time.deltaTime;
                yield return null;
            }

            tempBar.fillAmount = targetFillAmount;
            currentColor.a = alpha;
            tempBar.color = currentColor;

            if (isFriend)
            {
                friendEffectID = 0;
            }
            else
            {
                enemyEffectID = 0;
            }
        }

    }

}
