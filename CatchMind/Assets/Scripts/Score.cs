using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Score : MonoBehaviourPunCallbacks
{
    public Text scoreDT;
    int curScore = 0;

    void Awake()
    {

    }

    void Update()
    {
        int idx = transform.GetSiblingIndex();

        if (scoreDT.text == " : 0 점")
        {
            scoreDT.text = $"{PhotonNetwork.PlayerList[idx].NickName} : 0 점";
        }

        Text score = GetComponent<Text>();
        score.text = scoreDT.text;

        int dot = score.text.IndexOf(':');
        curScore = int.Parse(score.text.Substring(dot + 1).Replace(" 점", ""));

        if (idx != 0)
        {
            Score preScore = transform.parent.GetChild(idx - 1).GetComponent<Score>();
            if (preScore.curScore < curScore)
                transform.SetSiblingIndex(idx - 1);
        }
    }
}
