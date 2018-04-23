using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager> {
    [SerializeField] protected AudioSource m_bgMusic;

    [SerializeField] protected AudioClip m_panicMusic;
    [SerializeField] protected AudioClip m_stageMusic;

    [SerializeField] protected ObjectPool m_audioSourcePool;

    [SerializeField] protected AudioClip[] m_explosionClips;

    public void PlayPanicMusic()
    {
        PlayBGMusic(m_panicMusic);
    }

    public void PlayStageMusic()
    {
        PlayBGMusic(m_stageMusic);
    }

    public void PlayBGMusic(AudioClip music)
    {
        m_bgMusic.clip = music;
        m_bgMusic.Play();
    }

    public void PlayExplosionOneShot(float volume = 1.0f)
    {
        PlayOneShot(m_explosionClips, volume);
    }

    public void PlayOneShot(AudioClip[] clips, float volume = 1.0f)
    {
        PlayOneShot(clips[Random.Range(0, clips.Length)], volume);
    }

    public void PlayOneShot(AudioClip clip, float volume = 1.0f)
    {
        if(clip == null)
        {
            return;
        }

        m_audioSourcePool.GetAvailable<OneShotAudio>().Play(clip, volume);
    }
}
