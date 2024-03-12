using UnityEngine;

public class Powerup : MonoBehaviour
{
    public enum Variety {LifeOnKill, InfiniteAmmo, InfiniteStamina}
    [SerializeField] Variety kind;
    public Variety variety {get => kind;}
    [SerializeField] float effectDuration;
    public float duration {get => effectDuration;}
    [SerializeField] AudioClip pickupSound;

    void Update()
    {
     //animate   
    }

    void OnTriggerEnter(Collider collider)
    {
        PowerupController powerCon = collider.GetComponent<PowerupController>();
        if (powerCon != null){
            powerCon.PowerUp(kind, effectDuration);
            AudioManager.instance.PlaySound(pickupSound);
            Destroy(gameObject);
        }
    }
}
