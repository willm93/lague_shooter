using UnityEngine;

public abstract class Enemy : LivingEntity
{
    public static event System.Action OnDeathStatic;

    [SerializeField] protected AudioClip attackSound;
    [SerializeField] protected AudioClip deathSound;
    [SerializeField] protected ParticleSystem deathEffectPrefab;
    protected bool deathEffectPlayed = false;

    protected Material myMaterial;
    protected Color originalColor;

    protected virtual void Awake()
    {
        myMaterial = GetComponent<Renderer>().material;
    }

    public abstract void SetCharacteristics(EnemyWave wave);

    protected override void Start()
    {
        base.Start();
        originalColor = myMaterial.color;
    }

    public override void TakeHit(int damage, Vector3 hitDirection)
    {
        if (damage >= currentHealth && !deathEffectPlayed){            
            float effectLifetime = deathEffectPrefab.main.startLifetime.constant;
            ParticleSystem deathEffect = Instantiate(deathEffectPrefab, transform.position, Quaternion.FromToRotation(Vector3.forward, hitDirection));
            deathEffect.GetComponent<Renderer>().material.color = originalColor;
            deathEffect.Play();
            Destroy(deathEffect.gameObject, effectLifetime);
            deathEffectPlayed = true;
        }
        base.TakeHit(damage, hitDirection);
    }

    protected override void Die()
    {
        if (deathSound != null){
            AudioManager.instance.PlaySound(deathSound);
        } else {
            AudioManager.instance.PlaySound("Enemy Death");
        }
        OnDeathStatic?.Invoke();
        base.Die();
    }
}
