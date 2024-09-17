using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class Room : MonoBehaviour
{
    #region PROPERTIES
    [SerializeField] float _weight;
    [SerializeField] Transform _BlockContainer;
    [SerializeField] SortingGroup _sortingGroup; 
    [SerializeField] private bool _canMoveableState;
    #endregion

    #region CURRENT DATA
    private Data _data;
    private Dictionary<Vector2Int, Block> _blockPositions;
    private Dictionary<Block, bool> _blockOnUnitDestination;
    private Dictionary<Block, bool> _blockOnUnitDestinationsBlockRaycast;

    public System.Action<Room> OnCompleteRoom = delegate { };
    private PlaceableState _state;
    public List<Block> GetBlocks => _data.Blocks;
    public float GetWeight => _weight;
    public int GetBlockCount => _BlockContainer.childCount;
    #endregion
    public void Init()
    {
        _data = new Data();
        _blockPositions = new Dictionary<Vector2Int, Block>();
        _blockOnUnitDestination = new Dictionary<Block, bool>();
        _blockOnUnitDestinationsBlockRaycast = new Dictionary<Block, bool>();
        foreach (Transform child in _BlockContainer)
        {
            Block block = child.GetComponent<Block>();
            if (block != null)
            {
                _data.Blocks.Add(block);
                AddBlock(block);
                InitBlockDestionation(block);
                block.OnUnitDestionation += HandleBlockUnitDestionation;
                block.OnUnitStartOccpier += HandleBlockStartOccupier;
            }
        }
        _data.InitPoint = transform.localPosition;
        _state = PlaceableState.Free;
        
    }

    private void HandleBlockStartOccupier(Block block)
    {
        _blockOnUnitDestinationsBlockRaycast.Add(block, false);
        ChangePlaceableState(PlaceableState.Freeze);
    }

    private void InitBlockDestionation(Block block)
    {
        if (!_blockOnUnitDestination.ContainsKey(block))
        {
            _blockOnUnitDestination.Add(block, false);
        }
    }
    private void HandleBlockUnitDestionation(Block block)
    {
        //Check Complete
        if (!_blockOnUnitDestination.ContainsKey(block)) return;

        _blockOnUnitDestination[block] = true;

        if(_blockOnUnitDestination.Values.All(x => x == true))
        {        
            SetComplete(true);
            //OnCompleteRoom?.Invoke(this);
        }
        //Check can raycast
        if (!_blockOnUnitDestinationsBlockRaycast.ContainsKey(block)) return;
        _blockOnUnitDestinationsBlockRaycast[block] = true;

        if (_blockOnUnitDestinationsBlockRaycast.Values.All(x => x == true))
        {
            ChangePlaceableState(PlaceableState.Placed);
            _blockOnUnitDestinationsBlockRaycast.Clear();
        }
    }

    public void OnComplete()
    {
        gameObject.SetActive(false);
        foreach (var item in _blockOnUnitDestination)
        {
            Block block = item.Key;
            block.gameObject.SetActive(false);
            IOccupier occupier = block.GetLastOccupier();
            if (occupier != null)
            {
                occupier.OnPlaceable(true);
            }
           
        }
    }

    public void Dispose()
    {

    }
    internal void OnMove(bool onMove)
    {
        _sortingGroup.sortingOrder = onMove ? 10 : 2;
    }

    internal void Move(Vector3 worldPosition, Dictionary<Block, Cell> _blockRaycastedToCellDict)
    {
        transform.position = worldPosition;
        foreach (var valueKey in _blockRaycastedToCellDict)
        {
            _data.SetRaycast(valueKey.Key, valueKey.Value);
        }
    }
    public void ResetPosition()
    {
        transform.localPosition = _data.InitPoint;
        //if (GetPlaceableState != PlaceableState.Placed || _data.BlocksRaycastCell.Any(x => x.Value == null || x.Value.IsOccupier()))
        //{
        //    transform.localPosition = _data.InitPoint;
        //}
        //else
        //{
        //    foreach (var blockCellValuekey in _data.BlocksRaycastCell)
        //    {
        //        Block block = blockCellValuekey.Key;
        //        Cell cell = blockCellValuekey.Value;
        //        if (block == null || cell == null) continue;

        //        block.transform.parent = cell.transform;
        //        block.transform.localPosition = new Vector3(0, 0, 0);
        //        cell.SetOccupier(block);
        //    }
            
        //}
    }
    public void RecoverRoom()
    {
        foreach (var block in _data.Blocks)
        {
            if (block != null)
            {
                //block.OnPlaceable(true);
                block.transform.parent = transform;
                block.ResetPosition();
            }
        }
    }
    public void SetCheckPointPosition(Vector3 point)
    {
        _data.InitPoint = point;
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
    public void SetComplete(bool isComplete)
    {
        _data.IsCompelete = isComplete;
    }
    public bool IsComplete()
    {
        return _data.IsCompelete;
    }
    #region PLACEABLE STATE
    // this state is config
    public bool GetMoveableState => _canMoveableState;
    public void SetMoveableState(bool moveableState)
    {
        _canMoveableState = moveableState;
    }
    //This state can change in runtime
    public PlaceableState GetPlaceableState => _state;
    public void ChangePlaceableState(PlaceableState newState)
    {
        _state = newState;
    }
    public void UpdateWeight(int weight)
    {
        _weight += weight;
    }
    #endregion
    [SerializeField]
    public class Data
    {
        private bool _isCompelete;
        private bool _isPlaced;
        private List<Block> _blocks = new List<Block>();
        private Dictionary<Block, Cell> _blocksRaycastCell = new Dictionary<Block, Cell>();
        private Vector3 _initPoint;
        public Vector3 InitPoint { get => _initPoint; set => _initPoint = value; }
        public List<Block> Blocks { get => _blocks; set => _blocks = value; }
        public bool IsCompelete { get => _isCompelete; set => _isCompelete = value; }
        public Dictionary<Block, Cell> BlocksRaycastCell => _blocksRaycastCell;

        public void SetRaycast(Block block , Cell cell)
        {
            if (cell == null) return;
            if (!_blocksRaycastCell.ContainsKey(block))
            {
                _blocksRaycastCell.Add(block, cell);
            }
            else
            {
                _blocksRaycastCell[block] = cell;
            }
        }
        public Data()
        {
            _blocks = new List<Block>();
            _isCompelete = false;
        }

   
    }
}
