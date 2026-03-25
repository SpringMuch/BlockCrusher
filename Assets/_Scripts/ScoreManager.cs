using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private Board board;

    [SerializeField] private TMP_Text currentScoreText;
    [SerializeField] private TMP_Text bestScoreText;

    private void Update()
    {
        if (board == null) return;

        currentScoreText.text = board.CurrentScore.ToString();
        bestScoreText.text = board.BestScore.ToString();
    }
}
