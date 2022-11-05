using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class DrawLine : MonoBehaviourPunCallbacks
{
    public Material defaultMaterial; //Material for Line Renderer
    private GameObject curLine;  //Line which draws now
    // [SyncVar]
    private int positionCount = 2;  //Initial start and end position
    private Vector3 PrevPos = Vector3.zero; // 0,0,0 position variable
    PhotonView pv;
    // PlayerMove pm;

    public Camera drawCam;
    public Camera playerCam;

    public GameObject linePre;

    void Awake()
    {
        pv = GetComponent<PhotonView>();
        drawCam = GameObject.Find("DrawCam").GetComponent<Camera>();
        playerCam = GameObject.Find("PlayerCam").GetComponent<Camera>();
        // pm = GetComponent<PlayerMove>();
    }

    void Update()
    {
        if (pv.IsMine)
            DrawMouse();
    }

    void DrawMouse()
    {
        // Camera cam = Camera.main;
        Vector3 mousePos = drawCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1.3f));

        if (Input.GetMouseButtonDown(0))
        {
            pv.RPC("RpcCreateLine", RpcTarget.All, mousePos);
        }
        else if (Input.GetMouseButton(0))
        {
            pv.RPC("CmdConnectLine", RpcTarget.All, mousePos);
        }
    }

    [PunRPC]
    void RpcCreateLine(Vector3 mousePos)
    {
        // Camera cam = Camera.main;

        GameObject line = Instantiate(linePre, mousePos, Quaternion.identity, drawCam.transform);

        positionCount = 2;

        LineRenderer lineRend = line.GetComponent<LineRenderer>();

        lineRend.startWidth = 0.01f;
        lineRend.endWidth = 0.01f;
        lineRend.numCornerVertices = 5;
        lineRend.numCapVertices = 5;
        lineRend.material = defaultMaterial;
        lineRend.SetPosition(1, mousePos);
        lineRend.SetPosition(0, mousePos);

        // if (hasAuthority)
        curLine = line;
    }



    [PunRPC]
    void CmdConnectLine(Vector3 mousePos)
    {
        if (PrevPos != null && Mathf.Abs(Vector3.Distance(PrevPos, mousePos)) >= 0.001f)
        {
            LineRenderer cLine = curLine.GetComponent<LineRenderer>();
            PrevPos = mousePos;
            positionCount++;
            cLine.positionCount = positionCount;
            cLine.SetPosition(positionCount - 1, mousePos);
        }

    }

    public void ClearLine()
    {
        // pv.RPC("DeleteLines", RpcTarget.All);
        for (int i = 0; i < drawCam.transform.childCount; i++)
        {
            GameObject line = drawCam.transform.GetChild(i).gameObject;
            Destroy(line);
        }
    }

    [PunRPC]
    public void DeleteLines()
    {
        for (int i = 0; i < Camera.main.transform.childCount; i++)
        {
            GameObject line = Camera.main.transform.GetChild(i).gameObject;
            Destroy(line);
        }
    }
}
