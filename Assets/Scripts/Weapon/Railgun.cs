using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Railgun : MonoBehaviour, IFirearm
{
    [SerializeField] 
    string nameOfGun; 
    public string NameOfGun {get => nameOfGun;}

    [SerializeField] GameObject lazerPrefab;
    GameObject currentLazer;
    [SerializeField] ParticleSystem prefireEffect;
    [SerializeField] Transform lazerHolder;

    [SerializeField] AudioClip chargeSound, fireSound, reloadSound, emptyClick;

    [SerializeField] 
    float reloadTime = 0f; 
    public float ReloadTime {get => reloadTime;}
    bool triggerHeld;
    IEnumerator currentFiringRoutine;
    
    [SerializeField] float fireDuration = 1f;
    float chargePercent = 0f;
    [SerializeField] float chargeTime = 1f;
    [SerializeField] float nextFireTime = 0f;
    [SerializeField] float rechargeDelay = 0.1f;

    public void Reload(){}
    public bool CanReload()
    {
        return true;
    }

    public string DisplayAmmo()
    {
        return "Infinite";
    }

    void Update()
    {
        if(Time.time > nextFireTime){
            currentLazer = null;
        }
    }

    public void HoldTriggerTest()
    {
        currentLazer = Instantiate(lazerPrefab, lazerHolder.position, lazerHolder.rotation, lazerHolder);
        currentLazer.GetComponent<Lazer>().initPoint = lazerHolder;
    }

    public void ReleaseTriggerTest()
    {
        if (currentLazer != null){
            Destroy(currentLazer);
        }
    }

    public void HoldTrigger()
    {
        triggerHeld = true;
        prefireEffect.Play();

        if (currentFiringRoutine == null && Time.time > nextFireTime){
            currentFiringRoutine = ChargeFire();
            StartCoroutine(currentFiringRoutine);
        }
    }

    public void ReleaseTrigger()
    {
        triggerHeld = false;
        AudioManager.instance.FadeOutContinuousSound(0.1f);
        prefireEffect.Stop();
        chargePercent = 0;
    }

    IEnumerator ChargeFire()
    {
        while (triggerHeld){
            
            AudioManager.instance.PlayContinuousSound(chargeSound, false);
            while (chargePercent < 1 && triggerHeld){
                chargePercent += Time.deltaTime * 1 / chargeTime;
                yield return null;
            }
            
            if (chargePercent >= 1){
                AudioManager.instance.PlaySound(fireSound);
                FireLazer();
                break;
            }
        }
        currentFiringRoutine = null;
    }

    void FireLazer()
    {
        currentLazer = Instantiate(lazerPrefab, lazerHolder.position, lazerHolder.rotation, lazerHolder);
        currentLazer.GetComponent<Lazer>().initPoint = lazerHolder;
        currentLazer.GetComponent<Lazer>().lifetime = fireDuration;
        nextFireTime = Time.time + fireDuration + rechargeDelay;
    }
}
