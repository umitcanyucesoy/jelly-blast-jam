using System;
using UnityEngine;
using UnityExtensions;

namespace Project.Scripts.Managers
{
    public class Options : MonoSingleton<Options>
    {
        public GameObject[] vibrations;
        public GameObject[] sounds;
        private bool soundsOn;
        private bool vibrationsOn;

        private void Start()
        {
            soundsOn = Convert.ToBoolean(PlayerPrefs.GetInt("Sounds", 1));
            vibrationsOn = Convert.ToBoolean(PlayerPrefs.GetInt("Vibrations", 1));
            AudioListener.pause = !soundsOn;
            Taptic.tapticOn = vibrationsOn;
            ProcessOptions();
        }

        private void ProcessOptions()
        {
            vibrations[0].SetActive(vibrationsOn);
            vibrations[1].SetActive(!vibrationsOn);
            sounds[0].SetActive(soundsOn);
            sounds[1].SetActive(!soundsOn);
        }

        public void ChangeSounds()
        {
            Taptic.Light();
            soundsOn = !soundsOn;
            PlayerPrefs.SetInt("Sounds",Convert.ToInt32(soundsOn));
            AudioListener.pause = !soundsOn;
            ProcessOptions();
        }
    
        public void ChangeVibrations()
        {
            Taptic.Light();
            vibrationsOn = !vibrationsOn;
            Taptic.tapticOn = !Taptic.tapticOn;
            PlayerPrefs.SetInt("Vibrations",Convert.ToInt32(vibrationsOn));
            ProcessOptions();
        }
    }
}