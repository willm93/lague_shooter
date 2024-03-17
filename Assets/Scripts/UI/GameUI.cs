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

    [SerializeField] RectTransform newWaveBanner;
    Vector2 originalBannerPosition;
    [SerializeField] RectTransform bannerTargetPosition;
    [SerializeField] TextMeshProUGUI newWaveTitle;
    [SerializeField] float bannerPauseTime = 1.5f;
    IEnumerator currentCoroutine;

    [SerializeField] TextMeshProUGUI scoreUI;
    [SerializeField] RectTransform hpBar;
    float healthPercent = 1;
    float currentVelocity;
    float hpTransitionTime = 0.1f;

    [SerializeField] RectTransform stamBar;
    float stamPercent = 1;

    [SerializeField] TextMeshProUGUI ammoUI;
    [SerializeField] GameObject powerUpUI;
    [SerializeField] GameObject lifeOnKillIcon;
    [SerializeField] GameObject infiniteStaminaIcon;
    [SerializeField] GameObject infiniteAmmoIcon;
    [SerializeField] TextMeshProUGUI lifeOnKillText;
    [SerializeField] TextMeshProUGUI infiniteStaminaText;
    [SerializeField] TextMeshProUGUI infiniteAmmoText;

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

            UpdatePowerUpDuration();
            ShowPowerups();
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
        powerUpDurations[variety] += duration;

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

    void UpdatePowerUpDuration()
    {
        //reverse iteration to allow adding powerups from event
        for(int i = currentPowerups.Count - 1; i >= 0; i--)
        {
            powerUpDurations[currentPowerups[i]] -= Time.deltaTime;

            if (powerUpDurations[currentPowerups[i]] <= 0)
            {
                powerUpDurations[currentPowerups[i]] = 0;
                currentPowerups.Remove(currentPowerups[i]);
            }
                
        }
    }

    void ShowPowerups()
    {
        lifeOnKillIcon.SetActive(currentPowerups.Contains(Powerup.Variety.LifeOnKill));
        lifeOnKillText.text = powerUpDurations[Powerup.Variety.LifeOnKill].ToString();

        infiniteStaminaIcon.SetActive(currentPowerups.Contains(Powerup.Variety.InfiniteStamina));
        infiniteStaminaText.text = powerUpDurations[Powerup.Variety.InfiniteStamina].ToString();
        
        infiniteAmmoIcon.SetActive(currentPowerups.Contains(Powerup.Variety.InfiniteAmmo));
        infiniteAmmoText.text = powerUpDurations[Powerup.Variety.InfiniteAmmo].ToString();
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

