using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public bool[] isReady;

    public GameObject systemPanel;
    public TextMeshProUGUI systemText;

    private bool isSystemOn = false;

    void Awake()
    {
        isSystemOn = false;
    }

    void Update()
    {
        if (PhotonNetwork.PlayerList.Length >= 1)
        {
            int readyCount = 0;
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                if (isReady[i])
                    readyCount++;
            }

            if (PhotonNetwork.PlayerList.Length > readyCount)
            {
                if (isSystemOn)
                    isSystemOn = false;
                systemPanel.SetActive(false);
                StopAllCoroutines();
                PhotonNetwork.CurrentRoom.IsOpen = true;
            }
            else if (isSystemOn)
                return;
            else if (PhotonNetwork.PlayerList.Length == readyCount)
            {
                systemText.text = "The game starts in 3 seconds.";
                systemPanel.SetActive(true);
                isSystemOn = true;
                PhotonNetwork.CurrentRoom.IsOpen = false;
            }
        }
        else
        {
            if (isSystemOn)
                isSystemOn = false;
            systemPanel.SetActive(false);
            StopAllCoroutines();
            PhotonNetwork.CurrentRoom.IsOpen = true;
        }
    }
}
