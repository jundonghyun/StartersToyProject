using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    private static NetworkManager Instance;
    public static NetworkManager instance
    {
        get { return Instance; }
    }
    private List<RoomInfo> rooms = new List<RoomInfo>();
    public string nickName = "";

    [SerializeField] Vector3 respawnPos = new Vector3(0, 0, 0);
    [SerializeField] GameObject roomPrefab;

    void Awake()
    {
        if (Instance == null) Instance = this;

        DontDestroyOnLoad(this.gameObject);

        PhotonNetwork.ConnectUsingSettings();
    }

    public void OnClickStart()
    {
        nickName = GameObject.Find("NickNameInput").GetComponent<InputField>().text;

        PhotonNetwork.LoadLevel(1);
    }

    public void OnClickCreate()
    {
        string password = GameObject.Find("PassWordInput").GetComponent<InputField>().text;
        if (nickName == "")
        {
            PhotonNetwork.Disconnect();
            return;
        }
        else
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = 20;
            roomOptions.CustomRoomProperties = new Hashtable() { { "roomName", nickName + "'s room" }, { "password", password } };
            // 방이름, 비밀번호 값을 로비에서도 받을 수 있도록 함 
            string[] newPropertiesForLobby = new string[2];
            newPropertiesForLobby[0] = "roomName";
            newPropertiesForLobby[1] = "password";
            roomOptions.CustomRoomPropertiesForLobby = newPropertiesForLobby;
            PhotonNetwork.CreateRoom(nickName + "'s room", roomOptions);
        }
    }

    public void OnClickJoinRoom(string roomName, string password)
    {
        nickName = GameObject.Find("NickNameInput").GetComponent<InputField>().text;

        foreach (RoomInfo room in rooms)
        {
            Debug.Log($"roomName : {roomName} / {(string)room.CustomProperties["roomName"]}");
            if ((string)room.CustomProperties["roomName"] == roomName)
            {
                Debug.Log($"roomName : {password} / {(string)room.CustomProperties["password"]}");
                if (password == (string)room.CustomProperties["password"])
                {
                    // PhotonNetwork.LocalPlayer.NickName = nickName;
                    PhotonNetwork.LoadLevel(1);
                    PhotonNetwork.JoinRoom(roomName);
                    return;
                }
                else break;
            }
        }
        Debug.Log("Can't enter " + roomName + " room");
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {

    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LocalPlayer.NickName = nickName;
        GameObject.Find("PasswordPanel").SetActive(false);
        var player = PhotonNetwork.Instantiate("Player", respawnPos, Quaternion.identity);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Transform roomBox = GameObject.Find("RoomList").transform;

        foreach (RoomInfo room in roomList)
        {
            if (rooms.Contains(room)) return;
            if (room.RemovedFromList)
            {
                Destroy(roomBox.Find((string)room.CustomProperties["roomName"]));
                rooms.Remove(room);
                return;
            }

            rooms.Add(room);
            GameObject newRoom = Instantiate(roomPrefab, Vector3.zero, Quaternion.identity);
            newRoom.transform.parent = roomBox;
            newRoom.GetComponent<Room>().RoomInit((string)room.CustomProperties["roomName"]);
            newRoom.name = (string)room.CustomProperties["roomName"];
        }
    }

    void Update()
    {

    }
}
