using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class Board : MonoBehaviour
{
    public const int Size = 8;
    private const string BestScoreKey = "best_score";
    [SerializeField] private Cell cellPrefab;
    [SerializeField] private Transform cellsTransform;
    [SerializeField] private StudyManager studyManager;

    [Space(8.0f)]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text bestScoreText;

    private readonly Cell[,] cells = new Cell[Size, Size];
    private readonly int[,] data = new int[Size, Size];

    private readonly List<Vector2Int> hoverPoints = new();
    private readonly List<int> highlightPolyominoColumns = new();
    private readonly List<int> highlightPolyominoRows = new();
    private readonly List<int> fullLineColumns = new();
    private readonly List<int> fullLineRows = new();

    private Vector2Int previousHoverPoint;
    private readonly List<Vector2Int> previousHoverPoints = new();

    private int score;
    private int bestScore;

    public int CurrentScore => score;
    public int BestScore => bestScore;
    public int GetCellData(int r, int c) => data[r, c];

    private void Start()
    {
        for (var r = 0; r < Size; ++r)
        {
            for (var c = 0; c < Size; ++c)
            {
                cells[r, c] = Instantiate(cellPrefab, cellsTransform);
                cells[r, c].transform.position = new(c + 0.5f, r + 0.5f, 0.0f);
                cells[r, c].Hide();
            }
        }
        score = 0;
        bestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        scoreText.text = score.ToString();
        bestScoreText.text = bestScore.ToString();
    }

    public void Hover(Vector2Int point, int polyominoIndex) 
    { 
        var polyomino = Polyominos.Get(polyominoIndex);
        var polyominoRows = polyomino.GetLength(0);
        var polyominoColumns = polyomino.GetLength(1);

        UnHover();
        Unhighlight();

        highlightPolyominoColumns.Clear();
        highlightPolyominoRows.Clear();

        HoverPoints(point, polyominoRows, polyominoColumns, polyomino);
        if (hoverPoints.Count > 0)
        {
            previousHoverPoint = point;
            previousHoverPoints.Clear();
            previousHoverPoints.AddRange(hoverPoints);

            Hover();
            Highlight(point, polyominoColumns, polyominoRows);
        }
        else if(previousHoverPoints.Count > 0 && Mathf.Abs(point.x - previousHoverPoint.x) < 2 && Mathf.Abs(point.y - previousHoverPoint.y) < 2)
        {
            point = previousHoverPoint;
            hoverPoints.Clear();
            hoverPoints.AddRange(previousHoverPoints);

            Hover();
            Highlight(point, polyominoColumns, polyominoRows);
        }
        else
        {
            previousHoverPoints.Clear();
        }
    }

    private void HoverPoints(Vector2Int point, int polyominoRows, int polyominoColumns, int[,] polyomino)
    {
        for(var r = 0; r < polyominoRows; ++r)
        {
            for (var c = 0; c < polyominoColumns; ++c)
            {
                if (polyomino[r,c] > 0) 
                {
                    var hoverPoint = point + new Vector2Int(c, r);
                    if (IsValidPoint(hoverPoint) == false) 
                    {
                        hoverPoints.Clear();
                        return;
                    }

                    hoverPoints.Add(hoverPoint);
                }
            }
        }
    }

    private bool IsValidPoint(Vector2Int point)
    {
        if (point.x < 0 || Size <= point.x) return false;
        if (point.y < 0 || Size <= point.y) return false;
        if (data[point.y, point.x] > 0) return false;

        return true;
    }

    private void Hover() 
    {
        foreach(var hoverPoint in hoverPoints)
        {
            data[hoverPoint.y, hoverPoint.x] = 1;
            cells[hoverPoint.y, hoverPoint.x].Hover();
        }
    }

    private void UnHover() 
    {
        foreach (var hoverPoint in hoverPoints)
        {
            data[hoverPoint.y, hoverPoint.x] = 0;
            cells[hoverPoint.y, hoverPoint.x].Hide();
        }
        hoverPoints.Clear();
    }

    public bool Place(Vector2Int point, int polyominoIndex)
    {
        var polyomino = Polyominos.Get(polyominoIndex);
        var polyominoRows = polyomino.GetLength(0);
        var polyominoColumns = polyomino.GetLength(1);

        UnHover();
        HoverPoints(point, polyominoRows, polyominoColumns, polyomino);
        if (hoverPoints.Count > 0)
        {
            Place(point, polyominoColumns, polyominoRows);
            AddScore(10);
            if (studyManager != null)
            {
                studyManager.OnBlockPlaced();
            }
            SoundManager.Instance.PlaySound(SoundType.Place);

            previousHoverPoints.Clear();
            return true;
        }
        else if(previousHoverPoints.Count > 0 && Mathf.Abs(point.x - previousHoverPoint.x) < 2 && Mathf.Abs(point.y - previousHoverPoint.y) < 2)
        {
            point = previousHoverPoint;
            hoverPoints.Clear();
            hoverPoints.AddRange(previousHoverPoints);

            Place(point, polyominoColumns, polyominoRows);

            previousHoverPoints.Clear();
            return true;
        }

        previousHoverPoints.Clear();
        return false;
    }

    private void Place(Vector2Int point, int polyominoColumns, int polyominoRows)
    {
        foreach (var hoverPoint in hoverPoints)
        {
            data[hoverPoint.y, hoverPoint.x] = 2;
            cells[hoverPoint.y, hoverPoint.x].Normal();
        }
        ClearFullLines(point, polyominoColumns, polyominoRows);

        hoverPoints.Clear();
    }

    private void ClearFullLines(Vector2Int point, int polyminoColumns, int polyominoRows)
    {
        FullLineColumns(point.x, point.x + polyminoColumns);
        FullLineRows(point.y, point.y + polyominoRows);

        int totalCleared = (fullLineColumns.Count * Size) + (fullLineRows.Count * Size);

        AddScore(fullLineColumns.Count * Size * 10 + fullLineRows.Count * Size * 10);

        ClearFullLineColumns();
        ClearFullLineRows();
        SoundManager.Instance.PlaySound(SoundType.FullBreak);
    }

    private void FullLineColumns(int fromColumns, int toColumnExclusive)
    {
        fullLineColumns.Clear();
        for( var c = fromColumns; c < toColumnExclusive; ++c)
        {
            var isFullLine = true;
            for( var r = 0; r < Size; ++r)
            {
                if (data[r, c] != 2)
                {
                    isFullLine = false;
                    break;
                }
            }
            if(isFullLine == true) 
            {
                fullLineColumns.Add(c);
            }
        }
    }

    private void FullLineRows(int fromRow, int toRowExclusive)
    {
        fullLineRows.Clear();
        for (var r = fromRow; r < toRowExclusive; ++r)
        {
            var isFullLine = true;
            for (var c = 0; c < Size; ++c)
            {
                if (data[r, c] != 2)
                {
                    isFullLine = false;
                    break;
                }
            }
            if (isFullLine == true)
            {
                fullLineRows.Add(r);
            }
        }
    }

    private void ClearFullLineColumns()
    {
        float waveSpeed = 0.05f;

        foreach ( var c in fullLineColumns)
        {
            for (var r = 0; r < Size; ++r)
            {
                data[r, c] = 0;
                float delay = Mathf.Abs(r - (Size / 2f - 0.5f)) * waveSpeed;
                cells[r, c].PlayBreakAnimation(delay);
            }
        }
        SoundManager.Instance.PlaySound(SoundType.Break);
    }

    private void ClearFullLineRows()
    {
        float waveSpeed = 0.05f;

        foreach (var r in fullLineRows)
        {
            for (var c = 0; c < Size; ++c)
            {
                data[r, c] = 0;
                float delay = Mathf.Abs(c - (Size / 2f - 0.5f)) * waveSpeed;
                cells[r, c].PlayBreakAnimation(delay);
            }
        }
        SoundManager.Instance.PlaySound(SoundType.Break);
    }

    private void Highlight(Vector2Int point, int polyominoColumns, int polyominoRows)
    {
        PredictFullLineColumns(point.x, point.x + polyominoColumns);
        PredictFullLineRows(point.y, point.y + polyominoRows);

        HighlightFullLineColumns();
        HighlightFullLineRows();

        foreach (var fullLineColumn in fullLineColumns)
        {
            highlightPolyominoColumns.Add(fullLineColumn - point.x);
        }
        foreach (var fullLineRow in fullLineRows)
        {
            highlightPolyominoRows.Add(fullLineRow - point.y);
        }
    }
    private void Unhighlight()
    {
        UnhighlightFullLineColumns();
        UnhighlightFullLineRows();
    }

    private void PredictFullLineColumns(int fromColumns, int toColumnExclusive)
    {
        fullLineColumns.Clear();
        for (var c = fromColumns; c < toColumnExclusive; ++c)
        {
            var isFullLine = true;
            for (var r = 0; r < Size; ++r)
            {
                if (data[r, c] != 1 && data[r, c] != 2)
                {
                    isFullLine = false;
                    break;
                }
            }
            if (isFullLine == true)
            {
                fullLineColumns.Add(c);
            }
        }
    }

    private void PredictFullLineRows(int fromRow, int toRowExclusive)
    {
        fullLineRows.Clear();
        for (var r = fromRow; r < toRowExclusive; ++r)
        {
            var isFullLine = true;
            for (var c = 0; c < Size; ++c)
            {
                if (data[r, c] != 1 && data[r, c] != 2)
                {
                    isFullLine = false;
                    break;
                }
            }
            if (isFullLine == true)
            {
                fullLineRows.Add(r);
            }
        }
    }

    private void HighlightFullLineColumns()
    {
        foreach (var c in fullLineColumns)
        {
            for (var r = 0; r < Size; ++r)
            {
                if(data[r, c] == 2)
                {
                    cells[r, c].Highlight();
                }
            }
        }
    }

    private void HighlightFullLineRows()
    {
        foreach (var r in fullLineRows)
        {
            for (var c = 0; c < Size; ++c)
            {
                if (data[r, c] == 2)
                {
                    cells[r, c].Highlight();
                }
            }
        }
    }

    private void UnhighlightFullLineColumns()
    {
        foreach (var c in fullLineColumns)
        {
            for (var r = 0; r < Size; ++r)
            {
                if (data[r, c] == 2)
                {
                    cells[r, c].Normal();
                }
            }
        }
    }

    private void UnhighlightFullLineRows()
    {
        foreach (var r in fullLineRows)
        {
            for (var c = 0; c < Size; ++c)
            {
                if (data[r, c] == 2)
                {
                    cells[r, c].Normal();
                }
            }
        }
    }

    public bool CheckPlace(int polyominoIndex) 
    {
        var polyomino = Polyominos.Get(polyominoIndex);
        var polyominoRows = polyomino.GetLength(0);
        var polyominoColumns = polyomino.GetLength(1);

        for( var r = 0; r <= Size - polyominoRows; ++r) 
        { 
            for( var c = 0; c <= Size - polyominoColumns; ++c) 
            {
                if( CheckPlace(c, r, polyominoColumns, polyominoRows, polyomino) == true)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool CheckPlace(int column, int row, int polyominoColumns, int polyominoRows, int[,] polyomino)
    {
        for (var r = 0; r < polyominoRows; ++r)
        {
            for (var c = 0; c < polyominoColumns; ++c)
            {
                if (polyomino[r, c] > 0 && data[row + r, column + c] == 2)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void SetCellExplicitly(int r, int c, int value)
    {
        data[r, c] = value;
        if (value == 2) cells[r, c].Normal();
        else cells[r, c].Hide();
    }

    public void ClearBoardExplicitly()
    {
        for (int r = 0; r < Size; r++)
        {
            for (int c = 0; c < Size; c++)
            {
                data[r, c] = 0;
                cells[r, c].Hide();
            }
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
        if(score >= bestScore) 
        {
            bestScore = score;
            PlayerPrefs.SetInt(BestScoreKey, bestScore);
        }

        scoreText.text = score.ToString();
        bestScoreText.text = bestScore.ToString();
    }

    public List<int> HighlightPolyominoColumns => highlightPolyominoColumns;
    public List<int> HighlightPolyominoRows => highlightPolyominoRows;
}
