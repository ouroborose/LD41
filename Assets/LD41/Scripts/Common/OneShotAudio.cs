using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class OneShotAudio : MonoBehaviour {
    public AudioSource m_audioSource;
    protected void Awake()
    {
        if(m_audioSource == null)
        {
            m_audioSource = GetComponent<AudioSource>();
        }
    }

    protected void Update()
    {
        if(!m_audioSource.isPlaying)
        {
            gameObject.SetActive(false);
        }
    }

    public void Play(AudioClip clip, float volume = 1.0f)
    {
        gameObject.SetActive(true);
        m_audioSource.clip = clip;
        m_audioSource.volume = volume;
        m_audioSource.Play();
    }
}
