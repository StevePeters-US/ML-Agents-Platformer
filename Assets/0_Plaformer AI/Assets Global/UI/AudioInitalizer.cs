using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace APG
{
    public class AudioInitalizer : MonoBehaviour
    {
        public AudioMixer audioMixer;
        private const string musicVolumeParam = "MusicVolume";

        void Start()
        {
            float volume = OptionsPreferencesManager.GetVolume();
            Debug.Log("initializing Volue to: " + volume);
            InitializeVolume(volume);
        }

        public void SetDefaults()
        {

        }

        public void SetVolume(float volume)
        {
            float volumeInDecibels = ConvertToDecibel(volume);
            audioMixer.SetFloat(musicVolumeParam, volumeInDecibels);
            OptionsPreferencesManager.SetVolume(volumeInDecibels);
        }

        public void InitializeVolume(float volume)
        {
            SetVolume(ConvertFromDecibel(volume));
        }

        public float ConvertToDecibel(float value)
        {
            return Mathf.Log10(value) * 20;
        }

        public float ConvertFromDecibel(float value)
        {
            return Mathf.Pow(10, value / 20);
        }
    }
}
