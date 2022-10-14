using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage;
    public float speed;
    public float destroyTime;

    Rigidbody bulletRigidbody;

    void Start()
    {
        bulletRigidbody = GetComponent<Rigidbody>();

        bulletRigidbody.AddForce(transform.forward * speed, ForceMode.Impulse);

        Destroy(gameObject, destroyTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Wall" | collision.gameObject.tag == "Floor")
        {
            Destroy(gameObject);
        }
    }
}
