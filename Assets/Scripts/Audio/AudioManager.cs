using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public float masterVolume = 1f;
    public float sfxVolume = 1f;
    public float musicVolume = 1f;

    Transform audioListener;
    
    AudioSource[] musicSources;
    int activeMusicSourceIndex = 0;

    AudioSource sfxSource;
    AudioSource contSfxSource;

    void Awake()
    {
        instance = this;
        sfxVolume *= masterVolume;
        musicVolume *= masterVolume;

        audioListener = this.transform.Find("Audio Listener").transform;
        audioListener.position = Vector3.zero;

        musicSources = new AudioSource[2];
        for (int i = 0; i < musicSources.Length; i++){
            GameObject newMusicSource = new GameObject("Music Source " + (i + 1));
            musicSources[i] = newMusicSource.AddComponent<AudioSource>();
            musicSources[i].volume = musicVolume;
            newMusicSource.transform.parent = this.transform;
        }

        GameObject newSfxSource = new GameObject("Sfx Sound Source ");
        sfxSource = newSfxSource.AddComponent<AudioSource>();
        sfxSource.volume = sfxVolume;
        sfxSource.transform.position = Vector3.zero;
        newSfxSource.transform.parent = this.transform;

        GameObject newContSfxSource = new GameObject("Continuous Sfx Sound Source ");
        contSfxSource = newContSfxSource.AddComponent<AudioSource>();
        contSfxSource.volume = sfxVolume;
        contSfxSource.transform.position = Vector3.zero;
        contSfxSource.transform.parent = this.transform;
    }

    public void PlayMusic(AudioClip clip, float fadeDuration = 1f)
    {
        activeMusicSourceIndex = 1 - activeMusicSourceIndex;
        musicSources[activeMusicSourceIndex].clip = clip;
        musicSources[activeMusicSourceIndex].Play();

        StartCoroutine(MusicCrossFade(fadeDuration));
    }

    public void PlaySound(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void PlayContinuousSound(AudioClip clip)
    {
        if (contSfxSource.isPlaying){
            contSfxSource.Stop();
        }
        contSfxSource.clip = clip;
        contSfxSource.loop = true;
        contSfxSource.Play();
    }

    public void FadeOutContinuousSound(float fadeDuration = 0f)
    {
        StartCoroutine(ContinuousSoundFade(fadeDuration));
    }

    IEnumerator MusicCrossFade(float fadeDuration){
        float percent = 0f;

        while(percent < 1){
            percent += Time.deltaTime * (1 / fadeDuration);
            musicSources[activeMusicSourceIndex].volume = Mathf.Lerp(0, musicVolume, percent);
            musicSources[1 - activeMusicSourceIndex].volume = Mathf.Lerp(musicVolume, 0, percent);
            yield return null;
        }
    }

    IEnumerator ContinuousSoundFade(float fadeDuration)
    {
        float percent = 0f;

        while(percent < 1){
            percent += Time.deltaTime * (1 / fadeDuration);
            contSfxSource.volume = Mathf.Lerp(sfxVolume, 0, percent);
            yield return null;
        }
        contSfxSource.Stop();
        contSfxSource.volume = sfxVolume;
    }
}
