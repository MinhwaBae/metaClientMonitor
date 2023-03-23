using System.Collections;
using UnityEngine;
using UnityEngine.UI;

//MH
using System;
using UnityEngine.SceneManagement;
using System.Timers;

namespace ChobiAssets.KTP
{

    [RequireComponent(typeof(Text))]
    public class FPS_Counter_CS : MonoBehaviour
    {
        public Text thisText;

        const string textFormat = "{0} FPS";
        int ignoredFrames;
        float previousTime;
        float deltaTime = 0.0f;
        private static Timer interval;


        //MH
        public Scene scene;

       
        //MH
        private int targetFPS = 30;
        public bool isDetect = false;
        float logfps = 0.0f;
        float logmsec = 0.0f;


        void Start()
        {
            if (thisText == null)
            {
                thisText = GetComponent<Text>();
            }
            scene = SceneManager.GetActiveScene();
            // InvokeRepeating("LogFPS", 0.0f, 5.0f);
        }

       private void Awake()
        {
            StartCoroutine(printfps(12));
        }


        void Update()
        {
            //MH org src
            /*  ignoredFrames++;

              deltaTime = Time.realtimeSinceStartup - previousTime;
              if (deltaTime < 0.5f)
              {
                  return;
              }

              thisText.text = string.Format(textFormat, (int)(ignoredFrames / deltaTime));
              ignoredFrames = 0;
              previousTime = Time.realtimeSinceStartup;
            */


            float msec = 0.0f;
            float fps = 0.0f;

            deltaTime += (Time.deltaTime - deltaTime) * 0.1f;


            msec = deltaTime * 1000.0f;
           fps = 1.0f / deltaTime;

            //printfps()
            logfps = fps;
            logmsec = msec;

            thisText.text = string.Format("{0:0.} fps ({1:0.0} ms)", fps, msec);
        }

       /* void OnGUI()
        {
            float msec = deltaTime * 1000.0f;
            float fps = 1.0f / deltaTime;

            //printfps() 함수
            logfps = fps;
            logmsec = msec;

            thisText.text = string.Format("{0:0.} fps ({1:0.0} ms)", fps, msec);

              if (!isDetect)
               {

                   Debug.LogWarning("FPS " + targetFPS);
                   if (Time.deltaTime > 1.0f / targetFPS)
                   {
                       Debug.LogWarning("FPS dropped below target FPS of" + targetFPS);
                       isDetect = true;

                       try
                       {
                           throw new CustomException("Frame rate dropped");
                       }
                       catch (Exception e)
                       {

                       }
                   }

               }
        }*/

       

        IEnumerator printfps(int maxTime)
        {
            int time = 0;
            while (time < maxTime)
            {
                Debug.Log("time : " + time);
                Debug.Log("Scene name:" + scene.name + " " + logfps.ToString("N0") + "fps (" + logmsec.ToString("N1") + " msec )");
                yield return new WaitForSeconds(5f);

                time++;
            }
        }

    }

    public class CustomException : Exception
    {
        public CustomException(string message) : base(message)
        {
        }
    }
}
