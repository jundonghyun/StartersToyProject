using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RPGPlayer : MonoBehaviourPunCallbacks
{
    public string playerName;
    public float moveSpeed;
    public float jumpPower;
    float hAxis;
    float vAxis;
    bool wDown;
    bool jDown;

    bool isJump;
    bool isDodge;

    Vector3 moveVec;
    Vector3 dodgeVec;

    Rigidbody rigid;

    Animator anim;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rigid = GetComponent<Rigidbody>();
    }


    void Update()
    {
        GetInput();
        Move();
        Turn();
        // Jump();
        Dodge();
    }

    void GetInput()
    {
        // 플레이어의 rigidbody - collision detection을 continuous로 하면, 
        // cpu를 조금 더 잡아먹는 대신에 계속 충돌을 잡음 
        // transform으로 이동해서 충돌을 무시하는 경우를 방지 
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
    }

    void Move()
    {
        // 방향이나 보정된 거리를 이동할 때에 사용 
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        if (isDodge)
            moveVec = dodgeVec;

        transform.position += moveVec * moveSpeed * (wDown ? 0.3f : 1f) * Time.deltaTime;

        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);

        if (!isDodge)
        {
            anim.SetFloat("moveH", Mathf.Abs(hAxis));
            anim.SetFloat("moveV", vAxis);
        }
    }

    void Turn()
    {
        // 3d에서만 함 
        // 나아가는 방향으로 바라보기 
        if (gameObject.name == "Player (1)")
            transform.LookAt(transform.position + moveVec);
        else
        {
            if (hAxis > 0)
            {
                GetComponentInChildren<SpriteRenderer>().flipX = true;
            }
            else if (hAxis < 0)
            {
                GetComponentInChildren<SpriteRenderer>().flipX = false;
            }
        }
    }

    // void Jump()
    // {
    //     if (jDown && moveVec == Vector3.zero && !isJump && !isDodge)
    //     {
    //         rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
    //         anim.SetBool("isJump", true);
    //         anim.SetTrigger("doJump");
    //         isJump = true;
    //     }
    // }

    void Dodge()
    {
        if (jDown && moveVec != Vector3.zero && !isJump && !isDodge)
        {
            dodgeVec = moveVec;
            moveSpeed *= 2;
            anim.SetTrigger("doDodge");
            isDodge = true;

            Invoke("DodgeOut", 0.7f);
        }
    }

    void DodgeOut()
    {
        moveSpeed *= 0.5f;
        isDodge = false;
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Floor")
        {
            anim.SetBool("isJump", false);
            isJump = false;
        }
    }

    void FixedUpdate()
    {

    }
}
