using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class ConnectManager : MonoBehaviourPunCallbacks
{
    public GameObject LoginPanel;
    public InputField InputField;
    public Text inGameLog;
    public bool isLoaded = false;
    public string myName = "";

    [Header("scores")]
    public GameObject scorePanel;
    public GameObject scorePanelD;
    public Text[] scoreBox;
    public Text[] scoreBoxD;
    public int myPlayerIndex = 0;

    void Awake()
    {
        scorePanel = GameObject.Find("ScoresPanel");
        scorePanelD = GameObject.Find("ScoresPanelD");
        scoreBox = scorePanel.transform.GetComponentsInChildren<Text>();
        scoreBoxD = scorePanelD.transform.GetComponentsInChildren<Text>();
        scorePanel.SetActive(false);
        scorePanelD.SetActive(false);
        foreach (Text sb in scoreBox)
        {
            sb.gameObject.SetActive(false);
        }
    }

    public void ConnectBtn()
    {
        if (InputField.text == "")
        {
            // inGameLog.text = "닉네임을 입력해주세요!";
            Invoke("InitLog", 3f);
            return;
        }
        PhotonNetwork.ConnectUsingSettings();
    }

    public void InitLog()
    {
        inGameLog.text = "";
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions(), TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.MasterClient != PhotonNetwork.LocalPlayer)
            GameObject.Find("StartBtn").GetComponent<Button>().interactable = false;

        if (isLoaded)
        {
            PhotonNetwork.LocalPlayer.NickName = InputField.text;
            myName = PhotonNetwork.LocalPlayer.NickName;
            Vector3 pos = new Vector3(Random.Range(-3f, 3f), 0.2f, Random.Range(-3f, 3f));

            if (GameObject.FindGameObjectWithTag("TurnManager") == null)
                PhotonNetwork.Instantiate("TurnManager", new Vector3(0, 0, 0), Quaternion.identity);

            PhotonNetwork.Instantiate("Avatar", pos, Quaternion.identity);

            AddScoreList();

            LoginPanel.SetActive(false);
        }
        else Invoke("OnJoinedLobbyDelay", 1);
    }

    void OnJoinedLobbyDelay()
    {
        isLoaded = true;
        // 포톤 닉네임으로 표시 이름을 넣음 
        PhotonNetwork.LocalPlayer.NickName = InputField.text;
        myName = PhotonNetwork.LocalPlayer.NickName;
        var pos = new Vector3(Random.Range(-3f, 3f), 0.2f, Random.Range(-3f, 3f));

        if (GameObject.FindGameObjectWithTag("TurnManager") == null)
            PhotonNetwork.Instantiate("TurnManager", new Vector3(0, 0, 0), Quaternion.identity);

        PhotonNetwork.Instantiate("Avatar", pos, Quaternion.identity);

        AddScoreList();

        LoginPanel.SetActive(false);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Invoke("AddScoreList", 1);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Invoke("AddScoreList", 1);
    }

    #region 스코어창 

    void AddScoreList()
    {
        foreach (Text t in scoreBoxD)
        {
            t.text = "";
        }

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            Text newScoreT = scoreBoxD[i];
            Text newScoreText = scoreBox[i];
            newScoreText.gameObject.SetActive(true);
            newScoreT.text = $"{PhotonNetwork.PlayerList[i].NickName} : 0 점";

            if (PhotonNetwork.PlayerList[i] == PhotonNetwork.LocalPlayer)
                myPlayerIndex = i;
        }
    }

    #endregion

    public void DisconnectBtn()
    {
        PhotonNetwork.Disconnect();
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            Destroy(player);
        }
    }
}
