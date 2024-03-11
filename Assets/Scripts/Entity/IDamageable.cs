using UnityEngine;

public interface IDamageable
{
    public int MaxHealth {get;}
    void TakeHit(int damage, Vector3 hitDirection);

    void TakeDamage(int damage);
}
