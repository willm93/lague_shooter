using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public Transform weaponHoldPoint;
    public Gun equippedGun {get; private set;}
    public Gun[] guns;
    int currentGunIndex;
    int hiddenLayer;
    int defaultLayer;


    void Start()
    {
        hiddenLayer = LayerMask.NameToLayer("Hidden");
        defaultLayer = LayerMask.NameToLayer("Default");
    
        for(int i = 0; i < guns.Length; i++){
            guns[i] = Instantiate<Gun>(guns[i], weaponHoldPoint.position, weaponHoldPoint.rotation, weaponHoldPoint);
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
            ChangeLayer(equippedGun, hiddenLayer);
        }
        equippedGun = guns[index];
        ChangeLayer(equippedGun, defaultLayer);
    }

    public void NextGun()
    {
        if (!equippedGun.IsReloading && guns.Length > 0){
            currentGunIndex = (currentGunIndex + 1) % guns.Length;
            equippedGun.ReleaseTrigger();
            EquipGun(currentGunIndex);
        }
    }

    public void Reload()
    {
        if (equippedGun != null){
            equippedGun.Reload();
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

    public Vector3 GunPosition()
    {
        return weaponHoldPoint.position;
    }

    void ChangeLayer(Gun gun, int layer)
    {
        gun.gameObject.layer = layer;
        Transform[] children = gun.GetComponentsInChildren<Transform>(includeInactive: true);
        foreach (Transform child in children)
        {
            child.gameObject.layer = layer;
        }
    }
    
}
