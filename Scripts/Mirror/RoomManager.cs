using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RoomManager : NetworkRoomManager
{
    public List<GameObject> drawers = new List<GameObject>();
    public NetworkConnectionToClient me;

    public bool isStart = false;

    public override void OnRoomServerConnect(NetworkConnectionToClient conn)
    {
        base.OnRoomServerConnect(conn);
        var lobbyPlayer = Instantiate(spawnPrefabs[0]);
        NetworkServer.Spawn(lobbyPlayer, conn);
        var drawer = Instantiate(spawnPrefabs[1]);
        NetworkServer.Spawn(drawer, conn);
        DontDestroyOnLoad(drawer);
        drawer.SetActive(false);
        drawers.Add(drawer);

        if (GameObject.Find("TurnManager") == null)
        {
            var turnManager = Instantiate(spawnPrefabs[3]);
            NetworkServer.Spawn(turnManager, conn);
        }

        if (drawer.GetComponent<NetworkIdentity>().hasAuthority)
            me = conn;
    }

    public override void OnRoomServerPlayersReady()
    {
        base.OnRoomServerPlayersReady();
        foreach (GameObject d in drawers)
        {
            d.SetActive(true);
        }
        isStart = true;
    }
}
