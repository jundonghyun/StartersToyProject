using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Weapon : MonoBehaviourPunCallbacks
{
    public enum Type { Melee, Range };
    public Type type;
    public int damage;
    public float rate;
    public BoxCollider meleeArea;
    public TrailRenderer trailEffect;
    GameObject inputWord;
    TurnManager turnManager;
    public PlayerMove pm;

    bool isBusser = false;

    void Awake()
    {
        turnManager = GameObject.FindGameObjectWithTag("TurnManager").GetComponent<TurnManager>();
        inputWord = turnManager.inputAnswer.gameObject;
    }

    public void Use()
    {
        if (type == Type.Melee)
        {
            StopCoroutine("Swing");
            StartCoroutine("Swing");
        }
    }

    IEnumerator Swing()
    {
        yield return new WaitForSeconds(0.1f);
        meleeArea.enabled = true;
        trailEffect.enabled = true;

        yield return new WaitForSeconds(0.3f);
        meleeArea.enabled = false;

        yield return new WaitForSeconds(0.3f);
        trailEffect.enabled = false;
    }

    #region busser

    // private void OnTriggerStay(Collider other)
    // {
    //     if (other.tag == "Busser" && !isBusser)
    //     {
    //         if (pm.gameObject.name == PhotonNetwork.LocalPlayer.NickName)
    //             inputWord.SetActive(true);
    //         pm.photonView.RPC("Busser", RpcTarget.All);
    //         StartCoroutine(BackToStartPoint(pm));
    //         isBusser = true;
    //     }
    // }

    // public IEnumerator BackToStartPoint(PlayerMove pmObj)
    // {
    //     // pm.SetTime(0f);
    //     // yield return new WaitForSecondsRealtime(5f);
    //     yield return new WaitForSeconds(5f);
    //     // pm.SetTime(1f);
    //     inputWord.SetActive(false);
    //     pmObj.gameObject.transform.position = pmObj.startPos;
    //     turnManager.isStop = false;
    //     isBusser = false;
    // }

    #endregion

}
