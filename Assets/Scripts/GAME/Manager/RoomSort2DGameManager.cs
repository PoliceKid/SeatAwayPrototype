using DG.Tweening;
using Game.Core;
using Injection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
[System.Serializable]
public class RoomSort2DGameManager : IDisposable
{
    #region CURRENT DATA
    [Inject] private Injector injector;
    [Inject] private Timer _timer;
    [Inject] private PathFindingService _pathFindingService;
    [Inject] private CoroutineHelper _coroutineHelper;
    private RoomSort2DGameView _gameView;
    public RoomSort2DGameManager(RoomSort2DGameView gameView)
    {
        _gameView = gameView;
    }
    private List<Room> _rooms = new List<Room>();
    private Architecture _architecture;
    private Room _currentRoomInteract;
    private Dictionary<Block, Cell> _blockRaycastedToCellDict = new Dictionary<Block, Cell>();
    private Dictionary<Block,Cell> _blockPlacedOnCell = new Dictionary<Block, Cell> ();
    private Dictionary<Vector3, Room> _roomSpawner = new Dictionary<Vector3, Room>();
    private List<Gateway> _gateWays = new List<Gateway>();
    private StageManager _stageManager;
    private RoomSpawnerManager _roomSpawnerManager;
    private bool hasInit;
    #endregion
    #region FLOW
    public void Initialize()
    {
        _timer.POST_TICK += PostTick;
        _timer.FIXED_TICK += FixedTick;
        if (!hasInit)
        {
            _stageManager = new StageManager();
        
            LoadInitialLevel();
            hasInit = true;
        }
    }
    private void PostTick()
    {
        if (!hasInit) return;
        CheckRaycast();
    }
    private void FixedTick()
    {

    }
    public void Dispose()
    {
        _timer.POST_TICK -= PostTick;
        _timer.FIXED_TICK -= FixedTick;
    }
    public int CountBlockPlacedEmpty()
    {
        return _blockPlacedOnCell.Count(x => !x.Key.IsFullOccupier());
    }
    public Queue<Unit> GetAllUnitSortedQueue(List<Gateway> _gateWays)
    {
        return _gateWays.SelectMany(gw => gw.GetUnitSortByCodename()).ToQueue();
    }
    public int GetAllUnitQueueCount(List<Gateway> _gateWays)
    {
        return _gateWays.SelectMany(gw => gw.GetUnitQueue).ToQueue().Count;
    }
    public int GetACount()
    {
        int unitCount = GetAllUnitQueueCount(_gateWays);
        int blockEmptyCount = CountBlockPlacedEmpty();
        int count = unitCount - blockEmptyCount;
        return count;
    }
    public int GetBCount()
    {
        int cellPlacableCount = _architecture.GetCells.Count - CountBlockPlacedEmpty();

        return cellPlacableCount;
    }
    private void InitLevelFromView(Transform roomParentCotainer, Transform architectureContainer, Transform gateWayContainer, Transform[] roomSpawnerPoints)
    {
        InitArchitectureFromView(architectureContainer);
        InitUnitFromView(gateWayContainer);

        List<Room> rooms = InitRoomFromView(roomParentCotainer);
        if(rooms != null)
        {
            RoomSpawnerManager roomSpawnerManager = new RoomSpawnerManager(rooms);
            InitRoomSpawner(roomSpawnerManager);
            SpawnRooms(roomSpawnerManager, roomSpawnerPoints, roomParentCotainer, GetACount(), GetBCount(), GetAllUnitSortedQueue(_gateWays));
        }
    }
    public void InitRoomSpawner(RoomSpawnerManager roomSpawnerManager)
    {
        _roomSpawnerManager = roomSpawnerManager;
        injector.Inject(_roomSpawnerManager);
    }
    private List<Room> InitRoomFromView(Transform roomContainer)
    {
        List<Room> roomsConfig = new List<Room>();
        foreach (Transform child in roomContainer)
        {
            Room room = child.GetComponent<Room>();
            if (room != null)
            {
                room.Init();
                room.gameObject.SetActive(false);
                roomsConfig.Add(room);
                room.OnCompleteRoom += HandleCompleteRoom;
            }
        }
        return roomsConfig;
    }

    private void HandleCompleteRoom(List<Block> blocks)
    {
        foreach (var block in blocks)
        {
            Cell cellConaintBlock = _architecture.GetCells.FirstOrDefault(x => x.GetOccupier().Contains(block));

            if (cellConaintBlock != null)
            {
                cellConaintBlock.ClearOccupiers();
            }
          
        }
        CheckUnitMoveToBlock(_gateWays);
        CheckGameWin(_gateWays);
    }

    private void CheckUnitMoveToBlock(List<Gateway> _gateWays)
    {
        foreach (var gateway in _gateWays)
        {
            bool gatewayCanUpdateQueue = false;
            gateway.DequeueUnitLoop((unit) =>
            {

                if (unit != null)
                {
                    Block block = GetBlockValiable(gateway.GetConnectedCell, unit.GetOccupierType(), out List<Cell> cellPath);
                    if (block == null) {
                        return false;

                    }
                    block.SetOccupier(unit);
                    List<Vector3> pathPositions = cellPath.Select(cell => cell.transform.position).ToList();
                    unit.MoveTo(pathPositions);
                    gateway.DequeueUnit();
                    gatewayCanUpdateQueue = true;
                    if (_blockPlacedOnCell.ContainsKey(block))
                    {
                        _blockPlacedOnCell.Remove(block);
                    }
                    return true;

                }
                return false;
            });
            if(gatewayCanUpdateQueue)
            gateway.MoveUnitsInQueue();

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
    private void InitUnitFromView(Transform gateWayContainer)
    {
        foreach (Transform child in gateWayContainer)
        {
            Gateway gateWay = child.GetComponent<Gateway>();
            if (gateWay != null)
            {
                gateWay.Init();
                _gateWays.Add(gateWay);
            }
        }
    }
    #endregion
    #region UPDATE BEHAVIOUR
    public List<Block> GetlistBlock(Room targetRoom)
    {
        if (targetRoom == null) return null;
        return targetRoom.GetBlocks;
    }
    private void InitCheckRaycast(List<Block> blocks, Dictionary<Block, Cell> _blockRaycastedToCellDict)
    {
        if (blocks == null) return;
        foreach (var item in blocks)
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
    List<Block> blocks;
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
                        //Offset
                        Vector3 worldPosition = _gameView.GetMainCam.ScreenToWorldPoint(mousePosition);
                        clickOffset = worldPosition - room.transform.position;
                        
                        _currentRoomInteract = room;
                        blocks = GetlistBlock(_currentRoomInteract);
                        if (blocks == null) return;
                        //If room is placeable state
                        if(_currentRoomInteract.GetPlaceableState == PlaceableState.Placed)
                        {
                            DisplaceableRoom(blocks, _currentRoomInteract);
                            DisPlaceableRoomData(blocks, _architecture);
                        }
                        //
                        _currentRoomInteract.OnMove(true);
                        InitCheckRaycast(blocks, _blockRaycastedToCellDict);
                        _isValidPlace = true;
                    }
                }
            }
        }

        if (Input.GetMouseButton(0))
        {
            if (_currentRoomInteract == null) return;

            Vector3 worldPosition = _gameView.GetMainCam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 targetPoint = new Vector3(worldPosition.x - clickOffset.x, 2, worldPosition.z - clickOffset.z+ 1);

            _currentRoomInteract.Move(targetPoint);
            foreach (var block in blocks)
            {
                Ray blockRay = new Ray(block.transform.position, Vector3.down);
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
            if (CheckPlaceable(_blockRaycastedToCellDict))
            {
                ClearCellPreviewArchitecture(_architecture);
                SetPreviewToCell(_blockRaycastedToCellDict);
                _currentRoomInteract.ChangePlaceableState(PlaceableState.Placeable);
            }
            else
            {
                ClearCellPreviewArchitecture(_architecture);
                _currentRoomInteract.ChangePlaceableState(PlaceableState.Free);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (_currentRoomInteract == null) return;

            if (_currentRoomInteract.GetPlaceableState == PlaceableState.Placeable)
            {
                PlaceBlockRaycastToCell(_blockRaycastedToCellDict);
                _currentRoomInteract.ChangePlaceableState(PlaceableState.Placed);
                
            }
            else
            {
                DisplaceableRoom(blocks,_currentRoomInteract);
                DisPlaceableRoomData(blocks, _architecture);
            }

            ClearCellPreviewArchitecture(_architecture);
            ClearCellRaycast(_architecture);
            clickOffset = Vector3.zero;
            _currentRoomInteract = null;
            _blockRaycastedToCellDict.Clear();
        }
    }

    private void CheckGameWin(List<Gateway> _gateWays)
    {
        if (_gateWays.Count == 0) return;
        if(_gateWays.All(x => x.GetUnitQueue.Count ==0)){
            Debug.Log("Game WINN");
            OnLevelComplete();
        }
    
    }
    public void SetBlockRaycastToCell(Block blockRaycast, Cell cell)
    {
        if (!_blockRaycastedToCellDict.ContainsKey(blockRaycast)) return;
        _blockRaycastedToCellDict[blockRaycast] = cell;  
        if (cell != null)
        {
            cell.SetCurrentBlockRaycast(blockRaycast);

        }
    }
    private void SetPlaceableCellCondition(Cell cell)
    {
        if ( cell == null) return;
        if (!cell.IsOccupier()) return;
        IOccupier lastedOccupier  = cell.GetLastOccupier();
        if (lastedOccupier == null) return;
        {
            if (lastedOccupier.GetOccupierType() != BlockType.Normal.ToString())
            {
                BlockType blockTypeAvaliableOnCell = (lastedOccupier.GetOccupierType() == BlockType.Exdoor.ToString() ? BlockType.Indoor : BlockType.Exdoor);

                Cell.PlaceableCondition placeableCondition = new Cell.PlaceableCondition(blockTypeAvaliableOnCell, -lastedOccupier.GetDirection());
                cell.SetPlaceableCondition(cell, placeableCondition);

                if (lastedOccupier.GetOccupierType() == BlockType.Indoor.ToString())
                {

                    BlockType exceptblockTypeOnCell = (lastedOccupier.GetOccupierType() == BlockType.Exdoor.ToString() ?  BlockType.Exdoor: BlockType.Indoor);
                    Cell cellNeighbor = GetCellNeighborByDirection(cell, lastedOccupier.GetDirection());

                    Cell.PlaceableWithNeighborCondition placeableNeighborAction = new Cell.PlaceableWithNeighborCondition(exceptblockTypeOnCell,cell);
                    cellNeighbor.SetPlaceableCondition(cell, placeableNeighborAction);
                }
            }
            {
                if(lastedOccupier.GetOccupierType() != BlockType.Exdoor.ToString())
                {
                    var neighbors = cell.GetNeighbors();
                    foreach (var cellNeighborValueKey in neighbors)
                    {
                        var cellNeighbor = cellNeighborValueKey.Value;
                        var cellNeighborDir = cellNeighborValueKey.Key;
                        if (cellNeighborValueKey.Value != null)
                        {
                            if (!cellNeighbor.IsOccupier())
                            {
                                Cell.ExceptPlaceableCondition exceptPlaceableCondition = new Cell.ExceptPlaceableCondition(BlockType.Indoor, -cellNeighborValueKey.Key);
                                cellNeighbor.SetPlaceableCondition(cell, exceptPlaceableCondition);
                            }
                        }
                    }
                }
            }
        }       
    }
    public void SetPlaceableCellCondition(List<Cell> cells)
    {
        foreach (var cell in cells)
        {
            if (cell != null)
            {
                cell?.ClearPlaceableCondition();
                SetPlaceableCellCondition(cell);
            }
        }
    }
    public bool CheckPlaceable(Dictionary<Block, Cell> _blockRaycastedToCellDict)
    {
        if (_blockRaycastedToCellDict.Count == 0) return false;
        if (_blockRaycastedToCellDict.Any(x => x.Value == null || !x.Value.CheckPlaceableCondition()))
        {
            return false;
        }
        return true;
    }
 
    #endregion
    #region ROOM API
    public void PlaceBlockRaycastToCell(Dictionary<Block, Cell> _blockCheckRaycastDict)
    {
        if (_blockCheckRaycastDict == null) return;
        foreach (var blockCellValuekey in _blockCheckRaycastDict)
        {
            Block block = blockCellValuekey.Key;
            Cell cell = blockCellValuekey.Value;
            if (block == null || cell == null) continue;

            block.transform.parent = cell.transform;
            block.transform.localPosition = new Vector3(0, 0, 0);
            cell.SetOccupier(block);
            if (!_blockPlacedOnCell.ContainsKey(block))
            {
                _blockPlacedOnCell.Add(block, cell);
            }
            else
            {
                _blockPlacedOnCell[block] = cell;
            }
            
        }   
        CheckUnitMoveToBlock(_gateWays);
        SetPlaceableCellCondition(_blockCheckRaycastDict.Values.ToList());

        Vector3 currentRoomSpawnerPoint = _roomSpawner.FirstOrDefault(x => x.Value == _currentRoomInteract).Key;
        if (currentRoomSpawnerPoint != null)
        {
            _roomSpawner[currentRoomSpawnerPoint] = null;

            if(_roomSpawner.All(x => x.Value == null)){

                GetACount();
                GetBCount();

                SpawnRooms(_roomSpawnerManager,_levelContainer.GetRoomSpawnerPoints, _levelContainer.GetRoomContainer,GetACount(),GetBCount(), GetAllUnitSortedQueue(_gateWays));
            }
        }

    }         
    public void DisplaceableRoom(List<Block> blocks, Room _currentRoomInteract)
    {
        if (blocks == null) return;
        if (_currentRoomInteract == null) return;
        _currentRoomInteract.ResetPosition();
        foreach (var block in blocks)
        {
            if (block != null)
            {
                //block.OnPlaceable(true);
                block.transform.parent = _currentRoomInteract.transform;
                block.ResetPosition();
            }
        }
    }

    private void DisPlaceableRoomData(List<Block> blocks, Architecture _architecture)
    {
        if (blocks == null) return;
        foreach (var block in blocks)
        {
            List<Cell> cellValiableCondition = _architecture.GetCells.Where(x => x.GetOccupier().Contains(block)).ToList();
            foreach (var cell in cellValiableCondition)
            {
                cell?.RemoveOccupier(block);
            }
            SetPlaceableCellCondition(cellValiableCondition);
            if (_blockPlacedOnCell.ContainsKey(block))
            {
                _blockPlacedOnCell.Remove(block);
            }
        }
    }
    public Block GetBlockValiable(Cell startCell,string codeName, out List<Cell> cellPath)
    {
        cellPath = null;
        if (startCell.IsOccupier())
        {
            if(startCell.GetLastOccupier().GetCodeName() != codeName)
            {
                return null;
            }
            if (startCell.IsChildOccupier())
            {
                return null;
            }
        }
        _blockPlacedOnCell = _blockPlacedOnCell.OrderBy(x => Vector3.Distance(x.Value.transform.localPosition, startCell.transform.localPosition)).ToDictionary(x => x.Key, y => y.Value);
        foreach (var blockOnCell in _blockPlacedOnCell)
        {
            Cell cell = blockOnCell.Value;
            Block block = blockOnCell.Key;
            if(cell == null || block == null) continue;

            if (block.IsFullOccupier()) continue;
            if (block.GetData.CodeName != codeName) continue;
             cellPath = _pathFindingService.FindPath(startCell, cell);

            if(cellPath.Count == 0) continue;
            return cellPath.Count>0 ? block : null;
        }
        return null;
        
    }

    #endregion
    #region ROOM SPAWNER
    public void SpawnRooms(RoomSpawnerManager roomSpawnerManager, Transform[] spawnerPoints, Transform parent, int countA, int countB, Queue<Unit> UnitSortByCodename)
    {
        _roomSpawner.Clear();
        Debug.Log("CountA: " + countA);
        Debug.Log("CountB: " + countB);

        //Room1
        int count = Mathf.Min(countA, countB);
        if (count < 1)
        {
            count = 1;
        }
        List<Room> roomConfigs = new List<Room>();
        Room roomPrefab1 = roomSpawnerManager.GetRoomConfig(count);
        if (roomPrefab1 != null)
        {
            roomConfigs.Add(roomPrefab1);
            Debug.Log("Room1: " + roomPrefab1.GetBlockCount);
        }
        //
        //Room2
        if (countA <= countB)
        {
            if(roomPrefab1 != null)
            count = countA - roomPrefab1.GetBlockCount;
        }
        else
        {
            count = countB;
        }
        if (count < 1)
        {
            count = 1;
        }
        Room roomPrefab2 = roomSpawnerManager.GetRoomConfig(count);
        if (roomPrefab2 != null)
        {
            roomConfigs.Add(roomPrefab2);
            Debug.Log("Room2: " + roomPrefab2.GetBlockCount);
        }
        //
        //Room3
        if (countA <= countB)
        {
            if(roomPrefab1 != null && roomPrefab2 != null)
            count = countA - roomPrefab2.GetBlockCount - roomPrefab2.GetBlockCount;
            if(count < 1)
            {
                count = 1;
            }
        }
        else
        {
            count = countB;
        }

        Room roomPrefab3 = roomSpawnerManager.GetRoomConfig(count);
        if (roomPrefab3 != null)
        {
            roomConfigs.Add(roomPrefab3);
            Debug.Log("Room3: " + roomPrefab3.GetBlockCount);
        }
        //
        if(roomPrefab1 != null && roomPrefab2 != null && roomPrefab3 != null)
        {
            int index = 0;
            foreach (var roomConfig in roomConfigs)
            {
                Vector3 point = spawnerPoints[index].position;
                Room room = _gameView.SpawnRoom(roomConfig.gameObject, point, Quaternion.identity, parent);
                if (room != null)
                {
                    room.Init();
                    room.gameObject.SetActive(true);
                    room.OnCompleteRoom += HandleCompleteRoom;
                    _rooms.Add(room);
                    _roomSpawner.Add(point, room);

                    foreach (var block in room.GetBlocks)
                    {
                        Unit unit = null;
                        if (UnitSortByCodename.Count > 0)
                        {
                            unit = UnitSortByCodename.Dequeue();
                        }
                        block.Init(unit == null ? CodeNameType.Blue : unit.GetCodeNameType(), room.transform);
                    }
                }

                index++;
            }
        }
    }
    #endregion
    #region ARCHITECTURE API
    #region NEIGHBOR 
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
            Dictionary<Vector3, Cell> cellNeighborDicts = GetCellNeighbors(cell);
            if (cellNeighborDicts != null)
            {
                foreach (var cellNeighbor in cellNeighborDicts)
                {
                    if (cellNeighbor.Value.GetCellType != BlockType.Normal)
                    {
                        if(cell.GetDirection() != cellNeighbor.Key * (cellNeighbor.Value.GetCellType == BlockType.Exdoor ? -1:1))
                        {
                            continue;
                        }
                    }

                    cell.SetNeighbor(cellNeighbor.Key, cellNeighbor.Value);
                }
            }
            cell.InitStaticCondition();
        }
    }
    public Dictionary<Vector3, Cell> GetCellNeighbors(Cell cell)
    {
        Dictionary<Vector3, Cell> neighbors = new Dictionary<Vector3, Cell>();

        // Các chỉ số offset cho 4 hướng
        for (int i = 0; i < directions.GetLength(0); i++)
        {
            int newX = (int)cell.transform.localPosition.x + directions[i, 0];
            int newZ = (int)cell.transform.localPosition.z + directions[i, 1];

            // Kiểm tra nếu (newX, newY) có Cell hay không
            Cell neighbor = _architecture.GetCell(newX, newZ);
            if (neighbor != null)
            {
                neighbors.Add(new Vector3(directions[i, 0],0, directions[i, 1]), neighbor);
            }
        }

        return neighbors;
    }
    public Cell GetCellNeighborByDirection(Cell cell, Vector3 dir)
    {
        if (_architecture == null) return null;
        int newX = (int)cell.transform.localPosition.x + (int)dir.x;
        int newY = (int)cell.transform.localPosition.z + (int)dir.z;
        Cell neighbor = _architecture.GetCell(newX, newY);
        return neighbor;
    }
    private void InitNeighborBlockFromRoom(List<Room> rooms)
    {
        if (rooms == null) return;
        foreach (var room in rooms)
        {
            var blocks = GetlistBlock(room);
            foreach (var block in blocks)
            {
                var neighbors= GetBlockNeighbors(room, block);
                foreach (var valueKey in neighbors)
                {
                    block.SetNeighbor(valueKey.Key, valueKey.Value);
                }
            }
        }
        
    }
    public Dictionary<Vector3, Block> GetBlockNeighbors(Room room,Block block)
    {
        Dictionary<Vector3, Block> neighbors = new Dictionary<Vector3, Block>();

        // Các chỉ số offset cho 4 hướng
        for (int i = 0; i < directions.GetLength(0); i++)
        {
            int newX = (int)block.transform.localPosition.x + directions[i, 0];
            int newZ = (int)block.transform.localPosition.z + directions[i, 1];

            // Kiểm tra nếu (newX, newY) có Cell hay không
            Block neighbor = room.GetBlock(newX, newZ);
            if (neighbor != null)
            {
                neighbors.Add(new Vector3(directions[i, 0],0, directions[i, 1]), neighbor);
            }
        }

        return neighbors;
    }

    #endregion
    #region PREVIEW
    private void SetPreviewToCell(Dictionary<Block, Cell> _blockRaycastedToCellDict)
    {
        foreach (var item in _blockRaycastedToCellDict)
        {
            item.Value.OnPlaceable(true);  
        }
    }
    private void ClearCellPreviewArchitecture(Architecture _architecture)
    {
        foreach (var cell in _architecture.GetCells)
        {
            cell.OnPlaceable(false);
        }
    }
    private void ClearCellRaycast(Architecture _architecture)
    {
        foreach (var cell in _architecture.GetCells)
        {
            cell.SetCurrentBlockRaycast(null);
        }
    }
    #endregion
    #endregion
    #region STAGE API
    public void LoadInitialLevel()
    {
        GameObject level = _stageManager.GetCurrentLevelPrefab();
        if(level != null)
        {
            LoadLevel(level);
        }
    }
    private LevelContainer _levelContainer;
    private void LoadLevel(GameObject level)
    {
        if (level != null)
        {
            LevelContainer levelContainer = _gameView.SpawnLevel(level, Vector3.zero, Quaternion.identity);
            
            if (levelContainer != null)
            {
                _levelContainer = levelContainer;
                InitLevelFromView(levelContainer.GetRoomContainer, levelContainer.GetArchitectureContainer,levelContainer.GetGateWayContainer, levelContainer.GetRoomSpawnerPoints);
            }
        }
    }

    public void OnLevelComplete()
    {
        _currentRoomInteract = null;
        GameObject nextLevel = _stageManager.LoadNextLevel();
        if (nextLevel != null)
        {
            LoadLevel(nextLevel);
        }
    }

    public void RestartCurrentLevel()
    {
        GameObject currentLevel = _stageManager.RestartLevel();
        if (currentLevel != null)
        {
            LoadLevel(currentLevel);
        }
    }
    #endregion
    #region UNIT API
    public Unit GetFirstUnit(Gateway gateway)
    {
        return gateway.DequeueUnit();
    }
    
    #endregion
}
[Serializable]
public class Stage
{
    
}
public enum BlockType
{
    Normal,
    Indoor,
    Exdoor
}
public enum PlaceableState
{
    Placed,
    Placeable,
    Free
}
public enum CodeNameType
{
    Blue,
    Red,
    Yellow,
    Green,
    Purple
}