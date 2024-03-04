using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGun : Gun
{
    enum WindingState {WindingUp, Spinning, Firing, WindingDown, Idle}
    WindingState currentWindingState;

    [Header("Wind Up Fire Effects")]
    public Transform barrelRotary;
    public float rotationSpeed = 1f;
    public AudioClip windUpSound;
    public AudioClip spinningSound;
    public AudioClip fireContinuousSound;
    float spinningStartTime;
    float initialVolume;
    float fadingVolume;
    float fadeSpeed = 1.5f;

    public float windUpSpeed = 1f;
    public float windDownSpeed = 0.5f;
    float windUpPercent = 0f;

    IEnumerator currentFiringRoutine;

    protected override void Start()
    {
        base.Start();
        initialVolume = audioSource.volume;
    }

    protected override void Update()
    {
        base.Update();
        //wind down
        if (!triggerHeld && windUpPercent > 0){
            windUpPercent -= Time.deltaTime * windDownSpeed;
        }
        HandleWindUpFireEffects();
    }

    public override void HoldTrigger()
    {
        base.HoldTrigger();
        if (currentFiringRoutine == null && !isReloading){
            currentFiringRoutine = WindUpFire();
            StartCoroutine(currentFiringRoutine);
        }
        if (bulletsRemaining == 0 && !isReloading){
            audioSource.PlayOneShot(emptyClick);
        }
    }

    public override void Reload()
    {
        if (!triggerHeld && !isReloading){
            base.Reload();
            StartCoroutine(ReloadRoutine());
        }
    }

    IEnumerator WindUpFire()
    {
        while (triggerHeld && bulletsRemaining > 0){
            
            //windup
            while (windUpPercent < 1 && triggerHeld){
                windUpPercent += Time.deltaTime * windUpSpeed;
                yield return null;
            }
            
            //fire
            if (windUpPercent >= 1){
                FireProjectile();
            }
            yield return new WaitForSeconds(secondsBetweenShots);
        }
        currentFiringRoutine = null;
    }

    void HandleWindUpFireEffects()
    {
        //sounds bad if the player spams left click right before the gun is about to fire, but works ok if they hold it as intended
        if (isReloading){
            if(currentWindingState != WindingState.Idle){
                audioSource.Stop();
                audioSource.PlayOneShot(reloadSound);
            }
            currentWindingState = WindingState.Idle;
            return;
        }
        if (!triggerHeld && windUpPercent <= 0 && currentWindingState != WindingState.Idle){
            if (fadingVolume > 0.05f){
                currentWindingState = WindingState.WindingDown;
                fadingVolume -= Time.deltaTime * fadeSpeed;
                audioSource.volume = fadingVolume;
            } else {
                currentWindingState = WindingState.Idle;
                audioSource.Stop();
                fadingVolume = initialVolume;
                audioSource.volume = initialVolume;
            }
        } 
        if (windUpPercent > 0 && currentWindingState == WindingState.Idle){
            currentWindingState = WindingState.WindingUp;
            spinningStartTime = Time.time + windUpSound.length - 0.01f;
            audioSource.PlayOneShot(windUpSound); 
        }
        if (windUpPercent > 0 && windUpPercent < 1 && currentWindingState != WindingState.Spinning && Time.time > spinningStartTime){
            currentWindingState = WindingState.Spinning;
            audioSource.Stop();
            audioSource.clip = spinningSound;
            fadingVolume = initialVolume;
            audioSource.volume = initialVolume;
            audioSource.Play();
            audioSource.loop = true;
        }
        if (triggerHeld && windUpPercent >= 1 && currentWindingState != WindingState.Firing){
            currentWindingState = WindingState.Firing;
            audioSource.Stop();
            audioSource.clip = fireContinuousSound;
            audioSource.Play();
            audioSource.loop = true;
        }
        if (currentWindingState != WindingState.Idle){
            barrelRotary.Rotate(Vector3.forward * Time.deltaTime * rotationSpeed);
        }
    }
}
