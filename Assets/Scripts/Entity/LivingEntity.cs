using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamageable
{
    [SerializeField] protected int maxHealth = 100;
    public int MaxHealth {get => maxHealth;}

    public int currentHealth {get; protected set;}
    protected bool dead;
    public event System.Action OnDeath;
    
    protected virtual void Start()
    {
        currentHealth = maxHealth;
        dead = false;
    }

    public virtual void TakeHit(int damage, Vector3 hitDirection)
    {
        TakeDamage(damage);
    }

    public virtual void TakeDamage(int damage)
    {
        //Debug.Log(this.name + " took " + damage + " damage");
        currentHealth -= damage;
        if (currentHealth <= 0 && !dead){
            Die();
        }
    }
    
    [ContextMenu("Self Destruct")]
    protected virtual void Die()
    {
        dead = true;
        OnDeath?.Invoke();
        StopAllCoroutines();
        Destroy(this.gameObject);
    }
}
