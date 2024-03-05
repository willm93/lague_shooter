using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public Image fadePlane;
    public GameObject gameOverUI;
    public Material altSkybox;

    public RectTransform newWaveBanner;
    Vector2 originalBannerPosition;
    public RectTransform bannerTargetPosition;
    public TextMeshProUGUI newWaveTitle;
    public float bannerPauseTime = 1.5f;
    IEnumerator currentCoroutine;
    void Start()
    {
        FindObjectOfType<Player>().OnDeath += OnGameOver;    
        FindObjectOfType<EnemySpawner>().OnNewWave += OnNewWave;
        originalBannerPosition = newWaveBanner.anchoredPosition;
    }

    void OnGameOver()
    {
        StartCoroutine(Fade(Color.clear, Color.black, 1));
        gameOverUI.SetActive(true);
    }

    void OnNewWave(int waveNumber)
    {
        if (waveNumber == 4) {
            RenderSettings.skybox = altSkybox;
        }

        newWaveTitle.SetText("Wave " + waveNumber);
        
        if (currentCoroutine != null){
            StopCoroutine(currentCoroutine);
            newWaveBanner.anchoredPosition = originalBannerPosition;    
        }
        currentCoroutine = AnimateNewWaveBanner();
        StartCoroutine(currentCoroutine);
    }

    IEnumerator AnimateNewWaveBanner()
    {
        float time = 1f;
        float percent = 0f;

        while(percent < 1){
            percent += Time.deltaTime * (1 / time);

            newWaveBanner.anchoredPosition = Vector2.Lerp(originalBannerPosition, bannerTargetPosition.anchoredPosition, percent);
            yield return null;
        }
        
        yield return new WaitForSeconds(bannerPauseTime);

        while(percent > 0){
            percent -= Time.deltaTime * (1 / time);

            newWaveBanner.anchoredPosition = Vector2.Lerp(originalBannerPosition, bannerTargetPosition.anchoredPosition, percent);
            yield return null;
        }
    }

    IEnumerator Fade(Color from, Color to, float time)
    {
        float speed = 1 / time;
        float percent = 0;

        while(percent < 1){
            percent += Time.deltaTime * speed;
            fadePlane.color = Color.Lerp(from, to, percent);
            yield return null;
        }

    }

    //UI Input
    public void StartNewGame()
    {
        SceneManager.LoadScene("MainScene");
    }
}
