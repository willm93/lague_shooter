using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrosshairMask : MonoBehaviour
{
    public float liftime;
    
    void Start()
    {
        Destroy(this.gameObject, liftime);    
    }

    
    void Update()
    {
        this.transform.Translate(Vector3.down * Time.deltaTime * 2 / liftime);    
    }
}
