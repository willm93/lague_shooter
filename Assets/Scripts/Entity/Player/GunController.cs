using System;
using UnityEngine;

public class GunController : MonoBehaviour
{
    Player player;
    public Transform weaponHoldPoint;
    public IFirearm equippedGun {get; private set;}
    public GameObject[] guns;
    int currentGunIndex;
    int hiddenLayer;
    int defaultLayer;
    public event Action<float> OnReload;
    bool subbedToGun = false;


    void Start()
    {
        hiddenLayer = LayerMask.NameToLayer("Hidden");
        defaultLayer = LayerMask.NameToLayer("Default");
        player = GetComponent<Player>();
    
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

            if(subbedToGun){
                equippedGun.OnFire -= OnGunFire;
                equippedGun.OnFireEnd -= OnGunFireEnd;
                subbedToGun = false;
            }
        }


        equippedGun = guns[index].GetComponent<IFirearm>();
        ChangeLayer(((MonoBehaviour)equippedGun).gameObject, defaultLayer);

        if (equippedGun.EffectsPlayer){
            equippedGun.OnFire += OnGunFire;
            equippedGun.OnFireEnd += OnGunFireEnd;
            subbedToGun = true;
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

    public void OnGunFire()
    {
        player.LimitRotation(true);
        player.BigRecoil();
    }

    public void OnGunFireEnd()
    {
        player.LimitRotation(false);
    }

    public void InfiniteAmmo(bool isOn)
    {
        foreach(GameObject gun in guns){
            gun.GetComponent<IFirearm>().InfiniteAmmo(isOn);
        }
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
