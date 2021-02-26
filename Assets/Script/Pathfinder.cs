using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{
    public static Pathfinder Instance;

    public LineRenderer pathLine = null;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Debug.LogWarning("Too many pathfinders.", this);
            Destroy(gameObject);
        }
    }

    private Node[,] nodeGrid;
    public Node startNode;
    public Node endNode;

    private List<Node> openList;
    private List<Node> closedList;
    private Node[] currentPath;
    private Node currentNode;

    private bool isRunning = false;

    private void OnDrawGizmos()
    {
        if (currentPath != null && true)
        {
            Gizmos.color = Color.cyan;
            foreach (var node in openList)
            {
                Gizmos.DrawCube(new Vector3(node.Position.x + 0.5f, node.Position.y + 0.5f, -0.5f), Vector3.one * 0.3f);
            }
            foreach (var node in closedList)
            {
                Gizmos.DrawCube(new Vector3(node.Position.x + 0.5f, node.Position.y + 0.5f, -0.5f), Vector3.one * 0.3f);
            }

            Gizmos.color = Color.black;
            foreach (Node step in currentPath)
            {
                Gizmos.DrawCube(new Vector3(step.Position.x + 0.5f, step.Position.y + 0.5f, -1f), Vector3.one * 0.3f);
            }
        }
    }

    public void FindPathButtonPress()
    {
        if (isRunning)
            return;

        Initialise();

        if (pathLine == null)
        {
            Debug.LogWarning("Path line renderer not found!", this);
            return;
        }

        StartCoroutine(FindPath());
    }

    public void ClearPath()
    {
        nodeGrid = new Node[GridManager.Instance.size, GridManager.Instance.size];
        openList = new List<Node>();
        closedList = new List<Node>();
        currentPath = null;
        currentNode = null;
        isRunning = false;
    } 

    private void Initialise()
    {
        ClearPath();

        List<GridPoint> points = GridManager.Instance.GridList;

        foreach (var point in points)
        {
            Node n = CreateNode(point);

            switch (point.state)
            {
                case GridState.Start:
                    startNode = n;
                    break;
                case GridState.End:
                    endNode = n;
                    break;
            }

            nodeGrid[point.x, point.y] = n;
        }
    }

    private IEnumerator FindPath()
    {
        isRunning = true;
        yield return null;

        openList.Clear();

        var start = startNode;
        var end = endNode;

        start.g = 0f;
        start.f = start.H;
        openList.Add(start);


        while (openList.Count > 0)
        {
            

            currentNode = GetBestNode();
            openList.Remove(currentNode);
            closedList.Add(currentNode);

            currentPath = ConstructPath(currentNode);
            yield return new WaitForSeconds(0.05f);
            if (!isRunning)
                yield break;

            if (currentNode.Equals(end))
            {
                currentPath = ConstructPath(end);
                isRunning = false;
                yield break;
            }

            var neighbours = GetAdjacentNodes(currentNode);
            foreach (var neighbour in neighbours)
            {
                float t_g = currentNode.g + GridDistance(currentNode, neighbour);
                if (neighbour.point.state == GridState.Difficult) t_g += 1;
                if (t_g < neighbour.g)
                {
                    neighbour.parentNode = currentNode;
                    neighbour.g = t_g;
                    neighbour.f = neighbour.g + neighbour.H;
                    if (!openList.Contains(neighbour))
                        openList.Add(neighbour);
                }
            }
        }

        isRunning = false;
    }

    private Node GetBestNode()
    {
        Node result = null;
        float currentF = float.PositiveInfinity;

        for (int i = 0; i < openList.Count; i++)
        {
            var cell = openList[i];

            if (cell.f < currentF)
            {
                currentF = cell.f;
                result = cell;
            }
        }

        return result;
    }

    private Node[] ConstructPath(Node destination)
    {
        var path = new List<Node>() { destination };

        var current = destination;
        while (current.parentNode != null)
        {
            current = current.parentNode;
            path.Add(current);
        }

        path.Reverse();
        return path.ToArray();
    }

    private List<Node> GetAdjacentNodes(Node n)
    {
        int gridRows = GridManager.Instance.size;
        int gridCols = GridManager.Instance.size;

        List<Node> temp = new List<Node>();

        int col = n.point.x;
        int row = n.point.y;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (col + x < 0 || col + x >= gridCols || row + y < 0 || row + y >= gridRows)
                    continue;

                Node c = nodeGrid[col + x, row + y];
                if (c.IsWalkable)
                    temp.Add(c);
            }
        }


        foreach (var neighbour in temp)
        {
            if (GridDistance(n, neighbour) > 1)
            {
                Debug.LogWarning("Wtf");
            }
        }

        return temp;
    }

    public static float GridDistance(Node n1, Node n2)
    {
        return Mathf.Max(Mathf.Abs(n1.Position.x - n2.Position.x), Mathf.Abs(n1.Position.y - n2.Position.y));
    }


    private Node CreateNode(GridPoint point)
    {
        Node newNode = new Node();

        newNode.point = point;

        return newNode;
    }

    public class Node
    {
        public GridPoint point;

        public Vector2 Position => point.Point;
        public bool IsWalkable
        {
            get
            {
                return point.state == GridState.Free ||
                       point.state == GridState.Difficult ||
                       point.state == GridState.Start ||
                       point.state == GridState.End;
            }
        }

        public float g = float.PositiveInfinity;

        public float H => GridDistance(this, Instance.endNode);

        public float f = float.PositiveInfinity;

        public Node parentNode = null;

        public bool Equals(Node n)
        {
            return (point.x == n.point.x && point.y == n.point.y);
        }
    }

    public enum NodeState { Untested, Open, Closed }
}