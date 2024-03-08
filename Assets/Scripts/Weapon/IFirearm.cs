
public interface IFirearm
{
    public bool CanReload {get;}
    public float ReloadTime {get;}
    public void HoldTrigger();
    public void ReleaseTrigger();
    public void Reload();

    public string GetNameOfGun();
    public int GetBulletsRemaining();
    public int GetMagSize();
}
