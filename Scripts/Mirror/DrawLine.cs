using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DrawLine : NetworkBehaviour
{
    public Material defaultMaterial; //Material for Line Renderer
    public GameObject linePre;
    public readonly SyncList<GameObject> Lines = new SyncList<GameObject>();

    [SyncVar]
    private GameObject curLine;  //Line which draws now
    // [SyncVar]
    private int positionCount = 2;  //Initial start and end position
    [SyncVar]
    private Vector3 PrevPos = Vector3.zero; // 0,0,0 position variable

    void Awake()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (hasAuthority)
            DrawMouse();
    }

    void DrawMouse()
    {
        Camera cam = Camera.main;
        Vector3 mousePos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.3f));

        if (Input.GetMouseButtonDown(0))
        {
            CmdCreateLine(mousePos);
        }
        else if (Input.GetMouseButton(0))
        {
            CmdConnectLine(mousePos);
        }
    }

    // [ClientRpc]
    [Command] // client --> server 부탁 (host : 2 / client : 2?X)
    void CmdCreateLine(Vector3 mousePos)
    {
        Camera cam = Camera.main;
        // RoomManager roomManager = GameObject.Find("RoomManager").GetComponent<RoomManager>();
        // if (!GetComponent<NetworkIdentity>().hasAuthority)
        // {
        //     GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);
        // }
        // GameObject line = new GameObject("Line");
        var line = Instantiate(linePre, mousePos, Quaternion.identity, cam.transform);
        // var conn = roomManager.me;
        NetworkServer.Spawn(line);
        // line.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);
        // Lines.Add(line);

        RPCDrawLine(line, mousePos);
    }

    [ClientRpc] // server --> client 강제 
    void RPCDrawLine(GameObject line, Vector3 mousePos)
    {
        positionCount = 2;

        LineRenderer lineRend = line.GetComponent<LineRenderer>();

        lineRend.startWidth = 0.01f;
        lineRend.endWidth = 0.01f;
        lineRend.numCornerVertices = 5;
        lineRend.numCapVertices = 5;
        lineRend.material = defaultMaterial;
        lineRend.SetPosition(0, mousePos);
        lineRend.SetPosition(1, mousePos);

        // if (hasAuthority)
        curLine = lineRend.gameObject;
    }


    [Command]
    void CmdConnectLine(Vector3 mousePos)
    {
        if (PrevPos != null && Mathf.Abs(Vector3.Distance(PrevPos, mousePos)) >= 0.001f)
        {
            RpcConnectLine(mousePos);
        }

    }

    [ClientRpc]
    void RpcConnectLine(Vector3 mousePos)
    {
        LineRenderer cLine = curLine.GetComponent<LineRenderer>();
        PrevPos = mousePos;
        positionCount++;
        cLine.positionCount = positionCount;
        cLine.SetPosition(positionCount - 1, mousePos);
    }

    public void DeleteLines()
    {
        for (int i = 0; i < Camera.main.transform.childCount; i++)
        {
            GameObject line = Camera.main.transform.GetChild(i).gameObject;
            CmdClearLines(line);
        }
    }

    [Command]
    public void CmdClearLines(GameObject line)
    {
        RcpClearLines(line);
    }

    // [Command]
    // public void CmdClear()
    // {
    //     Lines.Clear();
    // }

    [ClientRpc]
    public void RcpClearLines(GameObject line)
    {
        Destroy(line);
    }

}
