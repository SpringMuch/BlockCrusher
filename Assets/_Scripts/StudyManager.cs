using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class StudyManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Board board;
    [SerializeField] private int initialBlockCount = 10;
    [SerializeField] private GameObject winPanel;

    [Header("UI Reference")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text moveCountText;

    [Header("UI Result (Win Panel)")]
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text finalMoveText;
    [SerializeField] private TMP_Text finalTimeText;

    private float elapsedTime = 0f;
    private int moveCount = 0;
    private bool isLevelActive = false;

    private void Start()
    {
        StartNewChallenge();
    }

    private void Update()
    {
        if (isLevelActive)
        {
            elapsedTime += Time.deltaTime;
            UpdateUI();

            if (CheckBoardCleared())
            {
                CompleteChallenge();
            }
        }
    }

    public void StartNewChallenge()
    {
        moveCount = 0;
        elapsedTime = 0f;

        board.ClearBoardExplicitly();

        SpawnRandomBlocks();

        isLevelActive = true;
    }

    private void SpawnRandomBlocks()
    {
        int spawned = 0;
        int maxAttempts = 100;
        int attempts = 0;

        while (spawned < initialBlockCount && attempts < maxAttempts)
        {
            int r = Random.Range(0, Board.Size);
            int c = Random.Range(0, Board.Size);

            if (board.GetCellData(r, c) == 0)
            {
                board.SetCellExplicitly(r, c, 2);
                spawned++;
            }
            attempts++;
        }
    }

    private bool CheckBoardCleared()
    {
        for (int r = 0; r < Board.Size; r++)
        {
            for (int c = 0; c < Board.Size; c++)
            {
                if (board.GetCellData(r, c) != 0) return false;
            }
        }
        return true;
    }

    private void CompleteChallenge()
    {
        isLevelActive = false;

        if(winPanel)
        {
            winPanel.SetActive(true);
            if (finalScoreText) finalScoreText.text = $"Score: {board.CurrentScore}";
            if (finalMoveText) finalMoveText.text = $"Moves: {moveCount}";
            if (finalTimeText) finalTimeText.text = $"Time: {Mathf.FloorToInt(elapsedTime)}s";
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void UpdateUI()
    {
        if (timerText) timerText.text = $"Time: {Mathf.FloorToInt(elapsedTime)}s";
        if (moveCountText) moveCountText.text = $"{moveCount}";
    }

    public void OnBlockPlaced()
    {
        moveCount++;
    }
}