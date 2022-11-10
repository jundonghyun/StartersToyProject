using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomBtn : MonoBehaviour
{
    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => CreateRoom());
    }

    public void CreateRoom()
    {
        NetworkManager.instance.OnClickCreate();
    }

    void Update()
    {

    }
}
