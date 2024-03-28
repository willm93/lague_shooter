using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    Transform player;
    Vector3 offsetFromCenter;
    Vector3 newPosition;
    Vector3 newPositionVelocity;
    public float newPositionTime = 0.4f;

    bool playerAlive;

    void Start()
    {
        player = FindAnyObjectByType<Player>().transform;
        player.GetComponent<Player>().OnDeath += OnPlayerDeath;
        playerAlive = true;
        offsetFromCenter = this.transform.position;
    }

    void FixedUpdate()
    {   
        if (playerAlive){
            newPosition = player.position + offsetFromCenter;
            this.transform.position = Vector3.SmoothDamp(this.transform.position, newPosition, ref newPositionVelocity, newPositionTime);
        }
    }

    void OnPlayerDeath()
    {
        playerAlive = false;
    }
}
