using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    public Dictionary<(int, int), GridPoint> grid { get; private set; }
    public List<GridPoint> gridList => grid.Values.ToList();

    public Camera mainCamera;
    public int size = 20;
    public float noiseScale = 5f;
    public GameObject tilePrefab;

    private List<SpriteRenderer> tilePool;
    private Transform poolContainer;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Debug.LogWarning("Too many Grid Managers!", this);
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        transform.localScale = Vector3.one;

        poolContainer = new GameObject("PoolContainer").transform;
        tilePool = new List<SpriteRenderer>();
        for (int i = 0; i < size*size; i++)
            CreateNewTile();

        GenerateGrid();
        RefreshCamera();
        MoveTiles();
    }

    private GameObject CreateNewTile()
    {
        GameObject newTile = Instantiate(tilePrefab, poolContainer);
        newTile.name = "Tile_" + tilePool.Count.ToString();
        tilePool.Add(newTile.GetComponent<SpriteRenderer>());
        return newTile;
    }

    public void GenerateGrid()
    {
        if (size < 10)
            size = 10;

        Vector2 noiseOffset = new Vector2(Random.Range(0f, 1000f), Random.Range(0f, 1000f));
        int safetyBorder = Mathf.RoundToInt(size / 15f);

        grid = new Dictionary<(int, int), GridPoint>();
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                GridState state;
                if (x == 0 && y == 0)
                    state = GridState.Start;
                else if (x == size - 1 && y == size - 1)
                    state = GridState.End;
                else if ((x <= safetyBorder && y <= safetyBorder) || (x >= size - (1 + safetyBorder) && y >= size - (1 + safetyBorder)))
                    state = GridState.Free;
                else
                {
                    float val = Mathf.PerlinNoise((float)x / size * noiseScale + noiseOffset.x, (float)y / size * noiseScale + noiseOffset.y);
                    state = val <= 0.3f ? GridState.Blocked : 
                            val <= 0.475f ? GridState.Difficult : 
                            GridState.Free;
                }

                grid.Add((x, y), new GridPoint(x, y, state));
            }
        }
    }

    public void RefreshCamera()
    {
        Vector3 newPos = new Vector3(0f, 0f, -10f);
        newPos.x = transform.position.x + size / 2;
        newPos.y = transform.position.y + size / 2;
        mainCamera.transform.position = newPos;
        mainCamera.orthographicSize = size / 2 + size * 0.075f;
    }

    public void MoveTiles()
    {
        int count = 0;
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if (count >= tilePool.Count)
                    CreateNewTile();
                
                if (grid.TryGetValue((x, y), out GridPoint point))
                {
                    tilePool[count].gameObject.SetActive(true);
                    tilePool[count].transform.position = point.GetWorldPosition();
                    tilePool[count].color = point.GetColor();
                    count++;
                }
            }
        }

        for (int i = count; i < size*size; i++)
        {
            tilePool[i].gameObject.SetActive(false);
        }
    }

    public void RandomiseButton() 
    {
        GenerateGrid();
        RefreshCamera();
        MoveTiles();
    }
}

public struct GridPoint
{
    public readonly int x;
    public readonly int y;
    public readonly GridState state;

    public Vector2 Point
    {
        get => new Vector2(x, y);
    }

    public GridPoint(int x, int y, GridState state)
    {
        this.x = x;
        this.y = y;
        this.state = state;
    }

    public Vector2 GetWorldPosition()
    {
        Vector2 pos = new Vector2(x, y);
        pos.x += GridManager.Instance.transform.position.x;
        pos.y += GridManager.Instance.transform.position.y;
        return pos;
    }

    public Vector2 GetLocalPosition()
    {
        return new Vector2(x, y);
    }

    public Color GetColor()
    {
        switch (state)
        {
            case GridState.Free:
                return Color.white;
            case GridState.Difficult:
                return Color.grey;
            case GridState.Blocked:
                return Color.black;
            case GridState.Start:
                return Color.green;
            case GridState.End:
                return Color.red;
            default:
                return Color.magenta;
        }
    }
}

public enum GridState
{
    Free,
    Difficult,
    Blocked,
    Start,
    End
}
