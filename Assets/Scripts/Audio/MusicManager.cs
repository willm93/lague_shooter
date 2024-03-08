using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public NamedTheme[] themes;
    public Dictionary<string, AudioClip> themeLookup = new Dictionary<string, AudioClip>();
    public float fadeDuration = 1f;
    string currentTheme;
    string currentSceneName;
    
    void Start()
    {
        for(int i = 0; i < themes.Length; i++){
            themeLookup.Add(themes[i].themeName, themes[i].theme);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;

        currentSceneName = SceneManager.GetActiveScene().name;
        SetTheme(currentSceneName);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnemySpawner spawner = FindAnyObjectByType<EnemySpawner>();
        if (spawner != null){
            spawner.OnNewWave += OnThemeChange;
        }

        if (currentSceneName != scene.name || currentTheme != scene.name){
            currentSceneName = scene.name;
            currentTheme = currentSceneName;
            SetTheme(currentTheme);
        }
    }

    void SetTheme(string key)
    {
        currentTheme = key;
        Debug.Log("Current Theme: " + currentTheme);
        AudioManager.instance.PlayMusic(themeLookup[key], fadeDuration);
    }
    
    public void OnThemeChange(int waveNumber)
    {
        if (waveNumber == 4){
            SetTheme("final_wave");
        }
    }

    [System.Serializable]
    public class NamedTheme {
        public string themeName;
        public AudioClip theme;
    }
}
