
public interface IFirearm
{
    public bool IsReloading {get;}
    public void HoldTrigger();
    public void ReleaseTrigger();
    public void Reload();

    public string GetNameOfGun();
    public int GetBulletsRemaining();
    public int GetMagSize();
}
