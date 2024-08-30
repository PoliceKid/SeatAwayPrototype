using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFindingService 
{
    public List<Cell> FindPath(Cell startCell, Cell destinationCell)
    {
        Dictionary<Cell, CellNode> cellNodes = new Dictionary<Cell, CellNode>();
        Queue<CellNode> openList = new Queue<CellNode>();

        CellNode startNode = new CellNode(startCell, 0, Vector3.Distance(startCell.transform.position, destinationCell.transform.position));
        cellNodes.Add(startCell, startNode);
        openList.Enqueue(startNode);

        while (openList.Count > 0)
        {
            CellNode current = openList.Dequeue();

            if (current.cell == destinationCell)
            {
                return ReconstructPath(cellNodes, current);
            }

            foreach (var neighbor in current.cell.GetNeighbors().Values)
            {
                if (neighbor.IsOccupier())
                {
                    continue;
                }

                float newCost = current.cost + Vector3.Distance(current.cell.transform.position, neighbor.transform.position);
                CellNode neighborNode;
                if (!cellNodes.TryGetValue(neighbor, out neighborNode) || newCost < neighborNode.cost)
                {
                    neighborNode = new CellNode(neighbor, newCost, Vector3.Distance(neighbor.transform.position, destinationCell.transform.position));
                    cellNodes[neighbor] = neighborNode;
                    neighborNode.previous = current;
                    openList.Enqueue(neighborNode);
                }
            }
        }

        return new List<Cell>(); // Destination cell is not reachable
    }

    private List<Cell> ReconstructPath(Dictionary<Cell, CellNode> cellNodes, CellNode current)
    {
        List<Cell> path = new List<Cell>();
        while (current != null)
        {
            path.Add(current.cell);
            current = current.previous;
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
}
