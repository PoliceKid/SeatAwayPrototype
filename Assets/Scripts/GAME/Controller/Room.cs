using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Room : MonoBehaviour
{
    [SerializeField] Transform _gridContainer;
    [SerializeField] SortingGroup _sortingGroup; 
    public List<Block> GetBlocks => _data._grids;
    private Data _data;
    public void Init()
    {
        _data = new Data();
        _data.initPoint = transform.localPosition;
        foreach (Transform child in _gridContainer)
        {
            Block grid = child.GetComponent<Block>();
            if (grid != null)
            {
                grid.Init(this.transform);
                _data._grids.Add(grid);
            }
        }

    }
    internal void OnMove(bool onMove)
    {
        _sortingGroup.sortingOrder = onMove ? 10 : 2;
    }

    internal void Move(Vector3 worldPosition)
    {
        transform.position = worldPosition;
    }
    public void ResetPosition()
    {
        transform.localPosition = _data.initPoint;
    }
    [SerializeField]
    public class Data
    {
        public List<Block> _grids = new List<Block>();
        public Vector3 initPoint;
        public Data()
        {
            _grids = new List<Block>();
        }
    }
}
