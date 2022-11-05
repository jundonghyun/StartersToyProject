using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class PlayerMove : MonoBehaviourPunCallbacks, IPunObservable
{
    Animator anim;
    PhotonView pv;
    Vector3 curPos;
    Rigidbody rb;
    float hAxis = 0;
    float vAxis = 0;
    bool wDown = false;
    bool jDown = false;
    bool fDown = false;

    bool isJump = false;
    bool isFireReady = true;

    Vector3 moveVec;
    public Weapon equipWeapon;
    public GameObject[] weapons;

    public Vector3 painterPos;
    public Vector3 startPos;
    public bool isSpawn = false;

    public float moveSpeed = 15f;
    public Vector3 cameraOffset;
    public float jumpForce = 15f;
    public float power = 10f;
    float fireDelay;

    public TextMeshProUGUI nameT;
    public TextMeshProUGUI scoreT;
    ConnectManager connectManager;
    TurnManager turnManager;
    InputField inputAnswer;
    Camera playerCam;

    public VariableJoystick joy;
    public Button ZBtn;
    public bool isZdown;
    public Button XBtn;
    public bool isXdown;

    public AudioSource audioSource;
    public AudioClip[] clips;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        pv = GetComponent<PhotonView>();
        playerCam = GameObject.Find("PlayerCam").GetComponent<Camera>();
        curPos = transform.position;
        turnManager = GameObject.FindGameObjectWithTag("TurnManager").GetComponent<TurnManager>();
        inputAnswer = turnManager.inputAnswer;
        joy = GameObject.Find("JoyStick").GetComponentInChildren<VariableJoystick>();
        ZBtn = GameObject.Find("JoyStick").transform.Find("Zbtn").GetComponent<Button>();
        XBtn = GameObject.Find("JoyStick").transform.Find("Xbtn").GetComponent<Button>();

        painterPos = GameObject.Find("PainterPoint").transform.position;
        startPos = GameObject.Find("StartPoint").transform.position;

        connectManager = GameObject.Find("ConnectManager").GetComponent<ConnectManager>();
        if (pv.IsMine)
        {
            pv.RPC("ChangeUserName", RpcTarget.AllBuffered, connectManager.myName);
            ZBtn.onClick.AddListener(() => { isZdown = true; });
            XBtn.onClick.AddListener(() => { isXdown = true; });
        }
    }

    // public void AudioPlay(int i, bool isLoop = false)
    // {
    //     if (!isLoop)
    //     {
    //         // pv.RPC("RPCAudioPlay", RpcTarget.All, i);
    //         RPCAudioPlay(i);
    //     }

    //     else
    //     {
    //         // pv.RPC("RPCAudioPlayLoop", RpcTarget.All, i);
    //         RPCAudioPlayLoop(i);
    //     }

    // }

    // [PunRPC]
    public void AudioPlay(int i)
    {
        audioSource.PlayOneShot(clips[i]);
    }

    // [PunRPC]
    public void AudioPlayLoop(int i)
    {
        audioSource.clip = clips[i];
        audioSource.Play();
    }

    [PunRPC]
    public void ChangeUserName(string n)
    {
        nameT.text = n;
        gameObject.name = n;
    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetKeyDown(KeyCode.X);
        fDown = Input.GetKeyDown(KeyCode.Z);
    }

    public void Move()
    {
        if (joy.Horizontal != 0 || joy.Vertical != 0)
        {
            hAxis = joy.Horizontal;
            vAxis = joy.Vertical;
        }

        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        if (!isFireReady)
            moveVec = Vector3.zero;

        AudioPlay(0);

        // transform.position += moveVec * moveSpeed * (wDown ? 0.3f : 1f) * Time.deltaTime;
        rb.position += moveVec * moveSpeed * (wDown ? 0.3f : 1f) * Time.deltaTime;

        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);

        transform.LookAt(transform.position + moveVec);
    }

    public void Jump()
    {
        if ((jDown || isXdown) && !isJump)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isJump = true;
            anim.SetTrigger("doJump");
            anim.SetBool("isJump", isJump);
            isXdown = false;
        }
        AudioPlay(1);
    }

    public void Attack()
    {
        if (equipWeapon == null)
            return;

        AudioPlay(3);

        fireDelay += Time.deltaTime;

        isFireReady = equipWeapon.rate < fireDelay;

        if ((fDown || isZdown) && isFireReady)
        {
            equipWeapon.Use();
            anim.SetTrigger("doSwing");
            fireDelay = 0;
            isZdown = false;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Ground")
        {
            isJump = false;
            anim.SetBool("isJump", isJump);
            AudioPlay(2);
        }

        if (other.gameObject.tag == "Busser" && !other.gameObject.GetComponent<Busser>().isBusser && other.contacts[0].normal.y > 0.7f)
        {
            if (pv.IsMine)
            {
                turnManager.inputAnswer.gameObject.SetActive(true);
                turnManager.inputAnswer.Select();
                StartCoroutine(BackToStartPoint());
            }
            pv.RPC("BusserOn", RpcTarget.All);
            // this.gameObject.transform.position = GameObject.Find("SubmitPoint").transform.position;
            AudioPlay(4);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Melee" && other.gameObject.GetComponent<Weapon>().pm != this)
        {
            Vector3 dir = (this.transform.position - other.transform.position).normalized;
            dir.y = 0;
            rb.AddForce(dir * power, ForceMode.Impulse);
        }
    }

    [PunRPC]
    public void BusserOn()
    {
        turnManager.isStop = true;
        GameObject.FindGameObjectWithTag("Busser").GetComponent<Busser>().isBusser = true;
        Invoke("BusserOff", 5f);
        // StartCoroutine(BackToStartPoint());
    }

    public void BusserOff()
    {
        if (turnManager.isStop)
            turnManager.isStop = false;
        GameObject.FindGameObjectWithTag("Busser").GetComponent<Busser>().isBusser = false;
    }

    public IEnumerator BackToStartPoint()
    {
        // pm.SetTime(0f);
        // yield return new WaitForSecondsRealtime(5f);
        AudioPlayLoop(7);
        GetComponent<ScoreManager>().PrintWarning("5");
        yield return new WaitForSeconds(1f);
        GetComponent<ScoreManager>().PrintWarning("4");
        yield return new WaitForSeconds(1f);
        GetComponent<ScoreManager>().PrintWarning("3");
        yield return new WaitForSeconds(1f);
        GetComponent<ScoreManager>().PrintWarning("2");
        yield return new WaitForSeconds(1f);
        GetComponent<ScoreManager>().PrintWarning("1");
        yield return new WaitForSeconds(1f);
        audioSource.Stop();
        // pm.SetTime(1f);
        turnManager.inputAnswer.gameObject.SetActive(false);
        this.gameObject.transform.position = new Vector3(100, 0, -5f);
        turnManager.isStop = false;
    }

    [PunRPC]
    public void ActiveWeapon(bool isActive)
    {
        equipWeapon.transform.parent.gameObject.SetActive(isActive);
    }

    public void SettingPosition()
    {
        if (turnManager.myTurn)
        {
            transform.position = painterPos;
            pv.RPC("ActiveWeapon", RpcTarget.All, false);
            isSpawn = true;
        }
        else
        {
            transform.position = startPos;
            pv.RPC("ActiveWeapon", RpcTarget.All, true);
            isSpawn = true;
        }
    }

    public void SetTime(float timeV)
    {
        pv.RPC("StopTime", RpcTarget.All, timeV);
    }

    [PunRPC]
    void StopTime(float timeV)
    {
        Time.timeScale = timeV;
    }

    // public IEnumerator BackToStartPoint()
    // {
    //     // pm.SetTime(0f);
    //     // yield return new WaitForSecondsRealtime(5f);
    //     yield return new WaitForSeconds(5f);
    //     // pm.SetTime(1f);
    //     inputAnswer.gameObject.SetActive(false);
    //     pmObj.gameObject.transform.position = pmObj.startPos;
    //     turnManager.isStop = false;
    // }

    void Update()
    {
        if (transform.position.y < -15f)
            isSpawn = false;

        if (pv.IsMine && !turnManager.isStop)
        {
            if (turnManager.myTurn)
            {
                if (turnManager.isStart)
                {
                    if (!isSpawn)
                    {
                        Invoke("SettingPosition", 0.5f);
                    }
                    return;
                }
            }
            else
            {
                if (turnManager.isStart && !isSpawn)
                {
                    Invoke("SettingPosition", 0.5f);
                }
            }

            if (inputAnswer.isFocused)
                return;

            GetInput();
            Move();
            Jump();
            Attack();
        }
        else if ((transform.position - curPos).sqrMagnitude >= 100) transform.position = curPos;
        else transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
    }

    void LateUpdate()
    {
        if (pv.IsMine)
        {
            playerCam.transform.position = transform.position + cameraOffset;
            Vector3 dir = (transform.Find("Canvas").transform.position - playerCam.transform.position).normalized;
            transform.Find("Canvas").transform.LookAt(transform.Find("Canvas").transform.position + dir);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(isSpawn);
            stream.SendNext(equipWeapon.meleeArea.enabled);
            stream.SendNext(equipWeapon.trailEffect.enabled);
        }
        else
        {
            curPos = (Vector3)stream.ReceiveNext();
            isSpawn = (bool)stream.ReceiveNext();
            equipWeapon.meleeArea.enabled = (bool)stream.ReceiveNext();
            equipWeapon.trailEffect.enabled = (bool)stream.ReceiveNext();
        }
    }
}
