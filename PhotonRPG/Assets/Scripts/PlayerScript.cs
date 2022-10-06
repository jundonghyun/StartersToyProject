using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Cinemachine;
using TMPro;

public class PlayerScript : MonoBehaviourPunCallbacks, IPunObservable
{

    [Header("ETC")]
    public Rigidbody RB;
    public Animator AN;
    public SpriteRenderer SR;
    public PhotonView PV;
    public TextMeshProUGUI NickNameText;
    public Image HealthImage;

    bool isGround;
    Vector3 curPos;

    [Header("PlayerStat")]
    public float moveSpeed;
    GameObject nearObject;

    Vector3 moveVec;
    Vector3 dodgeVec;
    float hAxis;
    float vAxis;
    bool wDown;
    bool jDown;
    bool iDown;
    bool isDodge;

    [Header("Weapon & Item")]
    public GameObject[] weapons;
    public bool[] hasWeapons;

    [Header("Photon")]
    public string playerName;
    public int charN;

    void Awake()
    {
        // 닉네임
        NickNameText.text = PV.IsMine ? PhotonNetwork.NickName : PV.Owner.NickName;
        NickNameText.color = PV.IsMine ? Color.green : Color.red;

        var TCM = GameObject.Find("TargetGroup").GetComponent<CinemachineTargetGroup>();
        TCM.AddMember(this.transform, 1, 0);

        if (PV.IsMine)
        {
            // 2D 카메라
            var CM = GameObject.Find("CMCamera").GetComponent<CinemachineVirtualCamera>();
            CM.Follow = transform;
            CM.LookAt = transform;
            charN = int.Parse(PhotonNetwork.LocalPlayer.CustomProperties["Char"].ToString());
            PV.RPC("SetChar", RpcTarget.AllBuffered, charN);
        }
    }

    [PunRPC]
    void SetChar(int n)
    {
        AN = transform.GetChild(n).GetComponent<Animator>();
        SR = transform.GetChild(n).GetComponent<SpriteRenderer>();
        for (int i = 0; i < transform.childCount; i++)
        {
            if (i == n)
                transform.GetChild(i).gameObject.SetActive(true);
            else if (i == transform.childCount - 1)
                transform.GetChild(i).gameObject.SetActive(true);
            else
                transform.GetChild(i).gameObject.SetActive(false);
        }
    }


    void Update()
    {
        if (PV.IsMine)
        {
            GetInput();
            // ← → 이동
            Move();

            if (hAxis != 0)
            {
                PV.RPC("FlipXRPC", RpcTarget.AllBuffered, hAxis); // 재접속시 filpX를 동기화해주기 위해서 AllBuffered
            }

            // 대시 
            if (jDown && moveVec != Vector3.zero && !isDodge) PV.RPC("Dodge", RpcTarget.All);

            Interaction();
            // 스페이스 총알 발사
            // if (Input.GetKeyDown(KeyCode.Space))
            // {
            //     PhotonNetwork.Instantiate("Bullet", transform.position + new Vector3(SR.flipX ? -0.4f : 0.4f, -0.11f, 0), Quaternion.identity)
            //         .GetComponent<PhotonView>().RPC("DirRPC", RpcTarget.All, SR.flipX ? -1 : 1);
            //     AN.SetTrigger("shot");
            // }
        }
        // IsMine이 아닌 것들은 부드럽게 위치 동기화
        else if ((transform.position - curPos).sqrMagnitude >= 100) transform.position = curPos;
        else transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
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
        iDown = Input.GetButtonDown("Interaction");
    }

    void Move()
    {
        // 방향이나 보정된 거리를 이동할 때에 사용 
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        if (isDodge)
            moveVec = dodgeVec;

        // transform.position += moveVec * moveSpeed * (wDown ? 0.3f : 1f) * Time.deltaTime;
        RB.velocity = new Vector3(moveSpeed * hAxis, RB.velocity.y, moveSpeed * vAxis);

        AN.SetBool("isRun", moveVec != Vector3.zero);
        AN.SetBool("isWalk", wDown);

        if (!isDodge)
        {
            AN.SetFloat("moveH", Mathf.Abs(hAxis));
            AN.SetFloat("moveV", vAxis);
        }
    }

    [PunRPC]
    void Dodge()
    {
        dodgeVec = moveVec;
        moveSpeed *= 2;
        AN.SetBool("doDodge", true);
        isDodge = true;

        Invoke("DodgeOut", 0.6f);
    }

    void DodgeOut()
    {
        moveSpeed *= 0.5f;
        isDodge = false;
        AN.SetBool("doDodge", false);
    }


    [PunRPC]
    void FlipXRPC(float axis) => SR.flipX = axis == 1;

    // [PunRPC]
    // void JumpRPC()
    // {
    //     RB.velocity = Vector2.zero;
    //     RB.AddForce(Vector2.up * 700);
    // }

    public void Hit()
    {
        HealthImage.fillAmount -= 0.1f;
        if (HealthImage.fillAmount <= 0)
        {
            // GameObject.Find("Canvas").transform.Find("RespawnPanel").gameObject.SetActive(true);
            PV.RPC("DestroyRPC", RpcTarget.AllBuffered); // AllBuffered로 해야 제대로 사라져 복제버그가 안 생긴다
        }
    }

    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(HealthImage.fillAmount);
            // stream.SendNext(nearObject);
        }
        else
        {
            curPos = (Vector3)stream.ReceiveNext();
            HealthImage.fillAmount = (float)stream.ReceiveNext();
            // nearObject = (GameObject)stream.ReceiveNext();
        }
    }

    void Interaction()
    {
        if (iDown && nearObject != null && !isDodge) //&&!isJump
        {
            if (nearObject.tag == "weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                PV.RPC("GetItem", RpcTarget.AllBuffered, item.value);
            }
        }
    }

    [PunRPC]
    void GetItem(int value)
    {
        int weaponIndex = value;
        hasWeapons[weaponIndex] = true;

        Destroy(nearObject);
    }

    public void OnTriggerStay(Collider other)
    {
        Debug.Log($"Trigger tag = {other.tag}");
        if (other.tag == "weapon")
            nearObject = other.gameObject;
        // PV.RPC("SetNearObj", RpcTarget.AllBuffered, nearObject);
    }

    public void OnTriggerExit(Collider other)
    {
        Debug.Log($"Trigger out tag = {other.tag}");
        if (other.tag == "weapon")
            nearObject = null;
        // PV.RPC("SetNearObj", RpcTarget.AllBuffered);
    }

    // [PunRPC]
    // void SetNearObj(GameObject obj = null)
    // {
    //     if (nearObject != obj)
    //         nearObject = obj;
    // }
}