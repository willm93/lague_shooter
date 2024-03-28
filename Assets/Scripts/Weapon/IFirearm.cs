
using System;

public interface IFirearm
{
    public string NameOfGun {get;}
    public float ReloadTime {get;}
    public string DisplayAmmo();
    public bool CanReload();
    public void HoldTrigger();
    public void ReleaseTrigger();
    public void Reload();

    public bool EffectsPlayer {get;}
    public event Action OnFire;
    public event Action OnFireEnd;

    public void InfiniteAmmo(bool isOn);
}
