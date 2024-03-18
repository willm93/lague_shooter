using UnityEngine;

[RequireComponent( typeof (LineRenderer))]
[RequireComponent( typeof (CapsuleCollider))]
public class Lazer : MonoBehaviour
{
    public float lifetime = 1f;
    public Transform initPoint;
    LineRenderer lineRenderer;
    CapsuleCollider lazerCollider;
    [SerializeField] LayerMask obstacleMask;
    public int damageRate = 100;
    public float maxDistance = 50f;
    
    void Start()
    {
        lazerCollider = GetComponent<CapsuleCollider>();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = true;

        lineRenderer.SetPosition(0, Vector3.zero);
        lineRenderer.SetPosition(1, Vector3.zero);
        Destroy(gameObject, lifetime);
    }

    void OnTriggerStay(Collider collider)
    {
        IDamageable damageableObject = collider.GetComponent<IDamageable>();
        damageableObject?.TakeHit(Mathf.RoundToInt(damageRate * Time.fixedDeltaTime), transform.forward);
    }

    void FixedUpdate()
    {   
        if (InsideCollider())
        {
            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, Vector3.zero);

            return;
        }

        SetLazerLength();
        SetColliderLength();
    }

    void SetLazerLength()
    {
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

    void SetColliderLength()
    {
        float length = lineRenderer.GetPosition(1).z;
        float center = length / 2f;
        lazerCollider.height = length;
        lazerCollider.center = Vector3.forward * center;
    }

    bool InsideCollider()
    {
        Collider[] initialColliders = Physics.OverlapSphere(transform.position, 0.1f, obstacleMask);
        return initialColliders.Length > 0;
    }
}
