using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviourSingleton<AudioManager>
{
    [System.Serializable]
    class BGMSound
    {

    }

    [System.Serializable]
    class FSXSound
    {
        [System.Serializable]
        public class Clips
        {
            public string name;
            public AudioClip clip;
        }
        public Clips clips;
    }

    void Reset()
    {
            
    }

    void Start()
    {
    }

    void Update()
    {
    }
}
