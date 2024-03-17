using System;
using System.Collections;
using UnityEngine;

public class PowerupController : MonoBehaviour
{
    public bool lifeOnKill {get; private set;}
    public bool infiniteAmmo {get; private set;}
    public bool infiniteStamina {get; private set;}
    float lifeOnKillDuration;
    float infiniteAmmoDuration;
    float infiniteStaminaDuration;

    public event Action<Powerup.Variety, float> OnPowerup;

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
        OnPowerup?.Invoke(kind, duration);

        switch (kind)
        {
            case Powerup.Variety.LifeOnKill:
                if(!lifeOnKill)
                {
                    lifeOnKill = true;
                    StartCoroutine(LifeOnKillRoutine(duration));
                } else {
                    lifeOnKillDuration += duration;
                }
                break;

            case Powerup.Variety.InfiniteAmmo:
                if(!infiniteAmmo)
                {
                    infiniteAmmo = true;
                    StartCoroutine(InfiniteAmmoRoutine(duration));
                } else {
                    infiniteAmmoDuration += duration;
                }
                break;

            case Powerup.Variety.InfiniteStamina:
                if(!infiniteStamina)
                {
                    infiniteStamina = true;
                    StartCoroutine(InfiniteStaminaRoutine(duration));
                } else {
                    infiniteStaminaDuration += duration;
                }
                break;
        }
    }

    IEnumerator LifeOnKillRoutine(float initDuration)
    {
        float refreshRate = 0.1f;
        lifeOnKillDuration = initDuration;

        while (lifeOnKillDuration >= 0)
        {
            lifeOnKillDuration -= refreshRate;
            yield return new WaitForSeconds(refreshRate);
        }
        
        lifeOnKillDuration = 0;
        lifeOnKill = false;
    }

    IEnumerator InfiniteAmmoRoutine(float initDuration)
    {
        float refreshRate = 0.1f;
        infiniteAmmoDuration = initDuration;
        gunController.InfiniteAmmo(true);

        while (infiniteAmmoDuration >= 0)
        {
            infiniteAmmoDuration -= refreshRate;
            yield return new WaitForSeconds(refreshRate);
        }

        infiniteAmmoDuration = 0;
        infiniteAmmo = false;
        gunController.InfiniteAmmo(false);
    }

    IEnumerator InfiniteStaminaRoutine(float initDuration)
    {
        float refreshRate = 0.1f;
        infiniteStaminaDuration = initDuration;
        player.InfiniteStamina(true);

        while (infiniteStaminaDuration >= 0)
        {
            infiniteStaminaDuration -= refreshRate;
            yield return new WaitForSeconds(refreshRate);
        }

        infiniteStaminaDuration = 0;
        infiniteStamina = false;
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
