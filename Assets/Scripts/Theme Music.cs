using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof ( AudioSource))]
public class ThemeMusic : MonoBehaviour
{
    AudioSource audioSource;
    public AudioClip altTheme;
    float initialVolume;
    float fadingVolume;
    float fadeSpeed = 0.5f;

    
    void Start()
    {
        audioSource = this.GetComponent<AudioSource>();
        initialVolume = audioSource.volume;
        FindAnyObjectByType<EnemySpawner>().OnNewWave += OnThemeChange;
    }

    IEnumerator ChangeTheme()
    {
        while (fadingVolume > 0.05f){
            fadingVolume -= Time.deltaTime * fadeSpeed;
            audioSource.volume = fadingVolume;
            yield return null;
        } 
            
        audioSource.Stop();
        audioSource.clip = altTheme;
        fadingVolume = initialVolume;
        audioSource.volume = initialVolume;
        audioSource.Play();
    }
    
    public void OnThemeChange(int waveNumber)
    {
        if (waveNumber == 4){
            StartCoroutine(ChangeTheme());
        }
    }
}
