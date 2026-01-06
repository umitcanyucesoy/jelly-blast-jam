using System;
using System.Linq;
using Project.Scripts.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityExtensions;
using Random = UnityEngine.Random;

public class AudioManager : MonoSingleton<AudioManager>
{

    [Serializable]
    public class AudioClass
    {
        public string name; 
        public AudioClip clip;
        public float volume = 0.2f;
        public bool pitched = false;
        [ShowIf("pitched")]public Vector2 pitch = Vector2.one;
    }
    public AudioClass[] sounds;
    public AudioSource regularSource;
    public AudioSource pitchedSource;
    
    public void PlayAudio(string value)
    {
        AudioClass sound = sounds.First(s=>s.name==value);
        regularSource.PlayOneShot(sound.clip, sound.volume);
    }
    
    public void PlayPitchedAudio(string value ,float pitch = 1)
    {
        AudioClass sound = sounds.First(s => s.name == value);
        pitchedSource.pitch = pitch;
        pitchedSource.PlayOneShot(sound.clip, sound.volume);
    }
    
    public void PlayPitchedAudioRandom(string value)
    {
        var count = StandingGrid.Instance.CurrentActiveShooterCount();
        var sound = sounds.First(s => s.name == value);
        var tempVolume = sound.volume * Mathf.Max(0f, 1f - (count - 1) / 10f);
        pitchedSource.pitch = Random.Range(sound.pitch.x, sound.pitch.y);
        pitchedSource.PlayOneShot(sound.clip, tempVolume);
    }
}