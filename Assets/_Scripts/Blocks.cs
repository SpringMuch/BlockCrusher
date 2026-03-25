using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Blocks : MonoBehaviour
{
    [SerializeField] private Block[] blocks;
    [SerializeField] private Board board;
    [SerializeField] private bool isSmartGeneration = false;

    [Space(8.0f)]
    [SerializeField] private GameObject loseGameObject;
    [SerializeField] private GameObject losePanel;

    private int[] polyominoIndexes;

    private int blockCount = 0;

    private void Start()
    {
        var blockWidth = (float)Board.Size / blocks.Length;
        var cellSize = (float)Board.Size / (Block.Size * blocks.Length + blocks.Length + 1);
        for(var i = 0; i < blocks.Length; ++i)
        {
            blocks[i].transform.localPosition = new(blockWidth * (i + 0.5f), -0.25f - cellSize * 4.0f, 0.0f);
            blocks[i].transform.localScale = new(cellSize, cellSize, cellSize);
            blocks[i].Initialize();
        }

        polyominoIndexes =  new int[blocks.Length];

        Generate();
    }

    //private void Generate()
    //{
    //    for(var i = 0; i < blocks.Length; ++i)
    //    {
    //        polyominoIndexes[i] = Random.Range(0, Polyominos.Length);
    //        blocks[i].gameObject.SetActive(true);
    //        blocks[i].Show(polyominoIndexes[i]);
    //        ++blockCount;
    //    }
    //}
    private void Generate()
    {
        for (var i = 0; i < blocks.Length; ++i)
        {
            if (isSmartGeneration)
            {
                List<int> validIndexes = new List<int>();
                for (int p = 0; p < Polyominos.Length; p++)
                {
                    if (board.CheckPlace(p))
                    {
                        validIndexes.Add(p);
                    }
                }

                if (validIndexes.Count > 0)
                {
                    polyominoIndexes[i] = validIndexes[Random.Range(0, validIndexes.Count)];
                }
                else
                {
                    polyominoIndexes[i] = Random.Range(0, Polyominos.Length);
                }
            }
            else
            {
                polyominoIndexes[i] = Random.Range(0, Polyominos.Length);
            }

            blocks[i].gameObject.SetActive(true);
            blocks[i].Show(polyominoIndexes[i]);
            ++blockCount;
        }
    }


    public void Remove()
    {
        --blockCount;
        if(blockCount <= 0)
        {
            blockCount = 0;
            Generate();
        }

        var lose = true;
        for (var i = 0; i < blocks.Length; ++i) 
        {
            if (blocks[i].gameObject.activeSelf == true && board.CheckPlace(polyominoIndexes[i]) == true) 
            {
                lose = false;
                break;
            }
        }
        if( lose == true) 
        {
            Lose();
        }
    }

    public void ResetBlocksSortingOrder()
    {
        for (var i = 0; i < blocks.Length; ++i)
        {
            blocks[i].SetSortingOrder(0);
        }
    }

    private void Lose()
    {
        loseGameObject.SetActive(true);
        SoundManager.Instance.PlaySound(SoundType.Lose);
        losePanel.SetActive(true);
        try
        {
            if (AdLoader.Instance != null)
            {
                AdLoader.Instance.ShowInterstitialAd();
            }
            else
            {
                Debug.LogWarning("AdLoader.Instance đang bị NULL! Game sẽ không hiện quảng cáo nhưng vẫn chạy tiếp bình thường.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Lỗi khi hiển thị quảng cáo: " + e.Message);
        }
    }
}
