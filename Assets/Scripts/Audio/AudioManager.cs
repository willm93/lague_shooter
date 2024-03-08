using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent( typeof(SoundLibrary))]
public class AudioManager : MonoBehaviour
{
    public enum AudioChannel {Master, Sfx, Music}
    public static AudioManager instance {get; private set;}
    
    public float masterVolume {get; private set;}
    public float sfxVolume {get; private set;}
    public float musicVolume {get; private set;}

    SoundLibrary soundLibrary;
    Transform audioListener;

    AudioSource[] musicSources;
    int activeMusicSourceIndex = 0;

    AudioSource sfxSource;
    AudioSource contSfxSource;

    Player player;

    void Awake()
    {
        if (instance != null){
            Destroy(this.gameObject);
        } else {
            instance = this;
            DontDestroyOnLoad(this.gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;

            soundLibrary = this.GetComponent<SoundLibrary>();
            audioListener = this.transform.Find("Audio Listener").gameObject.transform;
            audioListener.position = Vector3.zero;

            masterVolume = PlayerPrefs.GetFloat("master_volume", 1);
            sfxVolume = PlayerPrefs.GetFloat("sfx_volume", 1);
            musicVolume = PlayerPrefs.GetFloat("music_volume", 1);

            musicSources = new AudioSource[2];
            for (int i = 0; i < musicSources.Length; i++){
                GameObject newMusicSource = new GameObject("Music Source " + (i + 1));
                musicSources[i] = newMusicSource.AddComponent<AudioSource>();
                musicSources[i].volume = musicVolume * masterVolume;
                musicSources[i].ignoreListenerPause = true;
                newMusicSource.transform.parent = this.transform;
            }

            GameObject newSfxSource = new GameObject("Sfx Sound Source ");
            sfxSource = newSfxSource.AddComponent<AudioSource>();
            sfxSource.volume = sfxVolume * masterVolume;
            sfxSource.transform.position = Vector3.zero;
            newSfxSource.transform.parent = this.transform;

            GameObject newContSfxSource = new GameObject("Continuous Sfx Sound Source ");
            contSfxSource = newContSfxSource.AddComponent<AudioSource>();
            contSfxSource.volume = sfxVolume * masterVolume;
            contSfxSource.transform.position = Vector3.zero;
            contSfxSource.transform.parent = this.transform;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        player = FindAnyObjectByType<Player>();
        if (player != null){
            player.OnPause += OnPause;
            player.OnResume += OnResume;
        }
    }

    void OnPause()
    {
        AudioListener.pause = true;
    }

    void OnResume()
    {
        AudioListener.pause = false;
    }

    public void SetVolume(float volume, AudioChannel channel)
    {
        volume = Mathf.Clamp01(volume);
        switch (channel){
            case AudioChannel.Master:
                masterVolume = volume;
                break;
            case AudioChannel.Sfx:
                sfxVolume = volume;
                break;
            case AudioChannel.Music:
                musicVolume = volume;
                break;
        }

        musicSources[0].volume = musicVolume * masterVolume;
        musicSources[1].volume = musicVolume * masterVolume;
        sfxSource.volume = sfxVolume * masterVolume;
        contSfxSource.volume = sfxVolume * masterVolume;

        PlayerPrefs.SetFloat("master_volume", masterVolume);
        PlayerPrefs.SetFloat("sfx_volume", sfxVolume);
        PlayerPrefs.SetFloat("music_volume", musicVolume);
        PlayerPrefs.Save();
    }

    public void PlayMusic(AudioClip clip, float fadeDuration = 1f)
    {
        activeMusicSourceIndex = 1 - activeMusicSourceIndex;
        musicSources[activeMusicSourceIndex].clip = clip;
        musicSources[activeMusicSourceIndex].loop = true;
        musicSources[activeMusicSourceIndex].Play();

        StartCoroutine(MusicCrossFade(fadeDuration));
    }

    public void PlaySound(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void PlaySound(string name)
    {
        sfxSource.PlayOneShot(soundLibrary.GetClipByName(name));
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
            musicSources[activeMusicSourceIndex].volume = Mathf.Lerp(0, musicVolume * masterVolume, percent);
            musicSources[1 - activeMusicSourceIndex].volume = Mathf.Lerp(musicVolume * masterVolume, 0, percent);
            yield return null;
        }
        musicSources[1 - activeMusicSourceIndex].Stop();
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
