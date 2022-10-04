using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public GameObject meshObj;
    public GameObject effectObj;
    public Rigidbody rb;

    private void Start()
    {
        StartCoroutine(Explosion());
    }

    IEnumerator Explosion()
    {
        yield return new WaitForSeconds(3f);
        
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        meshObj.SetActive(false);
        effectObj.SetActive(true);

        RaycastHit[] raycastHits = Physics.SphereCastAll(transform.position, 15f, Vector3.forward, 0, LayerMask.GetMask("Enemy"));
        //레이캐스트 구체를 그냥 그자리에서 쓰고싶다면 SphereCastAll의 거리를 0으로 놔야함.

        foreach (var hitObj in raycastHits)
        {
            hitObj.transform.GetComponent<Enemy>().HitByGrenade(transform.position, true);
        }
        Destroy(gameObject, 5f);
    }
}
