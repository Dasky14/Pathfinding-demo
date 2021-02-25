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
    private Node startNode;
    private Node endNode;



    private List<Node> openList;
    private List<Node> closedList;




    public void FindPathButtonPress()
    {
        Initialise();

        if (pathLine == null)
        {
            Debug.LogWarning("Path line renderer not found!", this);
            return;
        }

        Node[] path = FindPath();

        if (path == null)
        {
            Debug.Log("No path found!", this);
            return;
        }

        pathLine.positionCount = path.Length;
        for (int i = 0; i < path.Length; i++)
        {
            Node n = path[i];
            pathLine.SetPosition(i, n.point.GetLocalPosition() + new Vector2(0.5f, 0.5f));
        }
    }

    private void Initialise()
    {
        nodeGrid = new Node[GridManager.Instance.size, GridManager.Instance.size];
        openList = new List<Node>();
        closedList = new List<Node>();

        List<GridPoint> points = GridManager.Instance.GridList;

        foreach (var point in points)
        {
            Node n = CreateNode(point);

            switch (point.state)
            {
                case GridState.Difficult:
                    n.cost *= 3;
                    break;
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







    private Node[] FindPath()
    {
        openList.Clear();
        closedList.Clear();

        var start = startNode;
        var end = endNode;

        start.heuristic = (endNode.Position - start.Position).magnitude;
        openList.Add(start);


        while (openList.Count > 0)
        {
            var bestNode = GetBestCell();
            openList.Remove(bestNode);

            var neighbours = GetAdjacentNodes(bestNode);
            for (int i = 0; i < neighbours.Count; i++)
            {
                var curNode = neighbours[i];

                if (curNode == null)
                    continue;
                if (curNode == end)
                {
                    curNode.parentNode = bestNode;
                    return ConstructPath(curNode);
                }

                var g = bestNode.cost + (curNode.Position - bestNode.Position).magnitude;
                var h = (end.Position - curNode.Position).magnitude;

                if (openList.Contains(curNode)/* && curNode.F < (g + h)*/)
                    continue;
                if (closedList.Contains(curNode)/* && curNode.F < (g + h)*/)
                    continue;

                curNode.cost = g;
                curNode.heuristic = h;
                curNode.parentNode = bestNode;

                if (!openList.Contains(curNode))
                    openList.Add(curNode);
            }

            if (!closedList.Contains(bestNode))
                closedList.Add(bestNode);
        }

        return null;
    }

    private Node GetBestCell()
    {
        Node result = null;
        float currentF = float.PositiveInfinity;

        for (int i = 0; i < openList.Count; i++)
        {
            var cell = openList[i];

            if (cell.F < currentF)
            {
                currentF = cell.F;
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

        int row = n.point.x;
        int col = n.point.y;

        if (row + 1 < gridRows && nodeGrid[col, row + 1].IsWalkable)
        {
            temp.Add(nodeGrid[col, row + 1]);
        }
        if (row - 1 >= 0 && nodeGrid[col, row - 1].IsWalkable)
        {
            temp.Add(nodeGrid[col, row - 1]);
        }
        if (col - 1 >= 0 && nodeGrid[col - 1, row].IsWalkable)
        {
            temp.Add(nodeGrid[col - 1, row]);
        }
        if (col + 1 < gridCols && nodeGrid[col + 1, row].IsWalkable)
        {
            temp.Add(nodeGrid[col + 1, row]);
        }

        return temp;
    }


    private Node CreateNode(GridPoint point)
    {
        Node newNode = new Node();

        newNode.point = point;

        return newNode;
    }

    private class Node
    {
        public GridPoint point;

        public Vector2 Position
        {
            get => point.Point;
        }
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

        public float distanceToTarget = -1f;
        public float cost = 1f;
        public float heuristic = 1f;
        public float F
        {
            get
            {
                return cost + heuristic;
            }
        }

        public Node parentNode = null;
    }

    public enum NodeState { Untested, Open, Closed }
}