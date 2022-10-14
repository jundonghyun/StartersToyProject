using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : MonoBehaviour
{
    public float destroyTime = 3f;

    Rigidbody shellRigidbody;

    void Start()
    {
        shellRigidbody = GetComponent<Rigidbody>();
        
        shellRigidbody.AddForce(transform.forward * Random.Range(-5f, -3f) + Vector3.up * Random.Range(3f, 5f), ForceMode.Impulse);
        shellRigidbody.AddTorque(Vector3.up * Random.Range(5, 10) + Vector3.forward * Random.Range(5, 15), ForceMode.Impulse);

        Destroy(gameObject, destroyTime);
    }
}
