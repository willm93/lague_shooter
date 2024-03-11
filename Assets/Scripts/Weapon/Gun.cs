using System;
using System.Collections;
using UnityEngine;

[RequireComponent (typeof (MuzzleFlash))]
public abstract class Gun : MonoBehaviour, IFirearm
{
    [SerializeField] 
    protected string nameOfGun; 
    public string NameOfGun {get => nameOfGun;}

    [SerializeField] protected Transform[] projectileSpawns;
    [SerializeField] protected Transform ejector;
    [SerializeField] protected Projectile projectilePrefab;
    [SerializeField] protected Shell shellPrefab;
    protected MuzzleFlash muzzleFlash;
    
    [SerializeField] protected AudioClip fireSound, reloadSound, emptyClick;

    [SerializeField] protected float rpm; 
    protected float secondsBetweenShots;

    [SerializeField] 
    protected float muzzleVelocity; 
    public float MuzzleVelocity {get => muzzleVelocity;}

    [SerializeField] 
    protected int magSize;
    protected int bulletsRemaining;

    [SerializeField] 
    protected float reloadTime; 
    public float ReloadTime {get => reloadTime;}

    protected bool isReloading;
    protected bool triggerHeld;
    
    [SerializeField] 
    protected float verticalRecoil = 0.1f, 
                    horizontalRecoil = 1f, 
                    vertRecoilRecoverTime = 0.1f, 
                    horzRecoilRecoverTime = 0.1f;
    protected Vector3 vertRecoilRecoverVelocity;
    protected float horzRecoilRecoverVelocity;

    protected Projectile newProjectile;
    protected Shell newShell;

    public event Action OnFire;
    public event Action OnFireEnd;
    public bool EffectsPlayer { get => false; }

    protected virtual void Start()
    {
        if (reloadTime <= 0){
            throw new ArgumentOutOfRangeException($"reloadTime of {this.nameOfGun} cannot be <= 0");
        }
        
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
    
    public string DisplayAmmo()
    {
        return $"{bulletsRemaining} / {magSize}";
    }
    

    void RecoilRecovery()
    {
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref vertRecoilRecoverVelocity, vertRecoilRecoverTime);
        transform.localEulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.localEulerAngles.y, 0, ref horzRecoilRecoverVelocity, horzRecoilRecoverTime);
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

    protected virtual IEnumerator ReloadRoutine()
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
