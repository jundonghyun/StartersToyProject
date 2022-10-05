using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Boss : Enemy
{
    public GameObject missile;
    public Transform missilePortA;
    public Transform missilePortB;
    public bool isLook;

    private Vector3 lookVec;
    private Vector3 tauntVec;

    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        nav = GetComponent<NavMeshAgent>();
        meshs = GetComponentsInChildren<MeshRenderer>();
        anim = GetComponentInChildren<Animator>();
        
        StartCoroutine(Think());
    }


    // Update is called once per frame
    void Update()
    {
        if (isDead)
        {
            StopAllCoroutines();
            Destroy(gameObject, 3f);
            return;
        }
        
        if (isLook)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            lookVec = new Vector3(h, 0, v) * 5f;
            
            transform.LookAt(target.position + lookVec);
        }
        else
        {
            nav.SetDestination(tauntVec);
        }
    }

    IEnumerator Think()
    {
        yield return new WaitForSeconds(0.1f);

        int ranAction = Random.Range(0, 5);

        switch (ranAction)
        {
            case 0:
            case 1:
                StartCoroutine(Taunt());
                //StartCoroutine(MissileShot());
                break;
            
            case 2:
            case 3:
                StartCoroutine(Taunt());
                //StartCoroutine(RockShot());
                break;
            
            case 4:
                StartCoroutine(Taunt());
                break;
        }
    }

    IEnumerator MissileShot()
    {
        anim.SetTrigger("doShot");

        yield return new WaitForSeconds(0.2f);

        GameObject instantMissileA = Instantiate(missile, missilePortA.position, missilePortA.rotation);
        BossMissile bossA = instantMissileA.GetComponent<BossMissile>();
        bossA.target = target;
        
        yield return new WaitForSeconds(0.3f);
        
        GameObject instantMissileB = Instantiate(missile, missilePortB.position, missilePortB.rotation);
        BossMissile bossB = instantMissileB.GetComponent<BossMissile>();
        bossB.target = target;
        
        yield return new WaitForSeconds(2f);
        
        StartCoroutine(Think());
    }

    IEnumerator RockShot()
    {
        isLook = false;
        nav.isStopped = true;
        anim.SetTrigger("doBigShot");

        Instantiate(Bullet, transform.position, transform.rotation);

        yield return new WaitForSeconds(3f);

        isLook = true;
        nav.isStopped = false;
        StartCoroutine(Think());
    }

    IEnumerator Taunt()
    {
        tauntVec = target.position + lookVec;
        isLook = false;
        nav.isStopped = false;
        boxCollider.enabled = false;
        anim.SetTrigger("doTaunt");
        
        yield return new WaitForSeconds(1.5f);

        meleeArea.enabled = true;
        
        yield return new WaitForSeconds(0.5f);
        
        meleeArea.enabled = false;
        
        yield return new WaitForSeconds(1f);

        isLook = true;
        nav.isStopped = true;
        boxCollider.enabled = true;
        
        StartCoroutine(Think());
    }
}
