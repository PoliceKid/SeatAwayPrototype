using Injection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Architecture : MonoBehaviour
{
    [SerializeField] Transform _gridSlotContainer;
    private Data _data;
    public List<Cell> GetCells => _data._cells;
    private Dictionary<Vector2Int, Cell> _cellPositions;

    public void Init()
    {
        _data = new Data();
        _cellPositions = new Dictionary<Vector2Int, Cell>();

        foreach (Transform child in _gridSlotContainer)
        {
            Cell cell = child.GetComponent<Cell>();
            if(cell != null && cell.gameObject.activeInHierarchy)
            {
                cell.Init();
                _data._cells.Add(cell);
                AddCell(cell);
            }
        }     
    }
    #region CELL API
    public void AddCell(Cell cell)
    {
        int x = (int)cell.transform.localPosition.x;
        int z = (int)cell.transform.localPosition.z;
        Vector2Int position = new Vector2Int(x, z);
        if (!_cellPositions.ContainsKey(position))
        {
            _cellPositions[position] = cell;
        }
    }

    public Cell GetCell(int x, int z)
    {
        Vector2Int position = new Vector2Int(x, z);
        if (_cellPositions.ContainsKey(position))
        {
            return _cellPositions[position];
        }
        return null;
    }
    #endregion
    public void Dispose()
    {

    }
    [System.Serializable]
    public class Data
    {
        public List<Cell> _cells;
        public Data()
        {
            _cells = new List<Cell>();
        }
    }
}
