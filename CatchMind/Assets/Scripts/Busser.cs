using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Busser : MonoBehaviourPunCallbacks
{
    PhotonView pv;
    TurnManager turnManager;
    public bool isBusser = false;

    void Awake()
    {
        pv = GetComponent<PhotonView>();
        turnManager = GameObject.FindGameObjectWithTag("TurnManager").GetComponent<TurnManager>();
    }

    void Update()
    {
        if (transform.position.y < -15f)
            transform.position = GameObject.Find("GoalPoint").transform.position;
    }
}
