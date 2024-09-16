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
    [Inject] private Injector _injector;
    [Inject] private Timer _timer;
    [Inject] private PathFindingService _pathFindingService;
    [Inject] private CoroutineHelper _coroutineHelper;
    private RoomSort2DGameView _gameView;
    public RoomSort2DGameManager(RoomSort2DGameView gameView)
    {
        _gameView = gameView;
    }
    private List<Room> _rooms;
    private Room _currentRoomInteract;
    private Architecture _architecture;
    private Dictionary<Block, Cell> _blockRaycastedToCellDict;
    private Dictionary<Block, Cell> _blockPlacedOnCell;
    private Dictionary<Vector3, Room> _roomSpawner;
    private List<Gateway> _gateWays;
    private StageManager _stageManager;
    private LevelContainer _levelContainer;
    private RoomSpawnerManager _roomSpawnerManager;
    private roomRaycastCheck _roomRaycastCheck;
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
            LoadInitialLevel(_stageManager);
            //GameObject roomGO = new GameObject("Room GO");
            //_roomRaycastCheck = new roomRaycastCheck(roomGO);
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
    private bool CheckGameWin(List<Gateway> _gateWays)
    {
        if (_gateWays.Count == 0) return false;
        if (_gateWays.All(x => x.IsCompleteWay()))
        {
            return true;
           
        }
        return false;
    }

    private void InitLevelFromView(Transform roomConfigCotainer, Transform roomStaticCotainer, Transform architectureContainer, Transform gateWayContainer, Transform[] roomSpawnerPoints)
    {
        _blockPlacedOnCell = new Dictionary<Block, Cell>();
        _rooms = new List<Room>();
        InitArchitectureFromView(architectureContainer);
        InitGateWayFromView(gateWayContainer);
        List<Room> rooms = InitRoomConfigFromView(roomConfigCotainer);
        if (rooms == null) return;
        RoomSpawnerManager roomSpawnerManager = new RoomSpawnerManager(rooms);
        InitRoomStaticFromView(roomStaticCotainer);
        InitRoomSpawner(roomSpawnerManager);
        SpawnRooms(roomSpawnerManager, roomSpawnerPoints, roomConfigCotainer, GetACount(), GetBCount(), GetAllUnitQueue(_gateWays));
    }
    public void InitRoomSpawner(RoomSpawnerManager roomSpawnerManager)
    {
        _roomSpawnerManager = roomSpawnerManager;
        _injector.Inject(_roomSpawnerManager);
        _roomSpawner = new Dictionary<Vector3, Room>();
    }
    private List<Room> InitRoomConfigFromView(Transform roomContainer)
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
    private List<Room> InitRoomStaticFromView(Transform roomStaticContainer)
    {
        List<Room> roomsStatic = new List<Room>();
        foreach (Transform child in roomStaticContainer)
        {
            if (child.gameObject.activeSelf == false)
            {
                continue;
            }
            Room room = child.GetComponent<Room>();
            if (room != null)
            {
                room.Init();
                room.gameObject.SetActive(true);
                roomsStatic.Add(room);
                _rooms.Add(room);
                room.OnCompleteRoom += HandleCompleteRoom;
            }
            List<Block> blocks = room.GetBlocks;

            if (blocks != null)
            {
                InitBlockRaycastToCell(blocks);
                foreach (var block in blocks)
                {
                    block.Init(CodeNameType.Blue, room.transform);
                    Ray blockRay = new Ray(block.transform.position + Vector3.up * 5, Vector3.down);
                    Cell cellSlot = null;
                    if (Physics.RaycastNonAlloc(blockRay, _raycastHits, 10f, _gameView.GetCellLayerMask) > 0)
                    {
                        cellSlot = _raycastHits[0].collider.GetComponentInParent<Cell>();
                    }
                    _isValidPlace = cellSlot != null;

                    SetBlockRaycastToCell(block, cellSlot);

                }
                if (CheckBlockRaycastPlaceable(_blockRaycastedToCellDict))
                {
                    PlaceBlockRaycastToCell(_blockRaycastedToCellDict);
                    room.ChangePlaceableState(PlaceableState.Freeze);
                }
            }

        }
        return roomsStatic;
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
                    Block block = GetBlockValiable(gateway.GetConnectedCell, gateway.GetDirection(), unit.GetOccupierType(), out List<Cell> cellPath);
                    if (block == null)
                    {
                        return false;
                    }
                    block.SetOccupier(unit);
                    List<Vector3> pathPositions = cellPath.Select(cell => cell.transform.position).ToList();
                    unit.MoveTo(pathPositions, onDestination: true);
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
            if (gatewayCanUpdateQueue)
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
    private void InitGateWayFromView(Transform gateWayContainer)
    {
        _gateWays = new List<Gateway>();
        foreach (Transform child in gateWayContainer)
        {
            Gateway gateWay = child.GetComponent<Gateway>();
            if (gateWay != null)
            {
                gateWay.Init();
                _gateWays.Add(gateWay);
                gateWay.OnCompleteWay += HandleGateWayComplete;
            }
        }
    }
    private void HandleGateWayComplete()
    {
        if (CheckGameWin(_gateWays))
        {
            Debug.Log("Game WINN");
            OnLevelComplete();
        }
    }
    #endregion
    #region UPDATE BEHAVIOUR
    public List<Block> GetlistBlock(Room targetRoom)
    {
        if (targetRoom == null) return null;
        return targetRoom.GetBlocks;
    }
    private void InitBlockRaycastToCell(List<Block> blocks)
    {
        _blockRaycastedToCellDict = new Dictionary<Block, Cell>();
        if (blocks == null) return;
        foreach (var item in blocks)
        {
            if (!_blockRaycastedToCellDict.ContainsKey(item))
            {
                _blockRaycastedToCellDict.Add(item, null);
            }
        }
    }
    #region RAYCAST PROPERTIES
    Vector3 clickOffset;
    private RaycastHit[] _raycastHits = new RaycastHit[1];
    private bool _isValidPlace;
    List<Block> blocks;
    //List<BlockRaycast> blockRaycastings;
    #endregion
    private void CheckRaycast()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Input.mousePosition;
            Ray ray = _gameView.GetMainCam.ScreenPointToRay(mousePosition);

            if (Physics.RaycastNonAlloc(ray, _raycastHits, 100f, _gameView.GetBlockLayerMask) > 0)
            {
                RaycastHit hit = _raycastHits[0];
                Block blockHit = hit.collider.GetComponentInParent<Block>();
                if (blockHit != null)
                {
                    var room = blockHit.GetData.initParent.GetComponent<Room>();
                    if (room != null)
                    {
                        //Offset
                        Vector3 worldPosition = _gameView.GetMainCam.ScreenToWorldPoint(mousePosition);
                        clickOffset = worldPosition - room.transform.position;

                        if (room.GetPlaceableState == PlaceableState.Freeze) return;
                        _currentRoomInteract = room;

                        blocks = GetlistBlock(_currentRoomInteract);
                        if (blocks == null) return;
                        //If room is placeable state
                        if (_currentRoomInteract.GetPlaceableState == PlaceableState.Placed)
                        {
                            _currentRoomInteract.RecoverRoom();
                            ClearRoomOccupier(_currentRoomInteract, _architecture);

                            //foreach (var block in blocks)
                            //{
                            //    BlockRaycast blockRaycastFree = _roomRaycastCheck.GetPreeBlockRaycast();
                            //    _roomRaycastCheck.AssigBlockPoint(blockRaycastFree,block.transform.localPosition);
                            //    blockRaycastFree.AssignBlockOrigin(block);
                            //}
                            //blockRaycastings = _roomRaycastCheck.GetBlockRaycasting();
                        }
                        _currentRoomInteract.OnMove(true);
                        InitBlockRaycastToCell(blocks);
                        _isValidPlace = true;
                    }
                }
            }
        }

        if (Input.GetMouseButton(0))
        {
            if (_currentRoomInteract == null) return;

            Vector3 worldPosition = _gameView.GetMainCam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 targetPoint = new Vector3(worldPosition.x - clickOffset.x, 2, worldPosition.z - clickOffset.z + 1);

            //if (_currentRoomInteract.GetPlaceableState == PlaceableState.Placed)
            //{
            //    _roomRaycastCheck.AssigRoomPoint(targetPoint);
            //    if (blockRaycastings != null)
            //    {
            //        foreach (var blockRaycast in blockRaycastings)
            //        {
            //            Ray blockRay = new Ray(blockRaycast.Point.transform.position, Vector3.down);
            //            Cell cellSlot = null;
            //            if (Physics.RaycastNonAlloc(blockRay, _raycastHits, 10f, _gameView.GetCellLayerMask) > 0)
            //            {
            //                cellSlot = _raycastHits[0].collider.GetComponentInParent<Cell>();
            //                blockRaycast.AssignTargetCellOrigin(cellSlot);
            //            }
            //            _isValidPlace = cellSlot != null;
            //            SetBlockRaycastToCell(blockRaycast.BlockOrigin, cellSlot);
            //            if (!_isValidPlace)
            //            {
            //                break;
            //            }
            //        }
            //    }
            //    if (CheckBlockRaycastPlaceable(_blockRaycastedToCellDict))
            //    {
            //        _currentRoomInteract.Move(targetPoint, _blockRaycastedToCellDict);
            //        ClearCellPreviewArchitecture(_architecture);
            //        SetBlockPreviewToCell(_blockRaycastedToCellDict);
            //    }
            //    else
            //    {
            //        ClearCellPreviewArchitecture(_architecture);
            //    }
            //}
            //else
            //{
            _currentRoomInteract.Move(targetPoint, _blockRaycastedToCellDict);
            foreach (var block in blocks)
            {
                Ray blockRay = new Ray(block.transform.position, Vector3.down);
                Cell cellSlot = null;
                if (Physics.RaycastNonAlloc(blockRay, _raycastHits, 10f, _gameView.GetCellLayerMask) > 0)
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
            if (CheckBlockRaycastPlaceable(_blockRaycastedToCellDict))
            {
                ClearCellPreviewArchitecture(_architecture);
                SetBlockPreviewToCell(_blockRaycastedToCellDict);
            }
            else
            {
                ClearCellPreviewArchitecture(_architecture);
            }
            //}

        }

        if (Input.GetMouseButtonUp(0))
        {
            if (_currentRoomInteract == null) return;

            if (CheckBlockRaycastPlaceable(_blockRaycastedToCellDict))
            {
                _currentRoomInteract.ChangePlaceableState(PlaceableState.Placed);

                PlaceBlockRaycastToCell(_blockRaycastedToCellDict);
                CheckSpawnRooms(_currentRoomInteract);
            }
            else
            {
                _currentRoomInteract.RecoverRoom();
                _currentRoomInteract.ResetPosition();
                ClearRoomOccupier(_currentRoomInteract, _architecture);
            }

            ClearCellPreviewArchitecture(_architecture);
            ClearCellRaycast(_architecture);
            clickOffset = Vector3.zero;
            _currentRoomInteract = null;
            //_roomRaycastCheck.ClearRaycast();
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
    public bool CheckBlockRaycastPlaceable(Dictionary<Block, Cell> _blockRaycastedToCellDict)
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

    }
    private void CheckSpawnRooms(Room targetRoom)
    {
      
        Vector3 currentRoomSpawnerPoint = _roomSpawner.FirstOrDefault(x => x.Value == _currentRoomInteract).Key;
        if (currentRoomSpawnerPoint != null)
        {
            _roomSpawner[currentRoomSpawnerPoint] = null;

            if (_roomSpawner.All(x => x.Value == null))
            {
                if (CheckGameWin(_gateWays))
                {
                    return;
                }
                SpawnRooms(_roomSpawnerManager, _levelContainer.GetRoomSpawnerPoints, _levelContainer.GetRoomConfigContainer, GetACount(), GetBCount(), GetAllUnitQueue(_gateWays));
            }
        }
    }
    private void ClearRoomOccupier(Room targetRoom, Architecture _architecture)
    {
        List<Block> blocks = GetlistBlock(targetRoom);
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
    public Block GetBlockValiable(Cell startCell, Vector3 direction, string codeName, out List<Cell> cellPath)
    {
        cellPath = null;
        if (startCell.IsOccupier())
        {

            if (startCell.GetLastOccupier().GetCodeName() != codeName)
            {
                return null;
            }
            if (startCell.IsChildOccupier())
            {
                return null;
            }
            if (direction == startCell.GetLastOccupier().GetDirection()) return null;
        }
        _blockPlacedOnCell = _blockPlacedOnCell.OrderBy(x => Vector3.Distance(x.Value.transform.localPosition, startCell.transform.localPosition)).ToDictionary(x => x.Key, y => y.Value);
        foreach (var blockOnCell in _blockPlacedOnCell)
        {
            Cell cell = blockOnCell.Value;
            Block block = blockOnCell.Key;
            if (cell == null || block == null) continue;

            if (block.IsFullOccupier()) continue;
            if (block.GetData.CodeName != codeName) continue;
            cellPath = _pathFindingService.FindPath(startCell, cell);

            if (cellPath.Count == 0) continue;
            return cellPath.Count > 0 ? block : null;
        }
        return null;

    }
    private void InitNeighborBlockFromRoom(List<Room> rooms)
    {
        if (rooms == null) return;
        foreach (var room in rooms)
        {
            var blocks = GetlistBlock(room);
            foreach (var block in blocks)
            {
                var neighbors = GetBlockNeighbors(room, block);
                foreach (var valueKey in neighbors)
                {
                    block.SetNeighbor(valueKey.Key, valueKey.Value);
                }
            }
        }

    }
    public Dictionary<Vector3, Block> GetBlockNeighbors(Room room, Block block)
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
                neighbors.Add(new Vector3(directions[i, 0], 0, directions[i, 1]), neighbor);
            }
        }

        return neighbors;
    }
    #endregion
    #region ROOM SPAWNER
    public void SpawnRooms(RoomSpawnerManager roomSpawnerManager, Transform[] spawnerPoints, Transform parent, int countA, int countB, Queue<Unit> UnitQueueValiable)
    {
        _roomSpawner.Clear();
        Debug.Log("CountA: " + countA);
        Debug.Log("CountB: " + countB);
        List<Unit> ListUnitExcept = new List<Unit>();

        //Room1
        int count = Mathf.Min(countA, countB);
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
            if (roomPrefab1 != null)
                count = countA - roomPrefab1.GetBlockCount;
        }
        else
        {
            count = countB;
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
            if (roomPrefab1 != null && roomPrefab2 != null)
                count = countA - roomPrefab2.GetBlockCount - roomPrefab2.GetBlockCount;
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
        if (roomPrefab1 != null && roomPrefab2 != null && roomPrefab3 != null)
        {
            int allRoomBlockCount = roomPrefab1.GetBlockCount + roomPrefab2.GetBlockCount + roomPrefab3.GetBlockCount;
            UnitQueueValiable = GetAllUnitQueueRange(UnitQueueValiable, allRoomBlockCount);
            UnitQueueValiable = GetAllUnitSortedQueue(UnitQueueValiable);

            List<Block> blockPlacedEmpties = GetBlockPlacedEmpty();
            if (blockPlacedEmpties.Count > 0)
            {
                foreach (var block in blockPlacedEmpties)
                {
                    Unit unitAlreadyHasCodename = UnitQueueValiable.FirstOrDefault(x => x.GetCodeName() == block.GetCodeName());
                    if (unitAlreadyHasCodename != null)
                    {
                        ListUnitExcept.Add(unitAlreadyHasCodename);
                    }
                }
                UnitQueueValiable = new Queue<Unit>(UnitQueueValiable.Where(x => !ListUnitExcept.Contains(x)));
            }


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
                        if (UnitQueueValiable.Count > 0)
                        {
                            unit = UnitQueueValiable.Dequeue();
                        }
                        block.Init(unit == null ? CodeNameType.Blue : unit.GetCodeNameType(), room.transform);
                    }
                }
                roomSpawnerManager.DecreaseRoomConfigWeight(roomConfig, -1);
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
                        if (cell.GetDirection() != cellNeighbor.Key * (cellNeighbor.Value.GetCellType == BlockType.Exdoor ? -1 : 1))
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
                neighbors.Add(new Vector3(directions[i, 0], 0, directions[i, 1]), neighbor);
                Debug.Log($"Cell: {cell.name} has neighbor: {neighbor.name}");
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
    #endregion
    #region PREVIEW
    private void SetBlockPreviewToCell(Dictionary<Block, Cell> _blockRaycastedToCellDict)
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
    #region CELL API
    private void SetPlaceableCellCondition(Cell cell)
    {
        if (cell == null) return;
        if (!cell.IsOccupier()) return;
        IOccupier lastedOccupier = cell.GetLastOccupier();
        if (lastedOccupier == null) return;
        {
            if (lastedOccupier.GetOccupierType() != BlockType.Normal.ToString())
            {
                BlockType blockTypeAvaliableOnCell = (lastedOccupier.GetOccupierType() == BlockType.Exdoor.ToString() ? BlockType.Indoor : BlockType.Exdoor);

                Cell.PlaceableCondition placeableCondition = new Cell.PlaceableCondition(blockTypeAvaliableOnCell, -lastedOccupier.GetDirection());
                cell.SetPlaceableCondition(cell, placeableCondition);

                if (lastedOccupier.GetOccupierType() == BlockType.Indoor.ToString())
                {

                    BlockType exceptblockTypeOnCell = (lastedOccupier.GetOccupierType() == BlockType.Exdoor.ToString() ? BlockType.Exdoor : BlockType.Indoor);
                    Cell cellNeighbor = GetCellNeighborByDirection(cell, lastedOccupier.GetDirection());

                    Cell.PlaceableWithNeighborCondition placeableNeighborAction = new Cell.PlaceableWithNeighborCondition(exceptblockTypeOnCell, cell);
                    cellNeighbor.SetPlaceableCondition(cell, placeableNeighborAction);
                }
            }
            {
                if (lastedOccupier.GetOccupierType() != BlockType.Exdoor.ToString())
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
    #endregion
    #endregion
    #region STAGE API
    public void LoadInitialLevel(StageManager stageManager)
    {
        GameObject level = stageManager.GetCurrentLevelPrefab();
        if (level != null)
        {
            LoadLevel(level);
        }
    }

    private void LoadLevel(GameObject level)
    {
        if (level != null)
        {
            LevelContainer levelContainer = _gameView.SpawnLevel(level, Vector3.zero, Quaternion.identity);

            if (levelContainer != null)
            {
                _levelContainer = levelContainer;
                InitLevelFromView(levelContainer.GetRoomConfigContainer, levelContainer.GetRoomStaticContainer, levelContainer.GetArchitectureContainer, levelContainer.GetGateWayContainer, levelContainer.GetRoomSpawnerPoints);
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
    public List<Block> GetBlockPlacedEmpty()
    {
        return _blockPlacedOnCell.Keys.Where(x => !x.IsFullOccupier()).ToList();
    }
    public Queue<Unit> GetAllUnitQueue(List<Gateway> gateWays)
    {
        Dictionary<int, List<Unit>> indexUnitGroups = new Dictionary<int, List<Unit>>();
        foreach (var gateWay in gateWays)
        {
            int count = 0;
            foreach (var unit in gateWay.GetUnitQueue.ToList())
            {
                if (!indexUnitGroups.ContainsKey(count))
                {
                    indexUnitGroups.Add(count, new List<Unit> { unit });
                }
                else
                {
                    indexUnitGroups[count].Add(unit);
                }
                count++;
            }
        }
        Queue<Unit> unitQueue = new Queue<Unit>();
        foreach (var indexUnitGroupValueKey in indexUnitGroups)
        {
            var indexUnitGroup = indexUnitGroupValueKey.Value;
            foreach (var unit in indexUnitGroup)
            {
                unitQueue.Enqueue(unit);
            }
        }

        return unitQueue;
    }
    public Queue<Unit> GetAllUnitQueueRange(Queue<Unit> unitQueue, int range)
    {
        if (range > unitQueue.Count)
        {
            range = unitQueue.Count;
        }
        return new Queue<Unit>(unitQueue.Take(range));
    }
    public Queue<Unit> GetAllUnitSortedQueue(Queue<Unit> unitQueue)
    {
        Queue<Unit> sortedUnits = new Queue<Unit>();
        Dictionary<string, Queue<Unit>> unitSortByCodeName = new Dictionary<string, Queue<Unit>>();

        foreach (Unit unit in unitQueue)
        {
            string codeName = unit.GetCodeName();

            if (unitSortByCodeName.ContainsKey(codeName))
            {
                unitSortByCodeName[codeName].Enqueue(unit);
            }
            else
            {
                unitSortByCodeName[codeName] = new Queue<Unit>();
                unitSortByCodeName[codeName].Enqueue(unit);
            }
        }
        foreach (var unitTheSameCodeName in unitSortByCodeName)
        {
            foreach (var unit in unitTheSameCodeName.Value)
            {
                sortedUnits.Enqueue(unit);
            }
        }
        return sortedUnits;
    }
    public int GetAllUnitQueueCount(List<Gateway> _gateWays)
    {
        return _gateWays.SelectMany(gw => gw.GetUnitQueue).ToQueue().Count;
    }
    public int GetACount()
    {
        int unitCount = GetAllUnitQueueCount(_gateWays);
        List<Block> blockPlacedEmpties = GetBlockPlacedEmpty();
        if (blockPlacedEmpties == null) return 0;
        int blockEmptyCount = blockPlacedEmpties.Count;
        int count = unitCount - blockEmptyCount;
        return count;
    }
    public int GetBCount()
    {
        List<Block> blockPlacedEmpties = GetBlockPlacedEmpty();
        if (blockPlacedEmpties == null) return 0;
        int cellPlacableCount = _architecture.GetCells.Count - blockPlacedEmpties.Count;

        return cellPlacableCount;
    }
    #endregion
}
[Serializable]
public class roomRaycastCheck
{
    public GameObject RoomGO;
    public List<BlockRaycast> BlockRaycasts;

    public roomRaycastCheck(GameObject roomGO)
    {
        RoomGO = roomGO;
        BlockRaycasts = new List<BlockRaycast>();
    }
    public void AddBlockPoint(BlockRaycast blockRaycast)
    {
        if (!BlockRaycasts.Contains(blockRaycast))
        {
            BlockRaycasts.Add(blockRaycast);
        }
    }
    public List<BlockRaycast> GetBlockRaycasting() => BlockRaycasts.Where(x => x.IsFree == false).ToList();
    public BlockRaycast GetPreeBlockRaycast()
    {
        BlockRaycast blockFree = BlockRaycasts.FirstOrDefault(x => x.IsFree == true);
        if (blockFree == null)
        {
            GameObject point = new GameObject("Block Point");
            blockFree = new BlockRaycast(point);

            AddBlockPoint(blockFree);
        }
        blockFree.Point.transform.parent = RoomGO.transform;
        return blockFree;
    }
    public void AssigBlockPoint(BlockRaycast blockFree, Vector3 localPoint)
    {
        if (blockFree != null)
        {
            blockFree.Point.transform.localPosition = localPoint;
        }
    }
    public void AssigRoomPoint(Vector3 Point)
    {
        RoomGO.transform.position = Point;
    }
    public void ClearRaycast()
    {
        foreach (var blockRaycast in BlockRaycasts)
        {
            blockRaycast.ClearRaycast();
        }
    }
}
public class BlockRaycast
{
    public GameObject Point;
    public Block BlockOrigin;
    public Cell TargetCell;
    public bool IsFree;
    public BlockRaycast(GameObject point)
    {
        Point = point;
        IsFree = true;
        BlockOrigin = null;
        TargetCell = null;
    }
    public bool IsRaycastToTarget() => TargetCell != null;
    public void AssignBlockOrigin(Block block)
    {
        BlockOrigin = block;
        IsFree = false;
    }
    public void AssignTargetCellOrigin(Cell cell)
    {
        TargetCell = cell;
    }
    public void ClearRaycast()
    {
        BlockOrigin = null;
        TargetCell = null;
        IsFree = true;
    }
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
    Free,
    Freeze
}
public enum CodeNameType
{
    Blue,
    Red,
    Yellow,
    Green,
    Purple
}