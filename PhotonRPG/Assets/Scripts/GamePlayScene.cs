using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayScene : MonoBehaviour
{
    void Awake()
    {
        NetworkManager networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        networkManager.Spawn();
    }

    void Update()
    {

    }
}
