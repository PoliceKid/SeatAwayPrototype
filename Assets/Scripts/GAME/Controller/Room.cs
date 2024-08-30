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
    private Dictionary<Vector2Int, Block> _blockPositions;
    private Data _data;
    private PlaceableState _state;
    public void Init()
    {
        _data = new Data();
        _blockPositions = new Dictionary<Vector2Int, Block>();
        foreach (Transform child in _gridContainer)
        {
            Block block = child.GetComponent<Block>();
            if (block != null)
            {
                block.Init(this.transform);
                _data._grids.Add(block);
                AddBlock(block);
            }
        }
        _data.initPoint = transform.localPosition;
        _state = PlaceableState.Free;
    }
    public void Dispose()
    {

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
    public void AddBlock(Block block)
    {
        int x = (int)block.transform.localPosition.x;
        int z = (int)block.transform.localPosition.z;
        Vector2Int position = new Vector2Int(x, z);
        if (!_blockPositions.ContainsKey(position))
        {
            _blockPositions[position] = block;
        }
    }

    public Block GetBlock(int x, int z)
    {
        Vector2Int position = new Vector2Int(x, z);
        if (_blockPositions.ContainsKey(position))
        {
            return _blockPositions[position];
        }
        return null;
    }
    #region PLACEABLE STATE
    public PlaceableState GetPlaceableState => _state;
    public void ChangePlaceableState(PlaceableState newState)
    {
        _state = newState;
    }
    #endregion
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
