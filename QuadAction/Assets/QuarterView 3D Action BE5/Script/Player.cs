using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private Camera followCamera;
    [SerializeField] private float speed;
    [SerializeField] private GameObject[] weaponse;
    [SerializeField] private bool[] hasWeapon;
    [SerializeField] private int ammo;
    [SerializeField] private int coin;
    [SerializeField] private int health;
    [SerializeField] private int Maxammo;
    [SerializeField] private int Maxcoin;
    [SerializeField] private int Maxhealth;
    [SerializeField] private int MaxhasGrenade;
    [SerializeField] private GameObject[] grenades;
    [SerializeField] private int hasGrenades;
    [SerializeField] private GameObject grenade;

    private Rigidbody rb;
    private float hAxis;
    private float vAxis;
    private bool iDown;
    private Vector3 move;
    private Vector3 dodgeVec;
    private Vector3 lastDir;
    private Vector3 nextVec;
    private Animator anim;
    private bool walkDown;
    private bool jJump;
    private bool isJump;
    private bool isDodge;
    private bool sDown1;
    private bool sDown2;
    private bool sDown3;
    private bool sDown4;
    private bool isSwap;
    private bool fDown;
    private bool rDown;
    private bool gDown;
    private bool isFireReady = true;
    private bool isReload;
    private bool isBorder;
    private bool isDamage;

    private GameObject nearObj;
    private Weapon equipObj;
    private MeshRenderer[] meshs;

    private int eqipWeaponIndex = -1;
    private float fireDelay;
    private bool bossAttack;


    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        meshs = GetComponentsInChildren<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        Turn();
        Jump();
        Attack();
        Reload();
        Swap();
        Dodge();
        Interaction();
        Grenade();
    }

    void Grenade()
    {
        if (hasGrenades == 0) return;

        if (gDown && !isReload && !isSwap)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100f))
            {
                Vector3 nextVec = rayHit.point - gameObject.transform.position;
                nextVec.y = 20;

                GameObject instantiateGrenade = Instantiate(grenade, transform.position, transform.rotation);

                Rigidbody rbGrenade = instantiateGrenade.GetComponent<Rigidbody>();
                rbGrenade.AddForce(nextVec, ForceMode.Impulse);
                rbGrenade.AddTorque(Vector3.back * 10f, ForceMode.Impulse);

                hasGrenades--;
                grenades[hasGrenades].SetActive(false);
            }
        }
    }

    private void FixedUpdate()
    {
        if (!bossAttack)
        {
            Move();
        }

        //FreezRotation();
        //StopToWall();
    }

    void StopToWall()
    {
        Debug.DrawRay(transform.position, transform.forward * 5f, Color.green);
        isBorder = Physics.Raycast(transform.position, transform.forward, 5f, LayerMask.GetMask("Wall"));
    }

    void FreezRotation()
    {
        //angularVelocity 회전속도
        rb.angularVelocity = Vector3.zero;
    }

    void Reload()
    {
        if (equipObj == null) return;

        if (equipObj.type == Weapon.Type.Melee) return;

        if (ammo == 0) return;

        if (rDown && !isJump && !isDodge && !isSwap && isFireReady)
        {
            anim.SetTrigger("doReload");
            isReload = true;

            Invoke("ReloadOut", 3f);
        }
    }

    void ReloadOut()
    {
        int reAmmo = ammo < equipObj.maxAmmo ? ammo : equipObj.maxAmmo;
        equipObj.curAmmo = reAmmo;
        ammo -= reAmmo;
        isReload = false;
    }

    void Attack()
    {
        if (equipObj == null) return;

        fireDelay += Time.deltaTime;
        isFireReady = equipObj.rate < fireDelay;

        if (fDown && isFireReady && !isDodge && !isSwap)
        {
            equipObj.Use();
            anim.SetTrigger(equipObj.type == Weapon.Type.Melee ? "doSwing" : "doShot");
            fireDelay = 0;
        }
    }

    void GetInput()
    {
        hAxis = Input.GetAxis("Horizontal");
        vAxis = Input.GetAxis("Vertical");
        walkDown = Input.GetButton("Walk");
        jJump = Input.GetButtonDown("Jump");
        iDown = Input.GetButtonDown("Interaction");
        sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetButtonDown("Swap2");
        sDown3 = Input.GetButtonDown("Swap3");
        sDown4 = Input.GetButtonDown("Swap4");
        fDown = Input.GetButton("Fire1");
        gDown = Input.GetButtonDown("Fire2");
        rDown = Input.GetButtonDown("Reload");
    }

    void Move()
    {
        move = new Vector3(hAxis, 0, vAxis).normalized;

        if (isDodge)
        {
            move = dodgeVec;
        }

        if (isSwap || !isFireReady || isReload)
        {
            rb.velocity = Vector3.zero;
        }

        if (!isBorder)
        {
            rb.velocity = new Vector3(move.x * speed, rb.velocity.y, move.z * speed);
        }

        anim.SetBool("isRun", move != Vector3.zero);
        anim.SetBool("isWalk", walkDown);
    }

    void Turn()
    {
        if (move != Vector3.zero)
        {
            lastDir = move;
        }

        transform.rotation = Quaternion.LookRotation(lastDir);

        //마우스에 의한 회전
        if (fDown)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100f))
            {
                nextVec = rayHit.point - gameObject.transform.position;
                nextVec.y = 0; //캐릭터가 위를 보지 못하도록
                transform.rotation = Quaternion.LookRotation(nextVec);
                lastDir = nextVec;
            }
        }
    }

    void Jump()
    {
        if (move == Vector3.zero && jJump && !isJump && !isDodge && !isSwap)
        {
            rb.AddForce(Vector3.up * 15f, ForceMode.Impulse);
            anim.SetTrigger("doJump");
            anim.SetBool("isJump", true);
            isJump = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Floor")
        {
            anim.SetBool("isGround", true);
            anim.SetBool("isJump", false);
            isJump = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();

            switch (item.type)
            {
                case Item.Type.Ammo:
                    ammo += item.value;

                    if (ammo > Maxammo)
                    {
                        ammo = Maxammo;
                    }

                    break;

                case Item.Type.Coin:
                    coin += item.value;

                    if (coin > Maxcoin)
                    {
                        coin = Maxcoin;
                    }

                    break;

                case Item.Type.Heart:
                    health += item.value;

                    if (health > Maxhealth)
                    {
                        health = Maxhealth;
                    }

                    break;

                case Item.Type.Grenade:
                    grenades[hasGrenades].SetActive(true);
                    hasGrenades += item.value;

                    if (hasGrenades > MaxhasGrenade)
                    {
                        hasGrenades = MaxhasGrenade;
                    }

                    break;
            }

            Destroy(other.gameObject);
        }
        else if (other.tag == "EnemyBullet")
        {
            if (!isDamage) //적에게 공격당했을때 무적시간을 주기위해서 bool값을 생성
            {
                Bullet enemyBullet = other.GetComponent<Bullet>();
                health -= enemyBullet.damage;

                bool bossAttack = other.name == "Boss MeleeArea";

                StartCoroutine(OnDamage(bossAttack));
            }

            if (other.GetComponent<Rigidbody>() != null) //무적시간때도 미사일은 플레이어에 맞아도 사라져야 함.
            {
                Destroy(other.gameObject);
            }
        }
    }

    IEnumerator OnDamage(bool bossAttack)
    {
        isDamage = true;

        foreach (var mesh in meshs)
        {
            mesh.material.color = Color.yellow;
        }

        if (bossAttack)
        {
            Debug.Log("attack");
            rb.AddForce(-transform.forward * 1000f, ForceMode.Impulse);
        }

        yield return new WaitForSeconds(1f);

        isDamage = false;

        foreach (var mesh in meshs)
        {
            mesh.material.color = Color.white;
        }

        if (bossAttack)
        {
            rb.velocity = Vector3.zero;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Weapon")
        {
            nearObj = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")
        {
            nearObj = null;
        }
    }

    void Dodge()
    {
        if (jJump && move != Vector3.zero && !isDodge && !isJump && !isSwap)
        {
            dodgeVec = move;
            speed *= 2;
            anim.SetTrigger("doDodge");
            isDodge = true;

            Invoke("DodgeOut", 0.5f);
        }
    }

    void DodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;
    }

    void Interaction()
    {
        if (iDown && nearObj != null && !isJump && !isDodge)
        {
            if (nearObj.tag == "Weapon")
            {
                Item item = nearObj.GetComponent<Item>();
                int weaponIndex = item.value;

                hasWeapon[weaponIndex] = true;
                Destroy(nearObj);
            }
        }
    }

    void Swap()
    {
        if (sDown1 && (!hasWeapon[0] || eqipWeaponIndex == 0)) return;
        if (sDown2 && (!hasWeapon[1] || eqipWeaponIndex == 1)) return;
        if (sDown3 && (!hasWeapon[2] || eqipWeaponIndex == 2)) return;
        int weaponIndex = -1;
        if (sDown1) weaponIndex = 0;
        if (sDown2) weaponIndex = 1;
        if (sDown3) weaponIndex = 2;
        if ((sDown1 || sDown2 || sDown3) && !isJump && !isDodge)
        {
            if (equipObj != null && !isSwap)
            {
                equipObj.gameObject.SetActive(false);
            }

            eqipWeaponIndex = weaponIndex;
            equipObj = weaponse[weaponIndex].GetComponent<Weapon>();
            equipObj.gameObject.SetActive(true);

            anim.SetTrigger("doSwap");

            isSwap = true;

            Invoke("SwapOut", 0.4f);
        }
    }

    void SwapOut()
    {
        isSwap = false;
    }
}