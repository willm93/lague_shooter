using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (MuzzleFlash))]
public abstract class Gun : MonoBehaviour
{
    public string nameOfGun;
    public Transform[] projectileSpawns;
    public Transform ejector;
    public Projectile projectilePrefab;
    public Shell shellPrefab;
    protected MuzzleFlash muzzleFlash;
    
    public AudioClip fireSound;
    public AudioClip reloadSound;
    public AudioClip emptyClick;

    public float rpm = 400f;
    protected float secondsBetweenShots;
    public float muzzleVelocity = 35f;
    public int magSize = 1;
    protected int bulletsRemaining;
    public float reloadTime;
    public float ReloadTime {get {return reloadTime;}}
    protected bool isReloading;
    protected bool triggerHeld;

    [Header("Recoil")]
    public float verticalRecoil = 0.2f;
    public float horizontalRecoil = 2f;
    protected Vector3 vertRecoilRecoverVelocity; 
    public float vertRecoilRecoverTime = 0.1f;
    protected float horzRecoilRecoverVelocity; 
    public float horzRecoilRecoverTime = 0.1f;

    protected Projectile newProjectile;
    protected Shell newShell;

    protected virtual void Start()
    {
        muzzleFlash = this.GetComponent<MuzzleFlash>();

        //msBetweenShots = 60000 / rpm;
        secondsBetweenShots = 60f / rpm;
        bulletsRemaining = magSize;
    }

    protected virtual void Update()
    {
        RecoilRecovery();
    }

    public virtual void HoldTrigger()
    {
        triggerHeld = true;
    }
    public virtual void ReleaseTrigger()
    {
        triggerHeld = false;
    }

    public virtual void Reload()
    {
        isReloading = true;
    }

    public abstract bool CanReload();

    public string GetNameOfGun()
    {
        return nameOfGun;
    }

    public int GetBulletsRemaining()
    {
        return bulletsRemaining;
    }

    public int GetMagSize()
    {
        return magSize;
    }

    void RecoilRecovery()
    {
        this.transform.localPosition = Vector3.SmoothDamp(this.transform.localPosition, Vector3.zero, ref vertRecoilRecoverVelocity, vertRecoilRecoverTime);
        this.transform.localEulerAngles = Vector3.up * Mathf.SmoothDampAngle(this.transform.localEulerAngles.y, 0, ref horzRecoilRecoverVelocity, horzRecoilRecoverTime);
    }

    protected virtual void FireProjectile()
    {
        //spawn projectile(s)
        foreach(Transform pSpawn in projectileSpawns){
            newProjectile = Instantiate<Projectile>(projectilePrefab, pSpawn.position, pSpawn.rotation);
            newProjectile.SetSpeed(muzzleVelocity);
        }
        bulletsRemaining--;

        //projectile effects
        newShell = Instantiate<Shell>(shellPrefab, ejector.position, Quaternion.Euler(shellPrefab.transform.eulerAngles + ejector.eulerAngles));
        newShell.initDirection = this.transform.right;

        muzzleFlash.Activate();

        //recoil
        this.transform.localPosition -= Vector3.forward * verticalRecoil;
        this.transform.localEulerAngles += Vector3.up * UnityEngine.Random.Range(-horizontalRecoil, horizontalRecoil);
    }

    protected IEnumerator ReloadRoutine()
    {
        AudioManager.instance.PlaySound(reloadSound);

        float reloadPercent = 0;
        float interpolation;
        while (reloadPercent <= 1){    
            //animate
            reloadPercent += Time.deltaTime * (1 / reloadTime);
            interpolation = (-Mathf.Pow(reloadPercent, 2) + reloadPercent) * 4; //quadratic function where f(x) goes from 0 > 1 > 0 as x goes from 0 > 1
            this.transform.localEulerAngles = Vector3.right * Mathf.Lerp(0, -50, interpolation);

            yield return null;
        }

        bulletsRemaining = magSize;
        isReloading = false;
    }
}
