using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    PlayerController playerController;
    public Transform weaponHoldPoint;
    public IFirearm equippedGun {get; private set;}
    public GameObject[] guns;
    int currentGunIndex;
    int hiddenLayer;
    int defaultLayer;
    public event Action<float> OnReload;


    void Start()
    {
        hiddenLayer = LayerMask.NameToLayer("Hidden");
        defaultLayer = LayerMask.NameToLayer("Default");
        playerController = GetComponent<PlayerController>();
    
        for(int i = 0; i < guns.Length; i++){
            guns[i] = Instantiate(guns[i], weaponHoldPoint.position, weaponHoldPoint.rotation, weaponHoldPoint);
            ChangeLayer(guns[i], hiddenLayer);
        }
        if (guns.Length > 0){
            currentGunIndex = 0;
            EquipGun(currentGunIndex);
        }
    }

    public void EquipGun(int index)
    {
        if (equippedGun != null){
            ChangeLayer(((MonoBehaviour)equippedGun).gameObject, hiddenLayer);
        }
        equippedGun = guns[index].GetComponent<IFirearm>();
        ChangeLayer(((MonoBehaviour)equippedGun).gameObject, defaultLayer);

        if (equippedGun.LimitsRotation){
            equippedGun.OnFire += OnLimitRotation;
            equippedGun.OnFireEnd += OnLimitRotationEnd;
        }
    }

    public void NextGun()
    {
        if (equippedGun.CanReload() && guns.Length > 0){
            currentGunIndex = (currentGunIndex + 1) % guns.Length;
            equippedGun.ReleaseTrigger();
            EquipGun(currentGunIndex);
        }
    }

    public void Reload()
    {
        if (equippedGun != null && equippedGun.CanReload()){
            equippedGun.Reload();
            OnReload?.Invoke(equippedGun.ReloadTime);
        }
    }

    public void OnTriggerHold()
    {
        if (equippedGun != null){
            equippedGun.HoldTrigger();
        }
    }

    public void OnTriggerRelease()
    {
        if (equippedGun != null){
            equippedGun.ReleaseTrigger();
        }
    }

    public void OnLimitRotation()
    {
        playerController.LimitRotation(true);
    }

    public void OnLimitRotationEnd()
    {
        playerController.LimitRotation(false);
    }

    public Vector3 GunPosition()
    {
        return weaponHoldPoint.position;
    }

    void ChangeLayer(GameObject gun, int layer)
    {
        gun.layer = layer;
        Transform[] children = gun.GetComponentsInChildren<Transform>(includeInactive: true);
        foreach (Transform child in children)
        {
            child.gameObject.layer = layer;
        }
    }
    
}
