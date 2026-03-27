using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

public class AdventureManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Board board;
    [SerializeField] private GameObject winPanel;

    [Header("Challenge Targets")]
    [SerializeField] private GameObject[] challengePrefabs;
    [SerializeField] private TMP_Text[] targetTexts;

    [Header("Random Spawning Range")]
    [SerializeField] private int minTargetCount = 2;
    [SerializeField] private int maxTargetCount = 5;

    private int[] remainingTargets;
    private List<ActiveChallengeBlock> activeChallengeBlocks = new List<ActiveChallengeBlock>();
    private bool isLevelActive = false;

    private class ActiveChallengeBlock
    {
        public GameObject gameObject;
        public int r;
        public int c;
        public int typeIndex;
    }

    private void Start()
    {
        StartNewAdventure();
    }

    private void Update()
    {
        if (isLevelActive)
        {
            CheckBrokenChallenges();

            if (CheckWinCondition())
            {
                CompleteAdventure();
            }
        }
    }

    public void StartNewAdventure()
    {
        board.ClearBoardExplicitly();
        activeChallengeBlocks.Clear();

        int typeCount = challengePrefabs.Length;
        remainingTargets = new int[typeCount];

        for (int i = 0; i < typeCount; i++)
        {
            remainingTargets[i] = Random.Range(minTargetCount, maxTargetCount + 1);
            SpawnChallengeBlocks(i, remainingTargets[i]);
        }

        UpdateUI();
        isLevelActive = true;
    }

    private void SpawnChallengeBlocks(int typeIndex, int count)
    {
        int spawned = 0;
        int maxAttempts = 100;
        int attempts = 0;

        while (spawned < count && attempts < maxAttempts)
        {
            int r = Random.Range(0, Board.Size);
            int c = Random.Range(0, Board.Size);

            if (board.GetCellData(r, c) == 0)
            {
                board.SetCellExplicitly(r, c, 2);

                Vector3 spawnPos = new Vector3(c + 0.5f, r + 0.5f, 0.0f);

                GameObject challengeObj = Instantiate(challengePrefabs[typeIndex], spawnPos, Quaternion.identity, board.transform);

                var sg = challengeObj.GetComponent<SortingGroup>();
                if (sg != null)
                {
                    sg.sortingOrder = 1;
                }
                else
                {
                    var sr = challengeObj.GetComponentInChildren<SpriteRenderer>();
                    if (sr != null) sr.sortingOrder = 2;
                }

                activeChallengeBlocks.Add(new ActiveChallengeBlock
                {
                    gameObject = challengeObj,
                    r = r,
                    c = c,
                    typeIndex = typeIndex
                });

                spawned++;
            }
            attempts++;
        }
    }

    private void CheckBrokenChallenges()
    {
        for (int i = activeChallengeBlocks.Count - 1; i >= 0; i--)
        {
            var block = activeChallengeBlocks[i];

            if (board.GetCellData(block.r, block.c) == 0)
            {
                remainingTargets[block.typeIndex]--;
                if (remainingTargets[block.typeIndex] < 0) remainingTargets[block.typeIndex] = 0;

                Destroy(block.gameObject);
                activeChallengeBlocks.RemoveAt(i);

                UpdateUI();
            }
        }
    }

    private bool CheckWinCondition()
    {
        for (int i = 0; i < remainingTargets.Length; i++)
        {
            if (remainingTargets[i] > 0) return false;
        }
        return true;
    }

    private void CompleteAdventure()
    {
        isLevelActive = false;

        if (winPanel)
        {
            winPanel.SetActive(true);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void UpdateUI()
    {
        for (int i = 0; i < remainingTargets.Length; i++)
        {
            if (i < targetTexts.Length && targetTexts[i] != null)
            {
                targetTexts[i].text = remainingTargets[i].ToString();
            }
        }
    }
}