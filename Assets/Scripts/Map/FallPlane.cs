using UnityEngine;

[RequireComponent (typeof (BoxCollider))]
public class FallPlane : MonoBehaviour
{
    void OnTriggerEnter(Collider collider)
    {
        IDamageable damageableObject = collider.GetComponent<IDamageable>();
        damageableObject?.TakeHit(damageableObject.MaxHealth, transform.forward);
    }
    
}
