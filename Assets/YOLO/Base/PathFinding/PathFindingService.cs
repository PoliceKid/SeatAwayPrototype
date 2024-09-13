using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PathFindingService 
{
    public List<Cell> FindPath(Cell startCell, Cell destinationCell)
    {
        
        Dictionary<Cell, CellNode> cellNodes = new Dictionary<Cell, CellNode>();
        SortedSet<CellNode> openList = new SortedSet<CellNode>(new CellNodeComparer());

        CellNode startNode = new CellNode(startCell, 0, Vector3.Distance(startCell.transform.localPosition, destinationCell.transform.localPosition));
        cellNodes.Add(startCell, startNode);
        openList.Add(startNode);

        while (openList.Count > 0)
        {           
            CellNode current = openList.Min;
            openList.Remove(current);

            if (current.cell == destinationCell)
            {
                return ReconstructPath(startNode, current);
            }
            
            foreach (var neighborValueKey in current.cell.GetNeighbors())
            {
                Vector3 dir = neighborValueKey.Key;
                Cell neighbor = neighborValueKey.Value;
                if (neighbor.IsOccupier())
                {
                    if (neighbor != destinationCell)
                    {
                        continue;
                    }
                    if (neighbor == destinationCell && dir == neighbor.GetLastOccupier().GetDirection()) continue;
                }
                float newCost = current.cost + Vector3.Distance(current.cell.transform.localPosition, neighbor.transform.localPosition);
                if (!cellNodes.TryGetValue(neighbor, out CellNode neighborNode) || newCost < neighborNode.cost)
                {
                    neighborNode = new CellNode(neighbor, newCost, Vector3.Distance(neighbor.transform.localPosition, destinationCell.transform.localPosition));
                    cellNodes[neighbor] = neighborNode;
                    neighborNode.previous = current;

                    if (!openList.Contains(neighborNode))
                    {
                        openList.Add(neighborNode);
                    }
                }
            }
        }

        return new List<Cell>(); // Destination cell is not reachable
    }

    private List<Cell> ReconstructPath(CellNode startNode, CellNode destinationNode)
    {
        List<Cell> path = new List<Cell>();
        CellNode currentNode = destinationNode;

        while (currentNode != null)
        {
            path.Add(currentNode.cell);
            currentNode = currentNode.previous;
        }
        if (!path.Contains(startNode.cell))
        {
            path.Add(startNode.cell);
        }
        path.Reverse();
        return path;
    }

    private class CellNode : System.IComparable<CellNode>
    {
        public Cell cell;
        public float cost;
        public float heuristicCost;
        public CellNode previous;

        public CellNode(Cell cell, float cost, float heuristicCost)
        {
            this.cell = cell;
            this.cost = cost;
            this.heuristicCost = heuristicCost;
        }

        public int CompareTo(CellNode other)
        {
            float fCost = cost + heuristicCost;
            float otherFCost = other.cost + other.heuristicCost;
            return fCost.CompareTo(otherFCost);
        }
    }
    private class CellNodeComparer : IComparer<CellNode>
    {
        public int Compare(CellNode x, CellNode y)
        {
            float xFCost = x.cost + x.heuristicCost;
            float yFCost = y.cost + y.heuristicCost;

            if (xFCost == yFCost)
            {
                return x.heuristicCost.CompareTo(y.heuristicCost);
            }
            return xFCost.CompareTo(yFCost);
        }
    }
}
