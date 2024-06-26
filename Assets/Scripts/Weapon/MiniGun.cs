using System.Collections;
using UnityEngine;

public class MiniGun : Gun
{
    enum WindingState {WindingUp, Spinning, Firing, Idle}
    WindingState currentWindingState;

    [Header("Wind Up Fire Effects")]
    [SerializeField] protected Transform barrelRotary;
    [SerializeField] protected float rotationSpeed = 666f, windUpSpeed = 1f, windDownSpeed = 0.5f;
    [SerializeField] protected AudioClip windUpSound, spinningSound, fireContinuousSound;
    float spinningStartTime;
    float windUpPercent = 0f;

    IEnumerator currentFiringRoutine;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        //wind down
        if (!triggerHeld && windUpPercent > 0){
            windUpPercent -= Time.deltaTime * windDownSpeed;
        }
        HandleEffects();
    }

    public override void HoldTrigger()
    {
        base.HoldTrigger();
        if (currentFiringRoutine == null && !isReloading)
        {
            currentFiringRoutine = WindUpFire();
            StartCoroutine(currentFiringRoutine);
        }
        if (bulletsRemaining == 0 && !isReloading)
        {
            AudioManager.instance.PlaySound(emptyClick);
        }
    }

    public override bool CanReload()
    {
        return !triggerHeld && !isReloading;
    }

    public override void Reload()
    {
        base.Reload();
        StartCoroutine(ReloadRoutine());
    }

    IEnumerator WindUpFire()
    {
        while (triggerHeld && bulletsRemaining > 0)
        {
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

    void HandleEffects()
    {
        //sounds bad if the player spams left click right before the gun is about to fire, but works ok if they hold it as intended
        if (isReloading)
        {
            if(currentWindingState != WindingState.Idle){
                AudioManager.instance.FadeOutContinuousSound(0.1f);
                AudioManager.instance.PlaySound(reloadSound);
            }
            currentWindingState = WindingState.Idle;
            return;
        }

        if (bulletsRemaining == 0)
        {
            if (currentWindingState != WindingState.Idle){
                currentWindingState = WindingState.Idle;
                AudioManager.instance.FadeOutContinuousSound(0.1f);
                AudioManager.instance.PlaySound(emptyClick);
            }
            ReleaseTrigger();
            return;
        }

        if (!triggerHeld && windUpPercent <= 0 && currentWindingState != WindingState.Idle)
        {
            currentWindingState = WindingState.Idle;
            AudioManager.instance.FadeOutContinuousSound(0.8f);
        } 

        if (windUpPercent > 0 && currentWindingState == WindingState.Idle)
        {
            currentWindingState = WindingState.WindingUp;
            spinningStartTime = Time.time + windUpSound.length - 0.02f;
            AudioManager.instance.PlaySound(windUpSound);
        }

        if (windUpPercent > 0 && windUpPercent < 1 && currentWindingState != WindingState.Spinning && Time.time > spinningStartTime)
        {
            currentWindingState = WindingState.Spinning;
            AudioManager.instance.PlayContinuousSound(spinningSound);
        }

        if (triggerHeld && windUpPercent >= 1 && currentWindingState != WindingState.Firing)
        {
            currentWindingState = WindingState.Firing;
            AudioManager.instance.PlayContinuousSound(fireContinuousSound);
        }

        if (currentWindingState != WindingState.Idle)
        {
            barrelRotary.Rotate(Vector3.forward * Time.deltaTime * rotationSpeed);
        }
    }

    void OnDestroy()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.FadeOutContinuousSound(0.1f);
        }
    }
}
