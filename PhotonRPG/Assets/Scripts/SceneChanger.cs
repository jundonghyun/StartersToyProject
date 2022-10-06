using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    void Awake()
    {

    }

    void OnEnable()
    {
        StartCoroutine(PlayGame());
    }

    IEnumerator PlayGame()
    {
        TextMeshProUGUI text = transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        if (this.gameObject.activeSelf)
        {
            yield return new WaitForSeconds(1f);
            text.text = "The game starts in 2 seconds.";
            yield return new WaitForSeconds(1f);
            text.text = "The game starts in 1 seconds.";
            yield return new WaitForSeconds(1f);
            SceneManager.LoadScene("Game");
        }
    }

    void Update()
    {

    }
}
