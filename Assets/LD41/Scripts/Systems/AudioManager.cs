using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager> {
    [SerializeField] protected AudioSource m_bgMusic;
    [SerializeField] protected AudioSource m_panicMusic;
    [SerializeField] protected AudioSource m_titleMusic;

    
    public void StartStageMusic()
    {
        m_bgMusic.Play();
        m_titleMusic.Stop();
        m_panicMusic.Stop();
    }

    public void StartPanicMusic()
    {
        
        m_bgMusic.Stop();
        m_titleMusic.Stop();
        m_panicMusic.Play();
    }

    public void StartTitleMusic()
    {
        m_bgMusic.Stop();
        m_titleMusic.Play();
        m_panicMusic.Stop();
    }
}
