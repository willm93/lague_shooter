using System.Collections;
using UnityEngine;

public class PowerupController : MonoBehaviour
{
    public bool lifeOnKill {get; private set;}
    public bool infiniteAmmo {get; private set;}
    public bool infinitStamina {get; private set;}

    Player player;
    GunController gunController;

    void Start()
    {
        player = GetComponent<Player>();
        gunController = GetComponent<GunController>();
        Enemy.OnDeathStatic += OnLifeOnKill;
    }

    public void PowerUp(Powerup.Variety kind, float duration)
    {
        switch (kind){
            case Powerup.Variety.LifeOnKill:
                StartCoroutine(LifeOnKillRoutine(duration));
                break;
            case Powerup.Variety.InfiniteAmmo:
                StartCoroutine(InfiniteAmmoRoutine(duration));
                break;
            case Powerup.Variety.InfiniteStamina:
                StartCoroutine(InfiniteStaminaRoutine(duration));
                break;
        }
    }

    IEnumerator LifeOnKillRoutine(float duration)
    {
        lifeOnKill = true;
        yield return new WaitForSeconds(duration);
        lifeOnKill = false;
    }

    IEnumerator InfiniteAmmoRoutine(float duration)
    {
        gunController.InfiniteAmmo(true);
        yield return new WaitForSeconds(duration);
        gunController.InfiniteAmmo(false);
    }

    IEnumerator InfiniteStaminaRoutine(float duration)
    {
        player.InfiniteStamina(true);
        yield return new WaitForSeconds(duration);
        player.InfiniteStamina(false);
    }

    public void OnLifeOnKill(bool notUsed)
    {
        if (lifeOnKill){
            player.Heal(5);
        }
    }

    void OnDestroy()
    {
        Enemy.OnDeathStatic -= OnLifeOnKill;
    }
}
