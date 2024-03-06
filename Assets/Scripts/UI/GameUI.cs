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
    Player player;
    PlayerController playerController;
    GunController gunController;

    public RectTransform newWaveBanner;
    Vector2 originalBannerPosition;
    public RectTransform bannerTargetPosition;
    public TextMeshProUGUI newWaveTitle;
    public float bannerPauseTime = 1.5f;
    IEnumerator currentCoroutine;

    public TextMeshProUGUI scoreUI;
    public TextMeshProUGUI gameOverScoreUI;
    public RectTransform hpBar;
    float healthPercent = 1;
    float currentVelocity;
    float hpTransitionTime = 0.1f;

    public RectTransform stamBar;
    float stamPercent = 1;

    public TextMeshProUGUI ammoUI;

    void Start()
    {
        player = FindObjectOfType<Player>();
        playerController = player.GetComponent<PlayerController>();
        gunController = player.GetComponent<GunController>();
        player.OnDeath += OnGameOver;    

        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
        if (spawner != null){
            spawner.OnNewWave += OnNewWave;
        }
        
        originalBannerPosition = newWaveBanner.anchoredPosition;
    }

    void Update()
    {
        scoreUI.text = "Kill Count: " + ScoreKeeper.score;

        if (player != null){
            healthPercent = Mathf.SmoothDamp(healthPercent,  player.currentHealth / (float) player.maxHealth, ref currentVelocity, hpTransitionTime);
        }
        hpBar.localScale = new Vector3(healthPercent, 1, 1);

        if (playerController != null){
            stamPercent = playerController.stamina / playerController.maxStamina;
        }
        stamBar.localScale = new Vector3(stamPercent, 1, 1);

        if (gunController != null){
            ammoUI.text = gunController.equippedGun.nameOfGun + "\n" + gunController.equippedGun.GetBulletsRemaining() + " / " + gunController.equippedGun.GetMagSize();
        }

    }

    void OnGameOver()
    {
        Cursor.visible = true;
        StartCoroutine(Fade(Color.clear, new Color(0, 0, 0, 0.85f), 1));
        scoreUI.gameObject.SetActive(false);
        hpBar.transform.parent.gameObject.SetActive(false);

        gameOverScoreUI.text = scoreUI.text;
        gameOverUI.SetActive(true);
    }

    void OnNewWave(int waveNumber)
    {
        if (waveNumber == 4) {
            RenderSettings.skybox = altSkybox;
            GameObject.FindGameObjectWithTag("Directional Light").GetComponent<Light>().intensity = 2;
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
        float time = 0.35f;
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
        SceneManager.LoadScene("GameScene");
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}

