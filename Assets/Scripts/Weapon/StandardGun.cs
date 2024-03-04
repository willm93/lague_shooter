using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandardGun : Gun
{
    public enum FireMode {Auto, Burst, Single}
    public FireMode fireMode;
    public AudioClip fireSound;
    public int burstCount;
    
    IEnumerator currentFiringRoutine;
    public override void HoldTrigger()
    {
        //DebugInfo("Trigger Hold Start");
        base.HoldTrigger();
        if (currentFiringRoutine == null && !isReloading){
            switch (fireMode)
            {
                case FireMode.Auto:{
                    currentFiringRoutine = AutomaticFire();
                    break;
                }
                case FireMode.Burst:{
                    currentFiringRoutine = BurstFire();
                    break;
                }
                case FireMode.Single:{
                    currentFiringRoutine = SingleFire();
                    break;
                }
            }
            StartCoroutine(currentFiringRoutine);
        }
        if (bulletsRemaining == 0 && !isReloading){
            audioSource.PlayOneShot(emptyClick);
        }
        //DebugInfo("Trigger Hold End");
    }

    public override void ReleaseTrigger()
    {
        base.ReleaseTrigger();
    }

    protected override void FireProjectile()
    {
        base.FireProjectile();
        audioSource.PlayOneShot(fireSound);
    }

    public override void Reload()
    {
        if (!isReloading) {
            base.ReleaseTrigger();
            base.Reload();
            StartCoroutine(ReloadRoutine());
        }
    }

    IEnumerator AutomaticFire()
    {
        while (triggerHeld && bulletsRemaining > 0){
            FireProjectile();
            yield return new WaitForSeconds(secondsBetweenShots);
        }
        currentFiringRoutine = null;
    }

    IEnumerator BurstFire()
    {
        for(int i = 0; i < burstCount; i++){
            if (bulletsRemaining < 0){
                break;
            }

            FireProjectile();
            yield return new WaitForSeconds(secondsBetweenShots);
        }
        currentFiringRoutine = null;
    }

    IEnumerator SingleFire()
    {
        if (bulletsRemaining > 0){
            FireProjectile();
            yield return new WaitForSeconds(secondsBetweenShots);
        }
        currentFiringRoutine = null;
    }

    
    public void DebugInfo(String caller)
    {
        Debug.Log("                                                Caller: " + caller);
        Debug.Log("                                                Gun: " + nameOfGun);
        Debug.Log("                                                                        Current Firing Routine: " + currentFiringRoutine);
        Debug.Log("                                                                        Trigger Held: " + triggerHeld);
        //Debug.Log("                                                                        Reloading: " + isReloading);
        //Debug.Log("                                                                        Bullets Remaining: " + bulletsRemaining);
    }
}
