using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioClip[] themes;
    public float fadeDuration = 0.75f;
    int currentThemeIndex;

    
    void Start()
    {
        EnemySpawner spawner = FindAnyObjectByType<EnemySpawner>();
        if (spawner != null){
            spawner.OnNewWave += OnThemeChange;
        }

        currentThemeIndex = 0;
        AudioManager.instance.PlayMusic(themes[currentThemeIndex]);
    }

    void SetTheme(int index)
    {
        currentThemeIndex = index;
        AudioManager.instance.PlayMusic(themes[currentThemeIndex], 0.5f);
    }
    
    public void OnThemeChange(int waveNumber)
    {
        if (waveNumber == 4){
            SetTheme(1);
        }
    }
}
