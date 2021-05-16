using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG
{
    public class OptionsPreferencesManager
    {
        private const string volumeKey = "Volume";
        public const int defaultVolume = -10;

        public static float GetVolume()
        {
            return PlayerPrefs.GetFloat(volumeKey, defaultVolume);
        }

        public static void SetVolume(float volume)
        {
            PlayerPrefs.SetFloat(volumeKey, volume);
        }

    }
}
