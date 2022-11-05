using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class ScoreManager : MonoBehaviourPunCallbacks, IPunObservable
{
    PhotonView pv;
    Text waringText;
    public int curScore = 0;
    public TextMeshProUGUI scoreText;

    TurnManager turnManager;
    ConnectManager connectManager;

    InputField inputAnswer;
    Text turnAnswer;

    void Awake()
    {
        pv = GetComponent<PhotonView>();
        turnManager = GameObject.FindGameObjectWithTag("TurnManager").GetComponent<TurnManager>();
        connectManager = GameObject.Find("ConnectManager").GetComponent<ConnectManager>();
        inputAnswer = turnManager.inputAnswer;
        turnAnswer = turnManager.turnAnswer;
        waringText = GameObject.Find("inGameLog").GetComponent<Text>();
        inputAnswer.text = "";
    }

    public void SubmitAnswer()
    {
        if (inputAnswer.text == turnAnswer.text)
        {
            pv.RPC("GetDDabong", RpcTarget.All);
            // turnManager.InitDDabong(turnManager.photonView.Owner);

            pv.RPC("AddScore", RpcTarget.All, 1, connectManager.myPlayerIndex, PhotonNetwork.LocalPlayer);
            pv.RPC("PrintLog", RpcTarget.All, $"{pv.Owner.NickName}님, '{inputAnswer.text}' 정답입니다!");

            turnManager.TimeInit();
            GetComponent<PlayerMove>().AudioPlay(5);
        }
        else
        {
            pv.RPC("PrintLog", RpcTarget.All, $"{pv.Owner.NickName}님, 틀렸습니다!");
            pv.RPC("ReturnToStartPos", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName);
            GetComponent<PlayerMove>().AudioPlay(6);
        }
    }

    #region DDabong

    public void DDabongToScore(int point)
    {
        pv.RPC("AddScore", RpcTarget.All, point, connectManager.myPlayerIndex, PhotonNetwork.LocalPlayer);
    }

    [PunRPC]
    void GetDDabong()
    {
        Time.timeScale = 1f;
        if (turnManager.photonView.IsMine)
        {
            int point = turnManager.DDabong;
            ScoreManager sm = GameObject.Find(PhotonNetwork.LocalPlayer.NickName).GetComponent<ScoreManager>();
            sm.DDabongToScore(point);
        }
    }

    #endregion

    public void PrintWarning(string t)
    {
        pv.RPC("PrintLog", RpcTarget.All, t);
    }

    [PunRPC]
    public void PrintLog(string t)
    {
        if (turnManager.isStop)
            turnManager.isStop = false;
        waringText.text = t;
        Invoke("InitLog", 2f);
    }

    public void InitLog()
    {
        waringText.text = "";
    }

    [PunRPC]
    public void AddScore(int pScore, int myIndex, Player p)
    {
        curScore += pScore;
        Text myScoreBox = connectManager.scoreBoxD[myIndex];
        myScoreBox.text = $"{p.NickName} : {curScore} 점";

        turnManager.DDabong = 0;
    }

    [PunRPC]
    public void ReturnToStartPos(string nick)
    {
        GameObject playerObj = GameObject.Find(nick);
        playerObj.transform.position = new Vector3(100, 0, -5f);
    }

    void Update()
    {
        if (pv.IsMine)
        {
            if (inputAnswer.text != "" && Input.GetKeyDown(KeyCode.Return))
            {
                SubmitAnswer();
                inputAnswer.text = "";
            }
        }

        scoreText.text = curScore.ToString() + " 점";
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(curScore);
        }
        else
        {
            curScore = (int)stream.ReceiveNext();
        }
    }
}
