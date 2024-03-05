using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamageable
{
    public int maxHealth = 100;
    protected int currentHealth;
    protected bool dead;
    public event System.Action OnDeath;
    
    protected virtual void Start()
    {
        currentHealth = maxHealth;
        dead = false;
    }

    public virtual void TakeHit(int damage, Vector3 hitPoint, Vector3 hitDirection)
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
