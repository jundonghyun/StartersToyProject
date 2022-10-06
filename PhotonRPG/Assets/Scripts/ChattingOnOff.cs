using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChattingOnOff : MonoBehaviour
{
    public GameObject chattingBox;

    public void ChattingOnOffBtn()
    {
        if (chattingBox.activeSelf)
            chattingBox.SetActive(false);
        else
            chattingBox.SetActive(true);
    }
}
