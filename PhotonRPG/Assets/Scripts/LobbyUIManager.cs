using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class LobbyUIManager : MonoBehaviour
{
    public GameObject roomPrefab;
    public NetworkManager networkManager;
    public GameObject contentBox;
    public List<GameObject> rooms = new List<GameObject>();
    public Button[] readyBtns;

    private void Awake()
    {
        networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        networkManager.ResetVar();
    }

    public void CreateRoom(string roomName, string hostName, string password, int max)
    {
        GameObject room = Instantiate(roomPrefab);
        room.transform.parent = contentBox.transform;
        RoomInfo info = room.GetComponent<RoomInfo>();
        info.roomName = roomName;
        info.roomNameText.text = roomName;
        info.hostName = hostName;
        info.password = password;
        info.maxCount.value = max;
        info.maxCount.gameObject.SetActive(false);
        info.roomNameInput.gameObject.SetActive(false);
        info.roomNameText.gameObject.SetActive(true);
        info.createBtn.gameObject.SetActive(false);
        info.joinBtn.gameObject.SetActive(true);
        info.lockToggle.gameObject.SetActive(false);

        if (password == "")
        {
            info.isLocked = false;
            info.passwordInput.interactable = false;
        }
        else
        {
            info.isLocked = true;
            info.passwordInput.interactable = true;
        }

        info.lockToggle.gameObject.SetActive(false);

        rooms.Add(info.gameObject);
    }

    public void ClearRoom()
    {
        if (rooms.Count != 0)
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                Destroy(rooms[i]);
            }
        }
    }
}
