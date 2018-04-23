using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager> {
    [SerializeField] protected AudioSource m_bgMusic;
    [SerializeField] protected AudioSource m_panicMusic;

    public void StartPanicMusic()
    {
        m_panicMusic.Play();
        m_bgMusic.Stop();
    }
}
