using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class Room : MonoBehaviour
{
    [SerializeField] Transform _gridContainer;
    [SerializeField] SortingGroup _sortingGroup;
    public List<Block> GetBlocks => _data._blocks;
    private Dictionary<Vector2Int, Block> _blockPositions;
    private Dictionary<Block, bool> _blockOnDestination;
    private Data _data;
    private PlaceableState _state;
    public System.Action<List<Block>> OnCompleteRoom = delegate { };
    public void Init()
    {
        _data = new Data();
        _blockPositions = new Dictionary<Vector2Int, Block>();
        _blockOnDestination = new Dictionary<Block, bool>();
        foreach (Transform child in _gridContainer)
        {
            Block block = child.GetComponent<Block>();
            if (block != null)
            {
                block.Init(this.transform);
                _data._blocks.Add(block);
                AddBlock(block);
                InitBlockDestionation(block);
                block.OnUnitDestionation += HandleBlockUnitDestionation;
            }
        }
        _data.initPoint = transform.localPosition;
        _state = PlaceableState.Free;
    }

    private void InitBlockDestionation(Block block)
    {
        if (!_blockOnDestination.ContainsKey(block))
        {
            _blockOnDestination.Add(block, false);
        }
    }
    private void HandleBlockUnitDestionation(Block block)
    {
        if (!_blockOnDestination.ContainsKey(block)) return;

        _blockOnDestination[block] = true;

        if(_blockOnDestination.Values.All(x => x == true))
        {
            foreach (var item in _blockOnDestination)
            {
                Block block1 = item.Key;
                block1.gameObject.SetActive(false);
                block1.GetLastOccupier().OnPlaceable(true);
                
            }
            OnCompleteRoom?.Invoke(GetBlocks);
        }
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
        public List<Block> _blocks = new List<Block>();
        public Vector3 initPoint;
        public Data()
        {
            _blocks = new List<Block>();
        }
    }
}
