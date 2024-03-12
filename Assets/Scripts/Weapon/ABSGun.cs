using System.Collections;
using UnityEngine;

public class ABSGun : Gun
{
    public enum FireMode {Auto, Burst, Single}
    public FireMode fireMode;
    [SerializeField] protected int burstCount;
    
    IEnumerator currentFiringRoutine;
    public override void HoldTrigger()
    {
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
            AudioManager.instance.PlaySound(emptyClick);
        }
    }

    protected override void FireProjectile()
    {
        base.FireProjectile();
        AudioManager.instance.PlaySound(fireSound);
    }

    public override bool CanReload()
    {
        return !isReloading;
    }

    public override void Reload()
    {
        base.ReleaseTrigger();
        base.Reload();
        StartCoroutine(ReloadRoutine());
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
            if (bulletsRemaining <= 0){
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

    
    public void DebugInfo(string caller)
    {
        Debug.Log("                                                Caller: " + caller);
        Debug.Log("                                                Gun: " + nameOfGun);
        Debug.Log("                                                                        Current Firing Routine: " + currentFiringRoutine);
        Debug.Log("                                                                        Trigger Held: " + triggerHeld);
        //Debug.Log("                                                                        Reloading: " + isReloading);
        //Debug.Log("                                                                        Bullets Remaining: " + bulletsRemaining);
    }
}
