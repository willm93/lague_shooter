using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (SpriteRenderer))]
public class Crosshair : MonoBehaviour
{
    public float spinSpeed = 2f;
    public Sprite crosshairSprite;
    public Sprite reloadSprite;
    public GameObject maskingSpritePrefab;
    GunController gunController;
    SpriteRenderer spriteRenderer;

    void Start()
    {
        gunController = FindAnyObjectByType<Player>().GetComponent<GunController>();
        gunController.OnReload += OnReload;
        spriteRenderer = this.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        this.transform.Rotate(Vector3.forward * Time.deltaTime * spinSpeed);
    }

    void OnReload(float reloadTime)
    {
        StartCoroutine(OnReloadRoutine(reloadTime));
    }
    IEnumerator OnReloadRoutine(float reloadTime)
    {
        spriteRenderer.sprite = reloadSprite;

        GameObject mask = Instantiate(maskingSpritePrefab, this.transform.position, Quaternion.Euler(new Vector3(90, 0, 0)), this.transform);
        mask.GetComponent<CrosshairMask>().liftime = reloadTime;
        mask = Instantiate(maskingSpritePrefab, this.transform.position, Quaternion.Euler(new Vector3(90, 180, 0)), this.transform);
        mask.GetComponent<CrosshairMask>().liftime = reloadTime;

        yield return new WaitForSeconds(reloadTime);
        spriteRenderer.sprite = crosshairSprite;
    }
}
