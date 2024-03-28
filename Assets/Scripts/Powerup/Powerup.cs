using UnityEngine;

public class Powerup : MonoBehaviour
{
    public enum Variety {LifeOnKill, InfiniteAmmo, InfiniteStamina}
    [SerializeField] Variety kind;
    public Variety variety {get => kind;}
    [SerializeField] float effectDuration;
    public float duration {get => effectDuration;}
    [SerializeField] AudioClip pickupSound;

    Vector3 startingPosition;
    Vector3 startingShellScale;
    Transform shellTransform;

    void Start()
    {
        startingPosition = transform.position;
        shellTransform = transform.Find("Shell");
        startingShellScale = shellTransform.localScale;
    }

    void Update()
    {
        transform.position = Vector3.up * 0.1f * Mathf.Sin(Time.time) + startingPosition;
        shellTransform.localScale = Vector3.one * 0.1f * Mathf.Sin(3 * Time.time) + startingShellScale;
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
