using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public enum Type
    {
        A,
        B,
        C,
        D
    };

    public Type enemyType; //타입을 지정할 변수
    public int maxHealth;
    public int curHealth;
    public Transform target;
    public bool isChase;
    public BoxCollider meleeArea;
    public bool isAttack;
    public GameObject Bullet;
    public Animator anim;
    public bool isDead;
    
    public Rigidbody rb;
    public BoxCollider boxCollider;
    public NavMeshAgent nav;
    public MeshRenderer[] meshs;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        nav = GetComponent<NavMeshAgent>();
        
        meshs = GetComponentsInChildren<MeshRenderer>();
        anim = GetComponentInChildren<Animator>();

        if (enemyType != Type.D)
        {
            Invoke("ChaseStart", 2f);
        }
        
    }

    private void Update()
    {
        if (nav.enabled && enemyType != Type.D)
        {
            nav.SetDestination(target.position);
            nav.isStopped = !isChase;
        }
    }

    void Targeting()
    {
        if (enemyType != Type.D && !isDead)
        {
            float targetRadius = 0f;
            float targetRange = 0f;

            switch (enemyType)
            {
                case Type.A:
                    targetRadius = 1.5f;
                    targetRange = 3f;
                    break;
            
                case Type.B:
                    targetRadius = 1f;
                    targetRange = 12f;
                    break;
            
                case Type.C:
                    targetRadius = 0.5f;
                    targetRange = 25f;
                    break;
            }

            RaycastHit[] rayHits =
                Physics.SphereCastAll(transform.position, targetRadius, transform.forward, 
                    targetRange, LayerMask.GetMask("Player"));

            if (rayHits.Length > 0 && !isAttack)
            {
                StartCoroutine(Attack());
            }
        }
    }

    IEnumerator Attack()
    {
        isChase = false;
        isAttack = true;
        anim.SetBool("isAttack", true);

        switch (enemyType)
        {
            case  Type.A:
                yield return new WaitForSeconds(0.2f);
                meleeArea.enabled = true;
        
                yield return new WaitForSeconds(1f);
                meleeArea.enabled = false;
        
                yield return new WaitForSeconds(1f);
                break;
                
            case Type.B:
                yield return new WaitForSeconds(0.1f);
                rb.AddForce(transform.forward * 20, ForceMode.Impulse);
                meleeArea.enabled = true;
                
                yield return new WaitForSeconds(0.5f);
                rb.velocity = Vector3.zero;
                meleeArea.enabled = false;
                
                yield return new WaitForSeconds(2f);
                break;
            
            case Type.C:
                yield return new WaitForSeconds(0.5f);
                GameObject instantBullet = Instantiate(Bullet, transform.position, transform.rotation);
                Rigidbody rbBullet = instantBullet.GetComponent<Rigidbody>();

                rbBullet.velocity = transform.forward * 20f;
                
                yield return new WaitForSeconds(2f);
                break;
        }

        isChase = true;
        isAttack = false;
        anim.SetBool("isAttack", false);
    }

    private void FixedUpdate()
    {
        Targeting();
        FreezVelocity();
    }

    void ChaseStart()
    {
        isChase = true;
        anim.SetBool("isWalk", true);
    }

    void FreezVelocity()
    {
        if (isChase)
        {
            //angularVelocity 회전속도
            rb.angularVelocity = Vector3.zero;
            rb.velocity = Vector3.zero;
        }
    }

    public void HitByGrenade(Vector3 explosionPos, bool isGrenade)
    {
        curHealth -= 100;
        Vector3 reActVec = transform.position - explosionPos;
        
        StartCoroutine(OnDamage(reActVec, isGrenade));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Melee")
        {            
            Weapon weapon = other.GetComponent<Weapon>();
            curHealth -= weapon.damage;

            Vector3 reActVec = transform.position - other.transform.position;
            StartCoroutine(OnDamage(reActVec, false));
        }
        else if (other.gameObject.tag == "Bullet")
        {
            Bullet weapon = other.GetComponent<Bullet>();
            curHealth -= weapon.damage;
            
            Vector3 reActVec = transform.position - other.transform.position;
            Destroy(other.gameObject);

            StartCoroutine(OnDamage(reActVec, false));
        }
    }

    IEnumerator OnDamage(Vector3 reActVec, bool isGrenade)
    {
        foreach (var mesh in meshs)
        {
            mesh.material.color = Color.red;
        }
        yield return new WaitForSeconds(0.1f);

        if (curHealth > 0)
        {
            foreach (var mesh in meshs)
            {
                mesh.material.color = Color.white;
            }
        }
        else
        {
            anim.SetTrigger("doDie");
            isChase = false;
            nav.enabled = false;
            isDead = true;
            gameObject.layer = 12;
            
            foreach (var mesh in meshs)
            {
                mesh.material.color = Color.gray;
            }
            
            if (isGrenade)
            {
                reActVec = reActVec.normalized;
                reActVec += Vector3.up * 3;

                rb.freezeRotation = false;
                rb.AddForce(reActVec * 5, ForceMode.Impulse);
                rb.AddTorque(reActVec * 15, ForceMode.Impulse);
            }
            else
            {
                reActVec = reActVec.normalized;
                reActVec += Vector3.up;
            
                rb.AddForce(reActVec * 10, ForceMode.Impulse);
            
            }

            if (enemyType != Type.D)
            {
                Destroy(gameObject, 4f);
            }
        }
    }
}
