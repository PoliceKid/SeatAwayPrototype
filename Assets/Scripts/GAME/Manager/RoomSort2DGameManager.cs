using Game.Core;
using Injection;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[System.Serializable]
public class RoomSort2DGameManager : IDisposable
{
    #region CURRENT DATA
    [Inject] private Timer _timer;
    private RoomSort2DGameView _gameView;
    public RoomSort2DGameManager(RoomSort2DGameView gameView)
    {
        _gameView = gameView;
    }
    private List<Room> _rooms = new List<Room>();
    private Architecture _architecture;
    private Room _currentRoomInteract;
    private Dictionary<Block, Cell> _blockRaycastedToCellDict = new Dictionary<Block, Cell>();
    private List<Block> _blockPlaced = new List<Block>();
    #endregion
    #region FLOW
    public void Initialize()
    {
        _timer.POST_TICK += PostTick;
        _timer.FIXED_TICK += FixedTick;
        InitLevelFromView();

    }
    private void PostTick()
    {
        CheckRaycast();
    }
    private void FixedTick()
    {

    }
    public void Dispose()
    {
    }
    private void InitLevelFromView()
    {
        InitRoomFromView(_gameView.GetRoomParentCotainer);
        InitArchitectureFromView(_gameView.GetArchitectureContainer);
    }
    private void InitRoomFromView(Transform roomContainer)
    {
        foreach (Transform child in roomContainer)
        {
            Room room = child.GetComponent<Room>();
            if (room != null)
            {
                room.Init();
                _rooms.Add(room);
            }
        }
    }
    private void InitArchitectureFromView(Transform architectureContainer)
    {
        foreach (Transform child in architectureContainer)
        {
            Architecture architecture = child.GetComponent<Architecture>();
            if (architecture != null)
            {
                architecture.Init();
                _architecture = architecture;
            }
        }
        InitNeighborCellArchitecture(_architecture);
    }


    #endregion
    #region UPDATE BEHAVIOUR
    public List<Block> GetlistBlock(Room targetRoom)
    {
        if (targetRoom == null) return null;
        return targetRoom.GetBlocks;
    }
    private void InitCheckRaycast(Room room, Dictionary<Block, Cell> _blockRaycastedToCellDict)
    {
        if (room == null) return;
        foreach (var item in room.GetBlocks)
        {
            if (!_blockRaycastedToCellDict.ContainsKey(item))
            {
                _blockRaycastedToCellDict.Add(item, null);
            }
        }
    }
    Vector3 clickOffset;
    private RaycastHit[] _raycastHits = new RaycastHit[1];
    private bool _isValidPlace;
    private void CheckRaycast()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Input.mousePosition;
            Ray ray = _gameView.GetMainCam.ScreenPointToRay(mousePosition);

            if (Physics.RaycastNonAlloc(ray, _raycastHits, 100f, _gameView.GetBlockLayerMask) > 0)
            {
                RaycastHit hit = _raycastHits[0];
                Block block = hit.collider.GetComponentInParent<Block>();
                if (block != null)
                {
                    var room = block.GetData.initParent.GetComponent<Room>();
                    if (room != null)
                    {
                        Vector3 worldPosition = _gameView.GetMainCam.ScreenToWorldPoint(mousePosition);
                        clickOffset = worldPosition - room.transform.position;

                        _currentRoomInteract = room;
                        ResetRoom(_currentRoomInteract, _architecture);
                        _currentRoomInteract.OnMove(true);
                        InitCheckRaycast(_currentRoomInteract, _blockRaycastedToCellDict);
                        _isValidPlace = true;
                    }
                }
            }
        }

        if (Input.GetMouseButton(0))
        {
            if (_currentRoomInteract == null) return;

            Vector3 worldPosition = _gameView.GetMainCam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 targetPoint = new Vector3(worldPosition.x - clickOffset.x, worldPosition.y - clickOffset.y + 1, -5);

            _currentRoomInteract.Move(targetPoint);
            foreach (var block in _currentRoomInteract.GetBlocks)
            {
                Ray blockRay = new Ray(block.transform.position, Vector3.forward);
                Cell cellSlot = null;
                if (Physics.RaycastNonAlloc(blockRay, _raycastHits, 10f,_gameView.GetCellLayerMask) > 0)
                {
                    cellSlot = _raycastHits[0].collider.GetComponentInParent<Cell>();
                }
                _isValidPlace = cellSlot != null;
                SetBlockRaycastToCell(block, cellSlot);
                if (!_isValidPlace)
                {
                    break;
                }
            }

            if (!_isValidPlace)
            {
                ClearCellPreviewArchitecture(_architecture);
            }
            else if (CheckPlaceable(_blockRaycastedToCellDict))
            {
                ClearCellPreviewArchitecture(_architecture);
                PlacePreviewToCell(_blockRaycastedToCellDict);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (_currentRoomInteract == null) return;

            if (CheckPlaceable(_blockRaycastedToCellDict))
            {
                PlaceRoom(_blockRaycastedToCellDict);
            }
            else
            {
                ResetRoom(_currentRoomInteract, _architecture);
                _currentRoomInteract.ResetPosition();
            }

            ClearCellPreviewArchitecture(_architecture);
            clickOffset = Vector3.zero;
            _currentRoomInteract = null;
            _blockRaycastedToCellDict.Clear();
        }
    }

    public bool CheckBlockPlaceable(Block block)
    {
        if (block == null) return false;
        if (block.GetBlockType() != BlockType.Normal)
        {
            //Vector3 landBlockPoint = new Vector3(block.transform.position.x, block.transform.position.y, 0);
            Vector3 landBlockPoint = block.transform.position;
            Ray landBlockRay = new Ray(landBlockPoint, Vector2.up * (block.GetBlockType() == BlockType.Exdoor ? 1 : -1));
            if (Physics.RaycastNonAlloc(landBlockRay, _raycastHits, 0.622f, _gameView.GetBlockLayerMask) > 0)
            {
                Block blockRayast = _raycastHits[0].collider.GetComponentInParent<Block>();
                if (blockRayast != null)
                {
                    if (blockRayast.GetBlockType() == BlockType.Normal) return false;
                    bool result = blockRayast.GetBlockType() == (block.GetBlockType() == BlockType.Exdoor ? BlockType.Indoor : BlockType.Exdoor);
                    return result;
                }

            }
        }
        return true;
    }

    public void SetBlockRaycastToCell(Block block, Cell cell)
    {
        if (_currentRoomInteract == null) return;
        if (!_blockRaycastedToCellDict.ContainsKey(block)) return;
        _blockRaycastedToCellDict[block] = cell;
    }
    private void SetPlaceableCellCondition(Cell cell)
    {
        if ( cell == null) return;

        if (!cell.isOccupier()) return;
        IOccupier lastedOccupier  = cell.GetOccupier.Last();
        if (lastedOccupier == null) return;
        {
            if (lastedOccupier.GetBlockType() != BlockType.Normal)
            {

                BlockType blockTypeAvaliableOnCell = (lastedOccupier.GetBlockType() == BlockType.Exdoor ? BlockType.Indoor : BlockType.Exdoor);
                cell.SetPlaceableCondition(new Cell.PlaceableCondition(lastedOccupier, blockTypeAvaliableOnCell, -lastedOccupier.GetDirection()));
                Cell cellneighbor = null;
                if (lastedOccupier.GetBlockType() == BlockType.Indoor)
                {
                    cellneighbor = GetNeighborByDirection(cell, lastedOccupier.GetDirection());
                    if (cellneighbor != null)
                    {
                        cellneighbor.SetPlaceableCondition(new Cell.NeighborCondition(new List<Cell> { cell }));
                    }
                }
            }
        }       
    }
    public bool CheckPlaceable(Dictionary<Block, Cell> _blockRaycastedToCellDict)
    {
        if (_blockRaycastedToCellDict.Count == 0) return false;
        foreach (var valueKey in _blockRaycastedToCellDict)
        {
            if(valueKey.Value != null )
            {
                valueKey.Value.SetCurrentOccupierRaycast(valueKey.Key);

            }
        }
        if (_blockRaycastedToCellDict.All(x => x.Value != null && x.Value.CheckPlaceableCondition()))
        {
            return true;
        }
        return false;
    }
    private void PlacePreviewToCell(Dictionary<Block, Cell> _blockRaycastedToCellDict)
    {
        foreach (var item in _blockRaycastedToCellDict)
        {
            item.Value?.OnPlaceable();
        }
    }
    private void ClearCellPreviewArchitecture(Architecture _architecture)
    {
        foreach (var cell in _architecture.GetCells)
        {
            cell.ResetColor();
            cell.SetCurrentOccupierRaycast(null);
        }
    }
    #endregion
    #region LEVEL API
    #endregion
    #region ROOM API
    public void PlaceRoom(Dictionary<Block, Cell> _blockCheckRaycastDict)
    {
        if (_blockCheckRaycastDict == null) return;
        foreach (var blockCellValuekey in _blockCheckRaycastDict)
        {
            Block block = blockCellValuekey.Key;
            Cell cell = blockCellValuekey.Value;
            if (block == null || cell == null) continue;

            cell.SetOccupier(block);
            block.OnPlaceable(true);
            block.transform.parent = cell.transform;
            block.transform.localPosition = new Vector3(0, 0, -5);
            if (!_blockPlaced.Contains(block))
            {
                _blockPlaced.Add(block);
            }
            cell.ClearPlaceableCondition();
            SetPlaceableCellCondition(cell);
            
        }
    }
    public void ResetRoom(Room _currentRoomInteract, Architecture _architecture)
    {
        if (_currentRoomInteract == null) return;
        foreach (var block in _currentRoomInteract.GetBlocks)
        {
            if (block != null)
            {
                block.OnPlaceable(false);
                block.transform.parent = _currentRoomInteract.transform;
                block.ResetPosition();
                foreach (var cell in _architecture.GetCells)
                {
                    if (cell.GetOccupier.Contains(block))
                    {
                        cell?.RemoveOccupier(block);
                        cell.ClearPlaceableCondition();
                        SetPlaceableCellCondition(cell);
                    }
                    
                }
                if (_blockPlaced.Contains(block))
                {
                    _blockPlaced.Remove(block);

                }
            }

        }
    }
    #endregion
    #region CELL API
    public static int[,] directions = new int[,]
       {
            { -1,  0 }, // Trên
            {  1,  0 }, // Dưới
            {  0, -1 }, // Trái
            {  0,  1 } // Phải
       };
    private void InitNeighborCellArchitecture(Architecture _architecture)
    {
        if (_architecture == null) return;
        foreach (var cell in _architecture.GetCells)
        {
            List<Cell> cells = GetNeighbors(cell);
            if (cells != null)
            {
                foreach (var cellNeighbor in cells)
                {
                    cell.SetNeighbor(cellNeighbor);
                }
            }
        }
    }
    public List<Cell> GetNeighbors(Cell cell)
    {
        List<Cell> neighbors = new List<Cell>();

        // Các chỉ số offset cho 4 hướng
        for (int i = 0; i < directions.GetLength(0); i++)
        {
            int newX = (int)cell.transform.position.x + directions[i, 0];
            int newY = (int)cell.transform.position.y + directions[i, 1];

            // Kiểm tra nếu (newX, newY) có Cell hay không
            Cell neighbor = _architecture.GetCell(newX, newY);
            if (neighbor != null)
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }
    public Cell GetNeighborByDirection(Cell cell, Vector3 dir)
    {
        if (_architecture == null) return null;
        int newX = (int)cell.transform.position.x + (int)dir.x;
        int newY = (int)cell.transform.position.y + (int)dir.y;
        Cell neighbor = _architecture.GetCell(newX, newY);
        return neighbor;
    }
    #endregion
}
public enum BlockType
{
    Normal,
    Indoor,
    Exdoor
}