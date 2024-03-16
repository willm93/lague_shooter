using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    Player player;
    GunController gunController;
    PowerupController powerupController;

    public RectTransform newWaveBanner;
    Vector2 originalBannerPosition;
    public RectTransform bannerTargetPosition;
    public TextMeshProUGUI newWaveTitle;
    public float bannerPauseTime = 1.5f;
    IEnumerator currentCoroutine;

    public TextMeshProUGUI scoreUI;
    public RectTransform hpBar;
    float healthPercent = 1;
    float currentVelocity;
    float hpTransitionTime = 0.1f;

    public RectTransform stamBar;
    float stamPercent = 1;

    public TextMeshProUGUI ammoUI;
    public GameObject powerUpUI;
    TextMeshProUGUI powerUpTitle;

    List<Powerup.Variety> currentPowerups = new List<Powerup.Variety>();
    Dictionary<Powerup.Variety, float> powerUpDurations = new Dictionary<Powerup.Variety, float>();

    void Start()
    {
        player = FindObjectOfType<Player>();
        gunController = player.GetComponent<GunController>();
        powerupController = player.GetComponent<PowerupController>();
        
        foreach(Powerup.Variety variety in Enum.GetValues(typeof(Powerup.Variety)))
        {
            powerUpDurations.Add(variety, 0);
        }
        powerUpTitle = powerUpUI.transform.Find("Title").GetComponent<TextMeshProUGUI>();

        player.OnDeath += OnGameOver;
        powerupController.OnPowerup += OnPowerup;

        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
        if (spawner != null){
            spawner.OnNewWave += OnNewWave;
        }
        
        originalBannerPosition = newWaveBanner.anchoredPosition;
    }

    void Update()
    {
        scoreUI.text = "Kill Count: " + ScoreKeeper.score;

        if (player){
            healthPercent = Mathf.SmoothDamp(healthPercent,  player.currentHealth / (float) player.MaxHealth, ref currentVelocity, hpTransitionTime);
            stamPercent = player.stamina / player.maxStamina;
            ammoUI.text = $"{gunController.equippedGun.NameOfGun} \n{gunController.equippedGun.DisplayAmmo()}";

            UpdatePowerUpDurations();
            powerUpTitle.text = PowerUpsToText(currentPowerups);
        }

        hpBar.localScale = new Vector3(healthPercent, 1, 1);
        stamBar.localScale = new Vector3(stamPercent, 1, 1);
    }

    void OnNewWave(int waveNumber)
    {
        newWaveTitle.SetText("Wave " + waveNumber);
        
        if (currentCoroutine != null){
            StopCoroutine(currentCoroutine);
            newWaveBanner.anchoredPosition = originalBannerPosition;    
        }
        currentCoroutine = AnimateNewWaveBanner();
        StartCoroutine(currentCoroutine);
    }

    void OnPowerup(Powerup.Variety variety, float duration)
    {
        powerUpDurations[variety] = duration;

        if (!currentPowerups.Contains(variety))
            currentPowerups.Add(variety);
    }

    void OnGameOver()
    {
        gameObject.SetActive(false);
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

    void UpdatePowerUpDurations()
    {
        //reverse iteration to allow adding powerups from event
        for(int i = currentPowerups.Count - 1; i >= 0; i--)
        {
            powerUpDurations[currentPowerups[i]] -= Time.deltaTime;

            if (powerUpDurations[currentPowerups[i]] <= 0)
                currentPowerups.Remove(currentPowerups[i]);
        }
    }

    string PowerUpsToText(List<Powerup.Variety> powerups)
    {
        string text = "";
        foreach(Powerup.Variety p in powerups)
        {
            text += "\n" + p.ToString();
        }
        
        return text;
    }
}

