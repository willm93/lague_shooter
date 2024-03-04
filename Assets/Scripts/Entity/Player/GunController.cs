using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public Transform weaponHoldPoint;
    Gun equippedGun;
    public Gun[] guns;
    int currentGunIndex;
    int hiddenLayer;
    int defaultLayer;


    void Start()
    {
        hiddenLayer = LayerMask.NameToLayer("Hidden");
        defaultLayer = LayerMask.NameToLayer("Default");

        if (guns != null){
            for(int i = 0; i < guns.Length; i++){
                guns[i] = Instantiate<Gun>(guns[i], weaponHoldPoint.position, weaponHoldPoint.rotation, weaponHoldPoint);
                ChangeLayer(guns[i], hiddenLayer);
            }
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
        if (!equippedGun.IsReloading){
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
        foreach(Transform child in gun.transform){
            child.gameObject.layer = layer;
            foreach(Transform grandchild in child.transform){
                grandchild.gameObject.layer = layer;
            }
        }
    }
    
}
