using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerList : MonoBehaviour
{
    private static PlayerList Instance;
    public static PlayerList instance { get { return Instance; } }

    public List<Player> players = new List<Player>();

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {

    }
}
