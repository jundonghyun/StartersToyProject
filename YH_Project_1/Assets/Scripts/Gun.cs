using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public GameObject shellPrefab;
    public Transform shellPoint;
    public float rate;


    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void Shot()
    {
        GameObject bulletInstance = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        Invoke("MakeShell", 0.05f);
    }

    void MakeShell()
    {
        Instantiate(shellPrefab, shellPoint.position, shellPoint.rotation);
    }
}
