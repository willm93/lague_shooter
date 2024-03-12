using System.Collections;
using UnityEngine;

public class Revolver : Gun
{
    [SerializeField] Transform chamberPivot;
    float maxChamberRotation = 50f;

    IEnumerator currentFiringRoutine;
    
    public override void HoldTrigger()
    {
        base.HoldTrigger();
        if (currentFiringRoutine == null && !isReloading){
            currentFiringRoutine = Fire();
            StartCoroutine(currentFiringRoutine);
        }
        if (bulletsRemaining == 0 && !isReloading){
            AudioManager.instance.PlaySound(emptyClick);
        }
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

    IEnumerator Fire()
    {
        if (bulletsRemaining > 0){
            FireProjectile();
            yield return new WaitForSeconds(secondsBetweenShots);
        }
        currentFiringRoutine = null;
    }

    protected override void FireProjectile()
    {
        //spawn projectile(s)
        foreach(Transform pSpawn in projectileSpawns){
            newProjectile = Instantiate<Projectile>(projectilePrefab, pSpawn.position, pSpawn.rotation);
            newProjectile.SetSpeed(muzzleVelocity);
        }

        if(!infiniteAmmo){
            bulletsRemaining--;
        }
        muzzleFlash.Activate();

        //recoil
        transform.localPosition -= Vector3.forward * verticalRecoil;
        transform.localEulerAngles += Vector3.up * Random.Range(-horizontalRecoil, horizontalRecoil);
        AudioManager.instance.PlaySound(fireSound);
    }

    protected override IEnumerator ReloadRoutine()
    {
        AudioManager.instance.PlaySound(reloadSound);

        float reloadPercent = 0;
        float interpolation;
        bool spawnedShells = false;

        while (reloadPercent <= 1){    
            //animate
            reloadPercent += Time.deltaTime * (1 / reloadTime);
            interpolation = (-Mathf.Pow(reloadPercent, 2) + reloadPercent) * 4; //quadratic function where f(x) goes from 0 > 1 > 0 as x goes from 0 > 1

            transform.localEulerAngles = Vector3.right * Mathf.Lerp(0, -50, interpolation);
            chamberPivot.localEulerAngles = Vector3.forward * Mathf.Lerp(0, -maxChamberRotation, interpolation);

            if (!spawnedShells && reloadPercent >= 0.5f){
                for(int i = 0; i < magSize - bulletsRemaining; i++){
                    newShell = Instantiate<Shell>(shellPrefab, ejector.position, Quaternion.Euler(shellPrefab.transform.eulerAngles + ejector.eulerAngles));
                    newShell.initDirection = -transform.forward;
                }   
                spawnedShells = true;
            }

            yield return null;
        }

        bulletsRemaining = magSize;
        isReloading = false;
    }
}
