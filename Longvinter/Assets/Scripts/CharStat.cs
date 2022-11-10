using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CharStat : MonoBehaviourPunCallbacks
{
    public static CharStat localPlayer;
    public string nickname;

    void Awake()
    {
        if (photonView.IsMine)
        {
            localPlayer = this;
            photonView.RPC("SetName", RpcTarget.AllBuffered, NetworkManager.instance.nickName);
        }
    }

    [PunRPC]
    void SetName(string name)
    {
        nickname = name;

        PlayerList.instance.players.Add(PhotonNetwork.PlayerList[PhotonNetwork.PlayerList.Length - 1]);
    }

    void Update()
    {

    }
}
