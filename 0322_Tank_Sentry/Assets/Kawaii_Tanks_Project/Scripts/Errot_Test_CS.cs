using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Diagnostics;

//MH
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;

public class Errot_Test_CS : MonoBehaviour
{
    /*
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    */



    private const int ARRAY_SIZE = 1000; //OutOfMemoryExceptionTestBtn

    private void Start()
    {
    }

    public void NullReferenceExceptionTestBtn()
    { // Called from the button.

        string myString = null;
        int stringLength = myString.Length;
        Debug.Log(stringLength);

    }

    private List<byte[]> memoryList = new List<byte[]>();

    public void OutOfMemoryExceptionTestBtn()
    {

        //byte[] byteArray = new byte[ARRAY_SIZE];
        //for (int i = 0; i < ARRAY_SIZE; i++)
        //{
        //    byteArray[i] = (byte)(i % 256);
        //}





        /*   try
           {
               // Outer block to handle any unexpected exceptions.
               try
               {
                   string s = "This";
                   s = s.Insert(2, "is ");

                   // Throw an OutOfMemoryException exception.
                   throw new OutOfMemoryException();
               }
               catch (ArgumentException)
               {
                   Console.WriteLine("ArgumentException in String.Insert");
               }

               // Execute program logic.
           }
           catch (OutOfMemoryException e)
           {
               Console.WriteLine("Terminating application unexpectedly...");
               Environment.FailFast(String.Format("Out of Memory: {0}",
                                                  e.Message));
           }*/

        string s = "This";
        s = s.Insert(2, "is ");

        // Throw an OutOfMemoryException exception.
        throw new OutOfMemoryException();
    }

    public void IOExceptionTestBtn()
    {
       
         {
             // 파일을 열어서 쓰기 권한이 없는 디렉토리에 저장하려고 시도합니다.
             using (FileStream fileStream = new FileStream("C:/Windows/System32/test.txt", FileMode.Create))
             {
                 using (StreamWriter writer = new StreamWriter(fileStream))
                 {
                     writer.WriteLine("Hello, world!");
                 }
             }
         }
        /* catch (IOException e)
         {
             Debug.LogError("IOException occurred: " + e.Message);
         }catch(Exception e)
         {
             Debug.LogError("extra Exception occurred: " + e.Message);
         }*/
        /*System.IO.StreamWriter sw = null;
        try
        {
            sw = new System.IO.StreamWriter(@"C:\test\test.txt");
            sw.WriteLine("Hello");
        }
        
        catch (System.IO.FileNotFoundException ex)
        {
            // Put the more specific exception first.
            System.Console.WriteLine(ex.ToString());
        }
        catch (Exception e)
        {
            Debug.Log("finally IOExceptionTestBtn exception"+e);
        }

        catch (System.IO.IOException ex)
        {
            // Put the less specific exception last.
            System.Console.WriteLine(ex.ToString());
        }
        finally
        {
            sw.Close();
        }
        
        System.Console.WriteLine("Done");
        */
    }

    public void ANRTestBtn()
    {
        Debug.Log("Running Thread.Sleep() on the UI thread to trigger an ANR event.");
        Thread.Sleep(6 * 1000); // ANR detection currently defaults to 5 seconds
        Debug.Log("Thread.Sleep() finished.");
    }


    public void ForceCrash() => Utils.ForceCrash(ForcedCrashCategory.AccessViolation);

    //public void ForceCrash()
    //{
    //    // if (Input.GetKeyDown(KeyCode.K))

    //    // 앱 강제 종료
    //    //Debug.Log("MH App kill.");
    //    //Application.Quit();

    //    int[] array = new int[1];
    //    array[2] = 0; // 배열 인덱스를 벗어난 접근을 하여 NullReferenceException이 발생하도록 함

    //}

    public void LogTestBtn()
    {
        //Debug.Log / LogError / LogWarning
        //* Format로그
        //Debug.Logformat / LogErrorFormat / LogWarningFormat
        //* UnityLogger 로그
        //Debug.UnityLogger 계열

        int year = 2023;
        int month = 4;
        string company_name = "LG Uplus";

        Debug.Log("Debug.Log Catched.");
        Debug.LogError("Debug.LogError Catched");
        Debug.LogWarning("Debug.LogWarning Catched");


        Debug.LogFormat("LogFormatTest : Year {0}, Month : {1}, Company : {2}", year, month, company_name);
        Debug.unityLogger.Log("Debug.unityLogger.Log cachted, Company", company_name);

        throw new CustomException("Exception for logTest ");

    }

    public class CustomException : Exception
    {
        public CustomException(string message) : base(message)
        {
            Debug.LogError(message);
        }
    }

}
