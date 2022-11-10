using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class Room : MonoBehaviourPunCallbacks
{
    [SerializeField] Text roomName;
    [SerializeField] InputField roomPasswordInput;
    [SerializeField] Button roomJoinBtn;

    void Awake()
    {
        roomJoinBtn.onClick.AddListener(() =>
        {
            NetworkManager.instance.OnClickJoinRoom(roomName.text, roomPasswordInput.text);
        });
    }

    public void RoomInit(string name)
    {
        roomName.text = name;
    }

    void Update()
    {

    }
}
