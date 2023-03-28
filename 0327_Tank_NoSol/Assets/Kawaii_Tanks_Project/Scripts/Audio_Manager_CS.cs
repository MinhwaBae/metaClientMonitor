using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace ChobiAssets.KTP
{

    public class Audio_Manager_CS : MonoBehaviour
    {
        /*
		 * This script is attached to "Game_Controller" in the scene.
		 * The functions to play or stop the audio clips are called from "Score_Manager_CS" in the "Game_Controller" in the scene.
		*/

        public AudioClip mainBGMAudioClip;
        public AudioClip victoryAudioClip;
        public AudioClip defeatAudioClip;
        public AudioSource thisAudioSource;


        [HideInInspector] public static Audio_Manager_CS instance;


        void Awake()
        {
            instance = this;
        }


        void Start()
        {
            if (thisAudioSource == null)
            {
                thisAudioSource = GetComponent<AudioSource>();
            }
        }


        public IEnumerator Start_Music(float startingLag)
        { // Called from "Score_Manager_CS".
            yield return new WaitForSeconds(startingLag);
            Play_BGM(mainBGMAudioClip, true);
        }


        public IEnumerator Play_Victory(float startingLag)
        { // Called from "Score_Manager_CS".
            yield return new WaitForSeconds(startingLag);
            Play_BGM(victoryAudioClip, true);
        }


        public IEnumerator Play_Defeat(float startingLag)
        { // Called from "Score_Manager_CS".
            yield return new WaitForSeconds(startingLag);
            Play_BGM(defeatAudioClip, true);
        }


        void Play_BGM(AudioClip clip, bool isLooping)
        {
            if (clip == null)
            {
                return;
            }

            thisAudioSource.clip = clip;
            thisAudioSource.loop = isLooping;
            thisAudioSource.Play();
        }

    }
}
