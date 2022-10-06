using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomInfo : MonoBehaviour
{
    public NetworkManager networkManager;
    public LobbyUIManager lobbyUIManager;
    public string roomName = "";
    public string hostName = "";
    public string password = "";
    // public string hostID = "";

    public TextMeshProUGUI roomNameText;
    public bool isLocked = true;
    public Toggle lockToggle;

    public TMP_InputField roomNameInput;
    public TMP_InputField passwordInput;
    public Slider maxCount;
    public TextMeshProUGUI maxCountT;

    public Button createBtn;
    public Button joinBtn;

    private void Awake()
    {
        networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        maxCountT = transform.Find("Number").GetComponent<TextMeshProUGUI>();
        lobbyUIManager = GameObject.Find("LobbyManager").GetComponent<LobbyUIManager>();
    }

    private void Update()
    {
        maxCountT.text = maxCount.value.ToString();
    }

    public void SetLocked()
    {
        if (!isLocked)
        {
            passwordInput.interactable = true;
            isLocked = true;
        }
        else
        {
            passwordInput.text = "";
            passwordInput.interactable = false;
            isLocked = false;
        }
    }

    public void CreateRoom()
    {
        if (isLocked)
        {
            if (roomNameInput.text != "" && passwordInput.text != "")
            {
                password = passwordInput.text;
                hostName = networkManager.myPlayFabInfo.DisplayName;
                // lobbyUIManager.CreateRoom(roomNameInput.text, hostName, passwordInput.text, (int)maxCount.value);
                // networkManager.SetRoomInfo(roomNameInput.text, (int)maxCount.value, passwordInput.text);
                networkManager.JoinOrCreateUserRoom(roomNameInput.text, hostName, (int)maxCount.value, passwordInput.text);
            }
        }
        else
        {
            if (roomNameInput.text != "")
            {
                hostName = networkManager.myPlayFabInfo.DisplayName;
                // lobbyUIManager.CreateRoom(roomNameInput.text, hostName, "", (int)maxCount.value);
                // networkManager.SetRoomInfo(roomNameInput.text, (int)maxCount.value);
                networkManager.JoinOrCreateUserRoom(roomNameInput.text, hostName, (int)maxCount.value);
            }
        }
    }

    public void JoinRoom()
    {
        if (isLocked)
        {
            if (password == passwordInput.text)
            {
                networkManager.JoinOrCreateUserRoom(roomNameText.text, hostName, int.Parse(maxCountT.text));
            }
            else
            {
                print("패스워드를 확인하세요");
            }
        }
        else
        {
            networkManager.JoinOrCreateUserRoom(roomNameText.text, hostName, int.Parse(maxCountT.text));
        }
    }
}
