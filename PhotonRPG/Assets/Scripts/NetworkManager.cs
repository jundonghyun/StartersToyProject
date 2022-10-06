using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.UI;
using TMPro;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public GameObject cafePanel, shopPanel, userHousePanel, lobbyPanel, disconnectPanel;

    [Header("Disconnect")]
    public PlayerLeaderboardEntry myPlayFabInfo;
    public List<PlayerLeaderboardEntry> playFabUserList = new List<PlayerLeaderboardEntry>();
    public TMP_InputField emailInput, passwordInput, usernameInput;

    [Header("Lobby")]
    public TMP_InputField userNickNameInput;
    public TextMeshProUGUI lobbyInfoText, UserNickNameText;
    public GameObject roomsBox;

    [Header("Room")]
    public TMP_InputField setDataInput;
    public GameObject setDataBtnObj;
    public TextMeshProUGUI userHouseDataText, roomNameInfoText, roomNumInfoText;
    public TextMeshProUGUI[] ChatText;
    public TMP_InputField ChatInput;
    public ScrollRect chattingRect;

    [Header("Ready")]
    bool isReadyOn = false;
    public Button[] readyBtns;
    public Color readyOffColor;
    public Color readyOnColor;

    [Header("ETC")]

    bool isLoaded;
    bool isMyRoom = false;

    public LobbyUIManager lobbyUIManager;

    public List<Photon.Realtime.RoomInfo> myList = new List<Photon.Realtime.RoomInfo>();
    public PhotonView PV;
    public Sprite[] monsterSprites;

    #region 플레이팹
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        // 화면 해상도 설정
        Screen.SetResolution(1920, 1080, false);
        // ResetVar();
        // 포톤 동기화 속도 빠르게 설정
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
    }

    public void ResetVar()
    {
        // cafePanel, shopPanel, userHousePanel, lobbyPanel, disconnectPanel
        GameObject canvas = GameObject.Find("Canvas");
        cafePanel = canvas.transform.Find("CafePanel").gameObject;
        lobbyPanel = canvas.transform.Find("LobbyPanel").gameObject;
        userHousePanel = canvas.transform.Find("UserHousePanel").gameObject;
        disconnectPanel = canvas.transform.Find("DisconnectPanel").gameObject;
        // emailInput, passwordInput, usernameInput
        emailInput = canvas.transform.Find("DisconnectPanel/EmailInput").GetComponent<TMP_InputField>();
        passwordInput = canvas.transform.Find("DisconnectPanel/PasswordInput").GetComponent<TMP_InputField>();
        usernameInput = canvas.transform.Find("DisconnectPanel/UserNameInput").GetComponent<TMP_InputField>();
        // userNickNameInput, lobbyInfoText, UserNickNameText, roomsBox;
        userNickNameInput = canvas.transform.Find("LobbyPanel/UserNickNameInput").GetComponent<TMP_InputField>();
        lobbyInfoText = canvas.transform.Find("LobbyPanel/LobbyInfoText").GetComponent<TextMeshProUGUI>();
        UserNickNameText = canvas.transform.Find("Scroll View/Viewport/Content/UserNickNameText").GetComponent<TextMeshProUGUI>();
        roomsBox = canvas.transform.Find("LobbyPanel/Scroll Room/Viewport/Content").gameObject;
        // setDataInput, setDataBtnObj
        setDataInput = canvas.transform.Find("UserHousePanel/ChatBK/SetDataInput").GetComponent<TMP_InputField>();
        setDataBtnObj = canvas.transform.Find("UserHousePanel/ChatBK/SetDataBtn").gameObject;
        // userHouseDataText, roomNameInfoText, roomNumInfoText
        userHouseDataText = canvas.transform.Find("UserHousePanel/ChatBK/BoardBK/UserHouseDataText").GetComponent<TextMeshProUGUI>();
        roomNameInfoText = canvas.transform.Find("RoomNameInfoText").GetComponent<TextMeshProUGUI>();
        roomNumInfoText = canvas.transform.Find("RoomNumberInfoText").GetComponent<TextMeshProUGUI>();
        lobbyUIManager = GameObject.Find("LobbyManager").GetComponent<LobbyUIManager>();
        // ChattingText
        chattingRect = canvas.transform.Find("UserHousePanel/ChatBK/BoardBK/ChattingBox").GetComponent<ScrollRect>();
        GameObject chattingBox = chattingRect.transform.Find("Viewport/Content").gameObject;
        ChatText = chattingBox.GetComponentsInChildren<TextMeshProUGUI>();
        ChatInput = canvas.transform.Find("UserHousePanel/ChatBK/ChattingInput").GetComponent<TMP_InputField>();
        // readyBtns
        readyBtns = lobbyUIManager.readyBtns;
        readyOffColor = readyBtns[0].GetComponent<Image>().color;
        readyOnColor = new Color(1, 0.5f, 0.6f, 1);
    }

    public void Login()
    {
        // 청구서를 작성하여 로그인 
        var request = new LoginWithEmailAddressRequest { Email = emailInput.text, Password = passwordInput.text };
        PlayFabClientAPI.LoginWithEmailAddress(request, (result) =>
            {
                // 리더보드에서 플레이어들 정보 가져오기 
                GetLeaderboard(result.PlayFabId);
                // 포톤 서버에 연결 
                PhotonNetwork.ConnectUsingSettings();
            }
            , (error) => print("로그인 실패"));
    }

    public void Register()
    {
        // 청구서를 작성하여 회원가입 
        var request = new RegisterPlayFabUserRequest { Email = emailInput.text, Password = passwordInput.text, Username = usernameInput.text, DisplayName = usernameInput.text };
        PlayFabClientAPI.RegisterPlayFabUser(request, (result) => { print("회원가입 성공"); SetStat(); SetData("default"); }, (error) => print("회원가입 실패"));
    }

    void SetStat()
    {
        // 플레이어 스탯을 설정
        // IDInfo(string) = 0(int)
        var request = new UpdatePlayerStatisticsRequest { Statistics = new List<StatisticUpdate> { new StatisticUpdate { StatisticName = "IDInfo", Value = 0 } } };
        PlayFabClientAPI.UpdatePlayerStatistics(request, (result) => { }, (error) => print("값 저장실패"));
    }

    void GetLeaderboard(string myID)
    {
        // 리더보드에 플레이어 정보를 저장했다가 가져옴 
        // 재로그인일 수 있기 때문에 초기화 
        playFabUserList.Clear();

        // 100(리더보드 최대) x (0인덱스 ~ 10인덱스의 플레이어) 총 1000명의 
        for (int i = 0; i < 10; i++)
        {
            var request = new GetLeaderboardRequest
            {
                StartPosition = i * 100,
                StatisticName = "IDInfo",
                MaxResultsCount = 100,
                ProfileConstraints = new PlayerProfileViewConstraints() { ShowDisplayName = true }
            };
            PlayFabClientAPI.GetLeaderboard(request, (result) =>
            {
                // 0, 100, 200... 인덱스부터 100개씩 플레이어 정보를 가져오는데 
                // 만약 해당하는 100명의 정보가 0개인 경우 정보 가져오기를 끝냄 
                // (이전 100명까지 해서 모든 플레이어 정보를 가져온 경우)
                if (result.Leaderboard.Count == 0) return;
                for (int j = 0; j < result.Leaderboard.Count; j++)
                {
                    playFabUserList.Add(result.Leaderboard[j]);
                    // 로그인 시에 획득한 id와 대조해서 내 playfabinfo를 가져옴 
                    if (result.Leaderboard[j].PlayFabId == myID) myPlayFabInfo = result.Leaderboard[j];
                }
            },
            (error) => { });
        }
    }

    void SetData(string curData)
    {
        var request = new UpdateUserDataRequest()
        {
            // Home에 현재 입력한 값이 덮어씌워짐 
            Data = new Dictionary<string, string>() { { "Home", curData } },
            // 정보를 가져올 수 있도록 공개를 함 
            Permission = UserDataPermission.Public
        };
        PlayFabClientAPI.UpdateUserData(request, (result) => { }, (error) => print("데이터 저장 실패"));
    }

    void GetData(string curID)
    {
        // 플레이팹 아이디로 어떤 값이든 유저의 데이터를 가져올 수 있음 
        PlayFabClientAPI.GetUserData(new GetUserDataRequest() { PlayFabId = curID }, (result) =>
        // 유저 방 정보창에 미리 아이디와 유저 테이블의 Home의 값을 넣음 
        userHouseDataText.text = curID + "\n" + result.Data["Home"].Value,
        (error) => print("데이터 불러오기 실패"));
    }
    #endregion



    #region 로비
    // 서버 접속 시에 콜백함수로 바로 로비에 접속시킴 (JoinLobby())
    public override void OnConnectedToMaster() => PhotonNetwork.JoinLobby();

    // 전체 플레이어 - 방에 있는 플레이어 = 로비 플레이어 수 
    void Update() => lobbyInfoText.text = "Lobby : " + (PhotonNetwork.CountOfPlayers - PhotonNetwork.CountOfPlayersInRooms) + " / Connect : " + PhotonNetwork.CountOfPlayers;

    // 로비에 join하면 바로 콜백되는 함수 
    public override void OnJoinedLobby()
    {
        // 방에서 로비로 올 땐 딜레이없고, 로그인해서 로비로 올 땐 PlayFabUserList가 채워질 시간동안 1초 딜레이
        // isLoaded은 로그인을 해서 처음 로비로 오는지 확인하는 값 
        if (isLoaded)
        {
            ShowPanel(lobbyPanel);
            ShowUserNickName();
        }
        // 유저 정보들을 채워올 때까지 대기시간 1초 
        else Invoke("OnJoinedLobbyDelay", 1);
    }

    void OnJoinedLobbyDelay()
    {
        isLoaded = true;
        // 포톤 닉네임으로 표시 이름을 넣음 
        PhotonNetwork.LocalPlayer.NickName = myPlayFabInfo.DisplayName;
        ShowPanel(lobbyPanel);
        ShowUserNickName();
    }

    void ShowPanel(GameObject CurPanel)
    {
        lobbyPanel.SetActive(false);
        cafePanel.SetActive(false);
        shopPanel.SetActive(false);
        userHousePanel.SetActive(false);
        disconnectPanel.SetActive(false);

        CurPanel.SetActive(true);
    }

    void ShowUserNickName()
    {
        // 스크롤뷰에 있는 유저 닉네임들을 모두 지우고 (초기화)
        UserNickNameText.text = "";
        // 플레이팹의 유저 리스트에서 모든 유저의 닉네임을 가져와 넣음 
        for (int i = 0; i < playFabUserList.Count; i++) UserNickNameText.text += playFabUserList[i].DisplayName + "\n";
    }

    public void XBtn()
    {
        // 로비에 있으면, 포톤 서버 종료 / 방이면, 방 나가기 --> 로비 
        if (PhotonNetwork.InLobby) PhotonNetwork.Disconnect();
        else if (PhotonNetwork.InRoom) PhotonNetwork.LeaveRoom();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        isLoaded = false;
        ShowPanel(disconnectPanel);
    }

    public void RenewalRoomList()
    {
        lobbyUIManager.ClearRoom();
        for (int i = 0; i < myList.Count; i++)
        {
            print(myList[i].CustomProperties["HostNickName"].ToString());
            print(myList[i].CustomProperties["Password"].ToString());
            lobbyUIManager.CreateRoom(myList[i].Name, myList[i].CustomProperties["HostNickName"].ToString(), myList[i].CustomProperties["Password"].ToString(), myList[i].MaxPlayers);
        }
    }

    // 변동사항이 있는 방들만 가져옴 
    public override void OnRoomListUpdate(List<Photon.Realtime.RoomInfo> roomList)
    {
        int roomCount = roomList.Count;
        for (int i = 0; i < roomCount; i++)
        {
            if (!roomList[i].RemovedFromList)
            {
                if (!myList.Contains(roomList[i])) myList.Add(roomList[i]);
                else myList[myList.IndexOf(roomList[i])] = roomList[i];
            }
            else if (myList.IndexOf(roomList[i]) != -1) myList.RemoveAt(myList.IndexOf(roomList[i]));
        }
        RenewalRoomList();
    }
    #endregion



    #region 방
    public void JoinOrCreateRoom(string roomName)
    {
        // 유저 방 참여 호출 
        if (roomName == "userRoom")
        {
            //PlayFabUserList의 표시이름과 입력받은 닉네임이 같다면 PlayFabID를 커스텀 프로퍼티로 넣고 방을 만든다
            for (int i = 0; i < playFabUserList.Count; i++)
            {
                // 가져온 모든 플레이어와 해당 방 이름을 대조해서 일치하면, 해당 방으로 입장 
                if (playFabUserList[i].DisplayName == userNickNameInput.text)
                {
                    RoomOptions roomOptions = new RoomOptions();
                    // 최대 인원 수 조정 
                    roomOptions.MaxPlayers = 25;
                    // 방에 해시태그를 담 
                    // 기존 유니티의 hashtable과 겹치므로 위에 using hashtable을 써줌 
                    // 현재 방의 커스텀 테이블을 작성 - "PlayFabID"를 key로, 실제 playFabId를 value로 넣음 
                    roomOptions.CustomRoomProperties = new Hashtable() { { "PlayFabID", playFabUserList[i].PlayFabId } };
                    PhotonNetwork.JoinOrCreateRoom(userNickNameInput.text + "'s Room", roomOptions, null);
                    return;
                }
            }
            print("닉네임이 일치하지 않습니다");
        }
        // 카페, 상점 방 참여 호출 
        else PhotonNetwork.JoinOrCreateRoom(roomName, new RoomOptions() { MaxPlayers = 25 }, null);
    }

    public void JoinOrCreateUserRoom(string roomName, string hostName, int max, string password = "")
    {
        // 유저 방 참여 호출 
        //PlayFabUserList의 표시이름과 입력받은 닉네임이 같다면 PlayFabID를 커스텀 프로퍼티로 넣고 방을 만든다
        for (int i = 0; i < playFabUserList.Count; i++)
        {
            // 가져온 모든 플레이어와 해당 방 이름을 대조해서 일치하면, 해당 방으로 입장 
            if (playFabUserList[i].DisplayName == hostName)
            {
                RoomOptions roomOptions = new RoomOptions();
                // 최대 인원 수 조정 
                roomOptions.MaxPlayers = (byte)max;
                // 방에 해시태그를 담 
                // 기존 유니티의 hashtable과 겹치므로 위에 using hashtable을 써줌 
                // 현재 방의 커스텀 테이블을 작성 - "PlayFabID"를 key로, 실제 playFabId를 value로 넣음 
                roomOptions.CustomRoomProperties = new Hashtable() { { "PlayFabID", playFabUserList[i].PlayFabId }, { "Password", password }, { "HostNickName", playFabUserList[i].DisplayName } };
                roomOptions.CustomRoomPropertiesForLobby = new string[] { "PlayFabID", "Password", "HostNickName" };

                Transform charsBox = userHousePanel.transform.Find("inner/Chars");
                Transform readyBox = userHousePanel.transform.Find("inner/ReadyBox");
                for (int j = 0; j < charsBox.childCount; j++)
                {
                    GameObject player = charsBox.GetChild(j).gameObject;
                    readyBox.GetChild(j).gameObject.SetActive(false);
                    player.SetActive(false);
                }

                PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, null);

                return;
            }
        }
        print("일치하는 방이 없습니다");
    }

    public override void OnCreateRoomFailed(short returnCode, string message) => print("방만들기실패");

    public override void OnJoinRoomFailed(short returnCode, string message) => print("방참가실패");

    // 방 참여 시에 콜백으로 실행 
    public override void OnJoinedRoom()
    {
        // 들어오면 바로 방 갱신 
        RoomRenewal();
        ReadyRenewal();
        // PV.RPC("ResetChar", RpcTarget.All);

        // 어떤 방인지 포톤에서 현재 방 이름을 가져와서 작성 
        string curName = PhotonNetwork.CurrentRoom.Name;
        roomNameInfoText.text = curName;
        if (curName == "cafe 01" || curName == "cafe 02") ShowPanel(cafePanel);
        else if (curName == "shop 01" || curName == "shop 02") ShowPanel(shopPanel);
        //유저방이면 데이터 가져오기
        else
        {
            ShowPanel(userHousePanel);

            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                GameObject player = GameObject.Find("Chars").transform.GetChild(i).gameObject;
                GameObject.Find("ReadyBox").transform.GetChild(i).gameObject.SetActive(true);
                player.SetActive(true);
            }

            // 현재 방의 커스텀 테이블에서 PlayFabId를 가져와서 플레이팹에서 데이터를 가져옴 
            string curID = PhotonNetwork.CurrentRoom.CustomProperties["PlayFabID"].ToString();
            GetData(curID);

            // 현재 방 PlayFabID 커스텀 프로퍼티가 나의 PlayFabID와 같다면 값을 저장할 수 있음
            if (curID == myPlayFabInfo.PlayFabId)
            {
                // 내 방이면, 방 이름에 내 방이라는 표시를 추가로 넣음 
                roomNameInfoText.text += " (My Room)";

                // 내 방이면, Home의 값을 변경할 수 있도록 입력 칸과 버튼 표시 
                setDataInput.gameObject.SetActive(true);
                setDataBtnObj.SetActive(true);
            }
        }

        userHousePanel.transform.Find("SelectPanel").gameObject.SetActive(true);

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "IsAdmin", "Admin" } });

            Hashtable playerCP = PhotonNetwork.LocalPlayer.CustomProperties;

            print(playerCP["IsAdmin"]);
        }
        else
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "IsAdmin", "NotAdmin" } });
        }

        SelectBtnOnOff();

        ChatInput.text = "";
        for (int i = 0; i < ChatText.Length; i++) ChatText[i].text = "";
    }

    // 다른 플레이어가 들어오거나 나갈 때에도 방을 갱신 
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        RoomRenewal();
        ReadyRenewal();
        SelectBtnOnOff();
        PV.RPC("ChatRPC", RpcTarget.All, $"<color=yellow>{newPlayer.NickName} has entered.</color>");
    }

    void SelectBtnOnOff()
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i] == PhotonNetwork.LocalPlayer)
            {
                GameObject player = GameObject.Find("Chars").transform.GetChild(i).gameObject;
                GameObject ready = GameObject.Find("ReadyBox").transform.GetChild(i).gameObject;
                player.transform.GetComponent<Button>().interactable = true;
                ready.transform.GetComponent<Button>().interactable = true;
            }
            else
            {
                GameObject player = GameObject.Find("Chars").transform.GetChild(i).gameObject;
                GameObject ready = GameObject.Find("ReadyBox").transform.GetChild(i).gameObject;
                player.transform.GetComponent<Button>().interactable = false;
                ready.transform.GetComponent<Button>().interactable = false;
            }
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // if (PhotonNetwork.CurrentRoom.CustomProperties["PlayFabID"].ToString() == otherPlayer.UserId)
        RoomRenewal();
        ReadyRenewal();
        SelectBtnOnOff();
        PV.RPC("ResetChar", RpcTarget.All);
        PV.RPC("ChatRPC", RpcTarget.All, $"<color=yellow>{otherPlayer.NickName} has left.</color>");
    }

    void RoomRenewal()
    {
        // 로비에서는 유저 닉네임으로 채워져 있었기 때문에 지워줌 
        UserNickNameText.text = "";
        // 포톤 플레이어리스트의 닉네임들을 엔터로 구분해 써넣음 
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            UserNickNameText.text += PhotonNetwork.PlayerList[i].NickName + "\n";
        // 방 정보에 포톤 서버에서 방 인원 수를 가져와 작성 
        roomNumInfoText.text = PhotonNetwork.CurrentRoom.PlayerCount + " / " + PhotonNetwork.CurrentRoom.MaxPlayers + "maximum";
        // lobbyUIManager.DeleteRoom();
    }

    public override void OnLeftRoom()
    {
        PV.RPC("ResetChar", RpcTarget.All);
        setDataInput.gameObject.SetActive(false);
        setDataBtnObj.SetActive(false);
        // lobbyUIManager.DeleteRoom();

        setDataInput.text = "";
        userNickNameInput.text = "";
        userHouseDataText.text = "";
    }

    public void SetDataBtn()
    {
        // 자기자신의 방에서만 값 저장이 가능하고, 값 저장 후 1초(서버를 기다리고) 뒤에 값 불러오기
        SetData(setDataInput.text);
        Invoke("SetDataBtnDelay", 1);
    }

    // getdata를 통해 다시 데이터를 가져와서 방 정보창에 새로운 텍스트를 갱신 
    void SetDataBtnDelay() => GetData(PhotonNetwork.CurrentRoom.CustomProperties["PlayFabID"].ToString());
    #endregion

    #region 채팅
    public void Send()
    {
        PV.RPC("ChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + ChatInput.text);
        ChatInput.text = "";
    }

    public void SelectChar(int monsterNum)
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i] == PhotonNetwork.LocalPlayer)
            {
                PhotonNetwork.PlayerList[i].SetCustomProperties(new Hashtable { { "Char", monsterNum } });
                PV.RPC("SetChar", RpcTarget.AllBuffered, i, monsterNum);
                break;
            }
        }
    }

    [PunRPC]
    void SetChar(int i, int monsterNum)
    {
        GameObject player = GameObject.Find("Chars").transform.GetChild(i).gameObject;
        GameObject.Find("ReadyBox").transform.GetChild(i).gameObject.SetActive(true);
        player.SetActive(true);
        player.transform.GetChild(0).GetComponent<Image>().sprite = monsterSprites[monsterNum];
    }

    [PunRPC]
    void ResetChar()
    {
        Transform charsBox = userHousePanel.transform.Find("inner/Chars");
        Transform readyBox = userHousePanel.transform.Find("inner/ReadyBox");
        for (int i = 0; i < charsBox.childCount; i++)
        {
            GameObject player = charsBox.GetChild(i).gameObject;
            readyBox.GetChild(i).gameObject.SetActive(false);
            player.SetActive(false);
        }
        for (int j = 0; j < PhotonNetwork.PlayerList.Length; j++)
        {
            GameObject player = charsBox.GetChild(j).gameObject;
            readyBox.GetChild(j).gameObject.SetActive(true);
            player.SetActive(true);

            int monsterN = int.Parse(PhotonNetwork.PlayerList[j].CustomProperties["Char"].ToString());
            player.transform.GetChild(0).GetComponent<Image>().sprite = monsterSprites[monsterN];
        }
    }

    [PunRPC] // RPC는 플레이어가 속해있는 방 모든 인원에게 전달한다
    void ChatRPC(string msg)
    {
        bool isInput = false;
        for (int i = 0; i < ChatText.Length; i++)
            if (ChatText[i].text == "")
            {
                isInput = true;
                ChatText[i].text = msg;
                break;
            }
        if (!isInput) // 꽉차면 한칸씩 위로 올림
        {
            for (int i = 1; i < ChatText.Length; i++) ChatText[i - 1].text = ChatText[i].text;
            ChatText[ChatText.Length - 1].text = msg;
        }
        chattingRect.verticalNormalizedPosition = 0;
    }
    #endregion

    #region 레디 
    public void ClickReady()
    {
        if (!isReadyOn)
        {
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                if (PhotonNetwork.PlayerList[i] == PhotonNetwork.LocalPlayer)
                {
                    isReadyOn = true;
                    PV.RPC("Ready", RpcTarget.All, i);
                    break;
                }
            }
        }
        else
        {
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                if (PhotonNetwork.PlayerList[i] == PhotonNetwork.LocalPlayer)
                {
                    isReadyOn = false;
                    PV.RPC("ReadyOff", RpcTarget.All, i);
                    break;
                }
            }
        }
    }

    [PunRPC]
    void Ready(int n)
    {
        userHousePanel.GetComponent<RoomManager>().isReady[n] = true;
        readyBtns[n].GetComponent<Image>().color = readyOnColor;
    }

    [PunRPC]
    void ReadyOff(int n)
    {
        userHousePanel.GetComponent<RoomManager>().isReady[n] = false;
        readyBtns[n].GetComponent<Image>().color = readyOffColor;
    }

    void ReadyRenewal()
    {
        isReadyOn = false;
        userHousePanel.GetComponent<RoomManager>().isReady = new bool[PhotonNetwork.PlayerList.Length];
        for (int j = 0; j < PhotonNetwork.PlayerList.Length; j++)
        {
            userHousePanel.GetComponent<RoomManager>().isReady[j] = false;
            readyBtns[j].GetComponent<Image>().color = readyOffColor;
        }
    }
    #endregion

    #region 게임

    public void Spawn()
    {
        GameObject playerObj = PhotonNetwork.Instantiate("Player", new Vector3(Random.Range(-20f, 20f), 10, Random.Range(-20f, 20f)), Quaternion.identity);
        playerObj.name = "Player";
        playerObj.GetComponent<PlayerScript>().playerName = myPlayFabInfo.DisplayName;
        // playerObj.GetComponent<PlayerScript>().charN = int.Parse(PhotonNetwork.LocalPlayer.CustomProperties["Char"].ToString());
        // PV.RPC("SetPlayer", RpcTarget.AllBuffered, playerObj, , );
    }

    // [PunRPC]
    // public void SetPlayer(GameObject playerObj, string playerName, int charN)
    // {
    //     playerObj.GetComponent<PlayerScript>().playerName = playerName;
    //     playerObj.GetComponent<PlayerScript>().charN = charN;
    //     for (int i = 0; i < playerObj.transform.childCount; i++)
    //     {
    //         if (i == charN)
    //             playerObj.transform.GetChild(i).gameObject.SetActive(true);
    //         else
    //             playerObj.transform.GetChild(i).gameObject.SetActive(false);
    //     }
    // }

    #endregion
}
