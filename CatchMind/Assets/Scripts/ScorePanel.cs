using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScorePanel : MonoBehaviour
{
    public Color[] scoreColors;
    Text[] scores;

    void Awake()
    {
        scores = GetComponentsInChildren<Text>();
        scoreColors = new Color[scores.Length];
        for (int i = 0; i < scores.Length; i++)
            scoreColors[i] = scores[i].color;
    }

    void Update()
    {
        scores = GetComponentsInChildren<Text>();

        for (int i = 0; i < scoreColors.Length; i++)
            scores[i].color = scoreColors[i];
    }
}
