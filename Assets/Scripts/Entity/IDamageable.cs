using UnityEngine;

public interface IDamageable
{
    void TakeHit(int damage, Vector3 hitPoint, Vector3 hitDirection);

    void TakeDamage(int damage);
}
