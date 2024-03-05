using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
    public float spinSpeed = 2f;
    void Update()
    {
        this.transform.Rotate(Vector3.forward * Time.deltaTime * spinSpeed);     
    }
}
