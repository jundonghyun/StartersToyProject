using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class ResultPanel : MonoBehaviourPunCallbacks
{
    public Text scoreDT;
    int curScore = 0;

    void Start()
    {
        if (!scoreDT.gameObject.activeSelf)
            this.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!scoreDT.gameObject.activeSelf)
        {
            this.gameObject.SetActive(false);
            return;
        }

        int idx = transform.GetSiblingIndex();

        if (scoreDT.text == " : 0 점")
        {
            scoreDT.text = $"{PhotonNetwork.PlayerList[idx].NickName} : 0 점";
        }

        Text score = GetComponentInChildren<Text>();
        score.text = scoreDT.text;

        int dot = score.text.IndexOf(':');
        curScore = int.Parse(score.text.Substring(dot + 1).Replace(" 점", ""));

        if (idx != 0)
        {
            ResultPanel preScore = transform.parent.GetChild(idx - 1).GetComponent<ResultPanel>();
            if (preScore.curScore < curScore)
                transform.SetSiblingIndex(idx - 1);
        }
    }
}
