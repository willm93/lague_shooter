using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof (LineRenderer))]
[RequireComponent( typeof (CapsuleCollider))]
public class Lazer : MonoBehaviour
{
    public float lifetime = 1f;
    public Transform initPoint;
    LineRenderer lineRenderer;
    [SerializeField] LayerMask obstacleMask;
    public int damageRate = 100;
    public float maxDistance = 50f;
    
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = true;

        lineRenderer.SetPosition(0, Vector3.zero);
        lineRenderer.SetPosition(1, Vector3.zero);
        Destroy(gameObject, lifetime);
    }

    void OnTriggerStay(Collider collider)
    {
        //will not work if hit.collider.CompareTag("Enemy") is changed to !hit.collider.CompareTag("Obstacle") to make it more general
        //moving causes the raycast to hit something "Untagged" inside an obstacle, or "Player" if the players rigidbody interpolate is on

        Ray ray = new Ray(initPoint.position, collider.transform.position - initPoint.position);
        //Debug.DrawRay(initPoint.position, collider.transform.position - initPoint.position);
        if (!InsideCollider() && Physics.Raycast(ray, out RaycastHit hit, maxDistance) && hit.collider.CompareTag("Enemy")){
            //Debug.DrawRay(initPoint.position, collider.transform.position - initPoint.position, Color.red);
            //Debug.Log("On Stay: " + hit.collider.tag);
            IDamageable damageableObject = collider.GetComponent<IDamageable>();
            damageableObject?.TakeHit(Mathf.RoundToInt(damageRate * Time.fixedDeltaTime), hit.point, transform.forward);
        }
    }

    void FixedUpdate()
    {   
        if (InsideCollider()){
            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, Vector3.zero);

            return;
        }

        Ray ray = new Ray(transform.position, transform.forward);
        bool cast = Physics.Raycast(ray, out RaycastHit hit, maxDistance, obstacleMask);
        
        Vector3 lazerEndPoint;
        if(cast){
            lazerEndPoint = transform.InverseTransformPoint(hit.point);
        } else {
            lazerEndPoint = Vector3.forward * maxDistance;
        }

        lineRenderer.SetPosition(0, Vector3.zero);
        lineRenderer.SetPosition(1, lazerEndPoint);
    }

    bool InsideCollider()
    {
        Collider[] initialColliders = Physics.OverlapSphere(transform.position, 0.1f, obstacleMask);
        return initialColliders.Length > 0;
    }
}
