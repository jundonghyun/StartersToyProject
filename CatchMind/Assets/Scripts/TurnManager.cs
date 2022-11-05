using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class TurnManager : MonoBehaviourPunCallbacks, IPunObservable
{
    ConnectManager connectManager;

    public List<Player> players = new List<Player>();
    public GameObject[] playerMoves;

    public bool isStart = false;
    public bool isStop = false;
    public float maxTime = 30f;
    public float time;
    public int turn = 0;
    public bool myTurn = false;
    public int Maxturn = 0;
    public int turnCount = 0;

    PhotonView pv;
    DrawLine drawLine;
    Camera playerCam;

    public InputField inputAnswer;
    public Text turnAnswer;
    public string[] words;

    bool isSet = false;
    public string nWord = "";
    public Image timeBar;

    public int DDabong = 0;
    GameObject DDabongObjs;
    Button DDabongBtnObj;

    void Awake()
    {
        connectManager = GameObject.Find("ConnectManager").GetComponent<ConnectManager>();
        pv = GetComponent<PhotonView>();
        inputAnswer = GameObject.Find("InputAnswer").GetComponent<InputField>();
        turnAnswer = GameObject.Find("TurnAnswer").GetComponent<Text>();
        inputAnswer.gameObject.SetActive(false);
        turnAnswer.gameObject.SetActive(false);
        drawLine = GetComponent<DrawLine>();
        playerCam = GameObject.Find("PlayerCam").GetComponent<Camera>();
        GameObject.Find("StartBtn").GetComponent<Button>().onClick.AddListener(StartTurn);
        if (PhotonNetwork.PlayerList[0] == PhotonNetwork.LocalPlayer)
            pv.RequestOwnership();

        timeBar = GameObject.Find("TimeBar").GetComponent<Image>();

        DDabongObjs = GameObject.Find("DDabongs");
        DDabongBtnObj = GameObject.Find("DDabongBtn").GetComponent<Button>();
        DDabongBtnObj.onClick.AddListener(DDabongBtn);
        DDabongObjs.SetActive(false);
        DDabongBtnObj.gameObject.SetActive(false);
    }

    #region Turn

    public void StartTurn()
    {
        if (PhotonNetwork.MasterClient == PhotonNetwork.LocalPlayer)
            PhotonNetwork.Instantiate("Busser", GameObject.Find("GoalPoint").transform.position, Quaternion.identity);

        if (PhotonNetwork.PlayerList[turn] == PhotonNetwork.LocalPlayer)
        {
            pv.RequestOwnership();
            myTurn = true;
        }
        else
            myTurn = false;

        isStart = true;

        nWord = words[Random.Range(0, words.Length)];
        pv.RPC("Setword", RpcTarget.All, nWord);
    }

    [PunRPC]
    public void RpcTurnStart()
    {
        players.Clear();

        Maxturn = PhotonNetwork.PlayerList.Length * 3;
        turnCount = 1;

        Invoke("WaitforTime", 1f);

        GameObject.Find("StartBtn").SetActive(false);
        connectManager.scorePanel.SetActive(true);

        PlayerMove pm = GameObject.Find(PhotonNetwork.LocalPlayer.NickName).GetComponent<PlayerMove>();
        pm.isSpawn = false;

        isSet = true;
    }

    [PunRPC]
    public void RpcNextTurn()
    {
        turnCount++;
        if (turnCount >= Maxturn)
        {
            isStart = false;
            isStop = true;

            foreach (Player p in PhotonNetwork.PlayerList)
            {
                GameObject.Find(p.NickName).transform.position = new Vector3(0, 0, 0);
            }

            GameObject.Find("Canvas").transform.Find("ResultPanel").gameObject.SetActive(true);
            drawLine.ClearLine();

            playerCam.depth = -1;
            drawLine.drawCam.depth = 1;

            playerCam.rect = new Rect(0, 0, 1, 1);
            drawLine.drawCam.rect = new Rect(0.01f, 0.01f, 0.3f, 0.3f);

            playerCam.tag = "MainCamera";
            drawLine.drawCam.tag = "Untagged";

            return;
        }

        turn = turn >= PhotonNetwork.CurrentRoom.PlayerCount - 1 ? 0 : turn + 1;
        time = maxTime;

        if (PhotonNetwork.PlayerList[turn] == PhotonNetwork.LocalPlayer)
        {
            pv.RequestOwnership();
            myTurn = true;
        }
        else
            myTurn = false;

        Invoke("WaitforTime", 1f);

        PlayerMove pm = GameObject.Find(PhotonNetwork.LocalPlayer.NickName).GetComponent<PlayerMove>();
        pm.isSpawn = false;
    }

    public void WaitforTime()
    {
        SetUI();

        GameObject.Find("TurnT").GetComponent<Text>().text = $"{pv.Owner.NickName} 님의 턴";
        GetComponent<DrawLine>().ClearLine();
    }

    public void SetUI()
    {
        if (pv.IsMine)
        {
            isStart = true;

            playerCam.depth = 1;
            drawLine.drawCam.depth = -1;
            playerCam.rect = new Rect(0.35f, 0.02f, 0.3f, 0.3f);
            drawLine.drawCam.rect = new Rect(0, 0, 1, 1);

            playerCam.tag = "Untagged";
            drawLine.drawCam.tag = "MainCamera";

            turnAnswer.gameObject.SetActive(true);
            inputAnswer.gameObject.SetActive(false);

            DDabongBtnObj.gameObject.SetActive(false);
        }
        else
        {
            playerCam.depth = -1;
            drawLine.drawCam.depth = 1;

            playerCam.rect = new Rect(0, 0, 1, 1);
            drawLine.drawCam.rect = new Rect(0.35f, 0.02f, 0.3f, 0.3f);

            playerCam.tag = "MainCamera";
            drawLine.drawCam.tag = "Untagged";

            turnAnswer.gameObject.SetActive(false);
            inputAnswer.gameObject.SetActive(false);

            DDabongBtnObj.gameObject.SetActive(true);
            DDabongBtnObj.interactable = true;
        }
        DDabongObjs.SetActive(true);
    }

    #endregion

    #region CollectAnswer

    public void TimeInit()
    {
        pv.RPC("TimeIsZero", RpcTarget.All);
        // pv.RPC("TimeIsZero", RpcTarget.All);
        turnAnswer.gameObject.SetActive(false);
        while (turnAnswer.text == nWord)
        {
            nWord = words[Random.Range(0, words.Length)];
        }

        pv.RPC("RpcNextTurn", RpcTarget.All);
        pv.RPC("Setword", RpcTarget.All, nWord);
    }

    [PunRPC]
    public void TimeIsZero()
    {
        if (pv.IsMine)
            time = maxTime;
    }

    #endregion

    void Update()
    {
        if (isStart && pv.IsMine)
        {
            if (!isSet)
                pv.RPC("RpcTurnStart", RpcTarget.All);

            if (time <= 0)
            {
                turnAnswer.gameObject.SetActive(false);
                while (turnAnswer.text == nWord)
                {
                    nWord = words[Random.Range(0, words.Length)];
                }

                InitDDabong(pv.Owner);

                pv.RPC("RpcNextTurn", RpcTarget.All);
                pv.RPC("Setword", RpcTarget.All, nWord);
            }
            else
            {
                time -= Time.deltaTime;
                pv.RPC("SetTimeBar", RpcTarget.All, time);
            }
        }
    }

    [PunRPC]
    public void SetTimeBar(float timeV)
    {
        timeBar.fillAmount = (timeV) / maxTime;
    }

    [PunRPC]
    public void Setword(string t)
    {
        nWord = t;
        Invoke("SetWordAndTime", 1f);
    }

    void SetWordAndTime()
    {
        turnAnswer.text = nWord;
        Debug.Log(turnAnswer.text + " == nWord?");
        if (pv.IsMine && !turnAnswer.gameObject.activeSelf)
            turnAnswer.gameObject.SetActive(true);
    }

    #region DDabong

    public void DDabongBtn()
    {
        pv.RPC("AddDDabong", RpcTarget.All);
    }

    [PunRPC]
    void AddDDabong()
    {
        DDabong++;
        pv.RPC("PrintDDabong", RpcTarget.All, DDabong);
    }

    [PunRPC]
    void PrintDDabong(int count)
    {
        DDabongObjs.transform.GetComponentInChildren<Text>().text = $"X {count}";
    }

    public void InitDDabong(Player owner)
    {
        ScoreManager sm = GameObject.Find(owner.NickName).GetComponent<ScoreManager>();
        sm.DDabongToScore(DDabong);

        pv.RPC("PrintDDabong", RpcTarget.All, DDabong);
    }

    #endregion

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // stream.SendNext(nWord);
            // stream.SendNext(time);
            // stream.SendNext(timeBar.fillAmount);
        }
        else
        {
            // nWord = (string)stream.ReceiveNext();
            // time = (float)stream.ReceiveNext();
            // timeBar.fillAmount = (float)stream.ReceiveNext();
        }
    }
}
