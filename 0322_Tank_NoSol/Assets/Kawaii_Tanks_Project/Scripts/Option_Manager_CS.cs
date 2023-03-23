using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Audio;

namespace ChobiAssets.KTP
{

    public class Option_Manager_CS : MonoBehaviour
    {
        /*
         * This script is attached to "Volume_Settings" object under the "Canvas_Pause" in the scene.
         * This script controls volume values of the "Audio Mixer" with the sliders.
        */

        public AudioMixer audioMixer;
        public Slider masterVolumeSlider;
        public Slider seVolumeSlider;
        public Slider bgmVolumeSlider;


        public float Master_Volume
        { // Called from the slider for master volume.
            set
            {
                if (value == masterVolumeSlider.minValue)
                {
                    value = -80.0f;
                }
                audioMixer.SetFloat("Master_Volume", value);
            }
            get
            {
                float value;
                audioMixer.GetFloat("Master_Volume", out value);
                return value;
            }
        }


        public float SE_Volume
        { // Called from the slider for SE volume.
            set
            {
                if (value == seVolumeSlider.minValue)
                {
                    value = -80.0f;
                }
                audioMixer.SetFloat("SE_Volume", value);
            }
            get
            {
                float value;
                audioMixer.GetFloat("SE_Volume", out value);
                return value;
            }
        }


        public float BGM_Volume
        { // Called from the slider for BGM volume.
            set
            {
                if (value == bgmVolumeSlider.minValue)
                {
                    value = -80.0f;
                }
                audioMixer.SetFloat("BGM_Volume", value);
            }
            get
            {
                float value;
                audioMixer.GetFloat("BGM_Volume", out value);
                return value;
            }
        }


        void Start()
        {
            // Set the sliders.
            if (masterVolumeSlider)
            {
                masterVolumeSlider.value = Master_Volume;
            }
            if (seVolumeSlider)
            {
                seVolumeSlider.value = SE_Volume;
            }
            if (bgmVolumeSlider)
            {
                bgmVolumeSlider.value = BGM_Volume;
            }
        }

    }

}
