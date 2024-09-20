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
    [Inject] private Context _context;
    [Inject] private Timer _timer;
    [Inject] private CoroutineHelper _coroutineHelper;
    private RoomSort2DGameView _gameView;
    public SaveGameSystem SaveGameSystem;
    private PathFindingService _pathFindingService;

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
    private Dictionary<Unit, bool> _unitOnDestinationDict;
    private List<Gateway> _gateWays;
    private StageManager _stageManager;
    private LevelContainer _levelContainer;
    private RoomSpawnerManager _roomSpawnerManager;
    private roomRaycastCheck _roomRaycastCheck;
    public static System.Action<int> _checkJumpResult = delegate { };
    public static System.Action<int, int> _OnUnitQueueUpdate = delegate { };
    public static System.Action<bool> _OnStuckRoom = delegate { };
    private int _lauchCount;
    private int _jumpCount;
    private bool hasInit;
    private int _initTotalUnit;
    private int _totalUnitComplete;
    private int _minMinWinCondition;
    private RaycastMode _raycastMode;

    #endregion
    #region FLOW
    public void Initialize()
    {
        _timer.POST_TICK += PostTick;
        _timer.FIXED_TICK += FixedTick;
        if (!hasInit)
        {
            SaveGameSystem = new SaveGameSystem();
            SaveGameSystem.Load();
            _context.Install(SaveGameSystem);
            _pathFindingService = new PathFindingService(_gameView.GetBlockDirPathFinding);
            _context.Install(_pathFindingService);
            _context.ApplyInstall();
            _stageManager = new StageManager();
            LoadInitialLevel(_stageManager);

            GameObject roomGO = new GameObject("Room GO");
            _roomRaycastCheck = new roomRaycastCheck(roomGO);

        }
        _raycastMode = RaycastMode.NORMAL;
        _OnUnitQueueUpdate += _gameView.HandleUpdateUnitOverviewText;
        _checkJumpResult += _gameView.HandleUpdateJumpCount;
        _OnStuckRoom += _gameView.HandleShowWarning;
        hasInit = true;

    }
    private void PostTick()
    {
        if (!hasInit) return;
        switch (_raycastMode)
        {
            case RaycastMode.NORMAL:
                CheckRaycast();
                break;
            case RaycastMode.JUMP:
                CheckRaycastJump();
                break;
            default:
                CheckRaycast();
                break;
        }
    }
    private void FixedTick()
    {

    }
    public void Dispose()
    {
        _timer.POST_TICK -= PostTick;
        _timer.FIXED_TICK -= FixedTick;
        _OnUnitQueueUpdate -= _gameView.HandleUpdateUnitOverviewText;
        _checkJumpResult -= _gameView.HandleUpdateJumpCount;
        _OnStuckRoom -= _gameView.HandleShowWarning;

    }
    private void GameEnd()
    {
        ClearAllRoom(_rooms, _architecture);
        if (CheckGameResult())
        {
            //Game win
            GameWin();
        }
        else
        {
            //Game lose
            Debug.Log("Game Over");
            _gameView.GetGameOverPopup.SetActive(true);
        }
    }
    private void GameWin()
    {
        Debug.Log("Game WINN");
        OnLevelComplete();
    }
    private bool CheckEndGame()
    {
        int count = GetAllUnitQueueCount(_gateWays);
        if (count <= 0) return true;
        if (_lauchCount <= 0) return true;

        return false;
    }
    private bool CheckGameResult()
    {
        int count = GetAllUnitQueueCount(_gateWays);
        if (count <= 0) return true;
        if (count <= _minMinWinCondition)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    List<Room> _roomStatics;
    private void InitRoomFromView(Transform roomConfigCotainer, Transform roomStaticCotainer, Transform architectureContainer, Transform gateWayContainer, Transform[] roomSpawnerPoints, int launchCount, int jumpCount, int minUnitWinCondition)
    {
        _lauchCount = launchCount;
        _jumpCount = jumpCount;
        _minMinWinCondition = minUnitWinCondition;
        _initTotalUnit = GetAllUnitQueueCount(_gateWays);
        _gameView.InitlaunchButton(_lauchCount, HandleLauch);
        _gameView.InitlaunchAllButton(_lauchCount, HandleLauchAll);
        _gameView.InitJumplButton(_jumpCount, _checkJumpResult, HandleJump);
        _gameView.InitMinUnitWinCondition(_minMinWinCondition);
        //_gameView.InitEventUpdateUnitOverviewText(_OnUnitQueueUpdate);

        _totalUnitComplete = 0;
        _OnUnitQueueUpdate?.Invoke(_totalUnitComplete, _initTotalUnit);
        if (_roomStatics.Count > 0)
        {
            PlaceRooms(_roomStatics);
        }
        SpawnRooms(_roomSpawnerManager, roomSpawnerPoints, roomConfigCotainer, GetACount(), GetBCount(), GetAllUnitQueue(_gateWays));

    }
    public void InitRoomFromSave(SaveGameData saveGameData)
    {
        _lauchCount = saveGameData.LaunchCount;

        foreach (var roomSave in saveGameData.RoomPlacedSaveGames)
        {
            string roomCodeNameConfig = GameHelper.GetStringBeforeCharacter(roomSave.Id, '_');
            Room roomConfig = _roomSpawnerManager.GetRoomConfig(roomCodeNameConfig);
            if (roomConfig == null) continue;
            Vector3 spawnPoint = new Vector3(roomSave.X, 0, roomSave.Z);

            SpawnRoom(roomConfig, spawnPoint, _levelContainer.GetRoomConfigContainer,
               action: (room) =>
               {
                   room.SetId(roomSave.Id);
                   room.gameObject.SetActive(true);
                   room.SetMoveableState(true);

                   for (int i = 0; i < room.GetBlocks.Count; i++)
                   {
                       Block blockConfig = roomConfig.GetBlocks[i];
                       Block block = room.GetBlocks[i];
                       string id = blockConfig.name;
                       BlockSaveGame blockSave = roomSave.FindBlockSave(id);
                       if (blockSave == null) continue;
                       block.Init(room.transform, blockSave.CodeName);
                   }
                   PlaceRoom(room);
                   InitNeighborBlockFromRoom(room);
               });
        }
    }
    private void InitGateWayFromView(Transform gateWayContainer)
    {
        _gateWays = new List<Gateway>();
        int count = 0;
        foreach (Transform child in gateWayContainer)
        {
            Gateway gateWay = child.GetComponent<Gateway>();
            if (gateWay != null)
            {
                gateWay.Init(_injector);
                gateWay.name = $"Gateway: {count}";
                _gateWays.Add(gateWay);
                gateWay.OnCompleteWay += HandleGateWayComplete;
                count++;
            }
        }
    }
    private void InitGatewayFromSave(SaveGameData saveGameData)
    {
        foreach (var gatway in saveGameData.GatewaySaveGames)
        {

        }
    }
    private void InitLevel(Transform roomConfigCotainer, Transform architectureContainer, Transform gateWayContainer, Transform[] roomSpawnerPoints, Transform roomStaticCotainer)
    {
        _gameView.Init();
        _blockPlacedOnCell = new Dictionary<Block, Cell>();
        _rooms = new List<Room>();
        InitGateWayFromView(gateWayContainer);
        InitArchitectureFromView(architectureContainer);
        InitUnitOnDestionation();

        List<Room> rooms = InitRoomConfigFromView(roomConfigCotainer);
        if (rooms == null) return;
        RoomSpawnerManager roomSpawnerManager = new RoomSpawnerManager(rooms);
        InitRoomSpawner(roomSpawnerManager);
        _roomStatics = InitRoomStaticFromView(roomStaticCotainer);

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
        int count = 0;
        foreach (Transform child in roomContainer)
        {
            Room room = child.GetComponent<Room>();
            if (room != null)
            {
                room.Init();
                room.gameObject.SetActive(false);
                roomsConfig.Add(room);
                room.SetId($"room ingame:{count}");
                count++;
            }
        }
        return roomsConfig;
    }
    private List<Room> InitRoomStaticFromView(Transform roomStaticContainer)
    {
        List<Room> roomsStatic = new List<Room>();
        int count = 0;

        foreach (Transform child in roomStaticContainer)
        {
            if (child.gameObject.activeSelf == false)
            {
                continue;
            }
            Room room = child.GetComponent<Room>();
            if (room == null) continue;

            room.Init();
            room.gameObject.SetActive(false);
            roomsStatic.Add(room);
            _rooms.Add(room);
            room.OnCompleteRoom += CompleteRoom;
            room.SetId($"room static:{count}");

            List<Block> blocks = room.GetBlocks;
            foreach (var block in blocks)
            {
                block.Init(room.transform);
            }
            InitNeighborBlockFromRoom(room);
            count++;
        }
        return roomsStatic;
    }
    public void InitUnitOnDestionation()
    {
        _unitOnDestinationDict = new Dictionary<Unit, bool>();
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
    #region RAYCAST PROPERTIES
    Vector3 clickOffset;
    private RaycastHit[] _raycastHits = new RaycastHit[1];
    private bool _isValidPlace;
    List<Block> blocks;
    Unit _currentUnitJump;
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
                        if (!CheckRoomMoveble(room)) return;

                        //Offset
                        Vector3 worldPosition = _gameView.GetMainCam.ScreenToWorldPoint(mousePosition);
                        clickOffset = worldPosition - room.transform.position;

                        _currentRoomInteract = room;

                        blocks = GetlistBlock(_currentRoomInteract);
                        if (blocks == null) return;
                        //If room is placed state
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

                PlaceBlockRaycastedToCell(_currentRoomInteract, _blockRaycastedToCellDict);
                RemoveRoomSpawner(_currentRoomInteract);
                CheckStuckRoom();
                if (CheckSpawnRooms())
                {
                    bool spawnSuccess = SpawnRooms(_roomSpawnerManager, _levelContainer.GetRoomSpawnerPoints, _levelContainer.GetRoomConfigContainer, GetACount(), GetBCount(), GetAllUnitQueue(_gateWays));
                    if (spawnSuccess)
                    {
                        CheckStuckRoom();
                    }
                }
                else
                {
                   
                }
                SaveRoom(_currentRoomInteract);
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
    private void CheckRaycastJump()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Vector3 mousePosition = Input.mousePosition;
            Ray ray = _gameView.GetMainCam.ScreenPointToRay(mousePosition);

            if (Physics.RaycastNonAlloc(ray, _raycastHits, 100f, _gameView.GetBlockLayerMask) > 0)
            {
                RaycastHit hit = _raycastHits[0];
                Block blockHit = hit.collider.GetComponentInParent<Block>();
                if (blockHit != null)
                {
                    if (_currentUnitJump == null) return;
                    List<Block> blocksPlacable = GetBlocksPlacable();
                    blocksPlacable = blocksPlacable.Where(x => x.GetCodeName() == _currentUnitJump.GetCodeName()).ToList();
                    if (!blocksPlacable.Contains(blockHit)) return;
                    if (blockHit.IsOccupier()) return;

                    blockHit.SetOccupier(_currentUnitJump);
                    _currentUnitJump.JumpTo(blockHit.transform.position);
                    foreach (var gateway in _gateWays)
                    {
                        var unitQueue = gateway.GetUnitQueue;

                        if (unitQueue.TryPeek(out Unit result))
                        {
                            if (result == _currentUnitJump)
                            {
                                DequeueUnit(gateway);
                                gateway.UpdateUnitQueuePosition();
                                break;
                            }
                        }
                    }
                    AddUnitOnMoving(_currentUnitJump);
                    _currentUnitJump.OnDestination += HandleUnitOndestination;
                    foreach (var block in blocksPlacable)
                    {
                        block.ActiveOutline(false);
                    }
                    blockHit.ActiveOutline(false);
                    SwitchRayCastMode(RaycastMode.NORMAL);
                    _jumpCount--;
                    _checkJumpResult?.Invoke(_jumpCount);
                }
            }
        }
    }
    public void SwitchRayCastMode(RaycastMode newRayCastMode)
    {
        _raycastMode = newRayCastMode;
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
    public void PlaceBlockRaycastedToCell(Room room, Dictionary<Block, Cell> _blockCheckRaycastDict)
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
            PlaceBlockToCell(block, cell);
            if (!_rooms.Contains(room))
            {
                _rooms.Add(room);
            }
        }
        CheckUnitMoveToBlock(_gateWays);
        SetPlaceableCellCondition(_blockCheckRaycastDict.Values.ToList());


    }
    public void PlaceBlockToCell(Block block, Cell cell)
    {
        if (!_blockPlacedOnCell.ContainsKey(block))
        {
            _blockPlacedOnCell.Add(block, cell);
        }
        else
        {
            _blockPlacedOnCell[block] = cell;
        }
    }
    public void RemoveBlockPlacable(Block block)
    {
        if (_blockPlacedOnCell.ContainsKey(block))
        {
            Cell cell = _blockPlacedOnCell[block];
            _blockPlacedOnCell.Remove(block);
        }
    }
    public void PlaceRooms(List<Room> rooms)
    {
        foreach (var room in rooms)
        {
            PlaceRoom(room);
        }
    }
    public void PlaceRoom(Room room)
    {
        room.gameObject.SetActive(true);
        List<Block> blocks = room.GetBlocks;
        if (blocks != null)
        {
            InitBlockRaycastToCell(blocks);
            foreach (var block in blocks)
            {
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
                PlaceBlockRaycastedToCell(room, _blockRaycastedToCellDict);
                room.ChangePlaceableState(PlaceableState.Placed);
                //SaveRoom(room);
            }

        }
    }
    private void CompleteRoom(Room room)
    {
        List<Block> blocks = room.GetBlocks;
        if (blocks == null) return;
        foreach (var block in blocks)
        {
            Cell cellConaintBlock = _architecture.GetCells.FirstOrDefault(x => x.GetOccupier().Contains(block));

            if (cellConaintBlock != null)
            {
                cellConaintBlock.ClearOccupiers();
            }
        }
        room.OnComplete();
        CheckUnitMoveToBlock(_gateWays);
        if (_rooms.Contains(room))
        {
            _rooms.Remove(room);
        }
        CheckStuckRoom();
    }
    List<BlockRaycast> _blockRaycastings;
    public bool CheckCanPlaceRoom(Room room, Architecture architecture)
    {
        _roomRaycastCheck.ClearRaycast();

        List<Block> blocks = GetlistBlock(room);
        Block pivotBlock = blocks.First();
        foreach (var block in blocks)
        {
            BlockRaycast blockRaycastFree = _roomRaycastCheck.GetPreeBlockRaycast();
            blockRaycastFree.AssignBlockOrigin(block);
            _roomRaycastCheck.AssigBlockPoint(blockRaycastFree, block.transform.localPosition);
        }
        _blockRaycastings = _roomRaycastCheck.GetBlockRaycasting();
        _roomRaycastCheck.AssigRoomPoint(pivotBlock.transform.position);
        //Vector3 offSet = pivotBlock.transform.position - _roomRaycastCheck.RoomGO.transform.position;
        Vector3 offSet = pivotBlock.transform.localPosition;
        List<Cell> cells = architecture.GetCells;

        if (_blockRaycastings != null)
        {
            foreach (var cell in cells)
            {
                if (cell.IsOccupier()) continue;
                InitBlockRaycastToCell(blocks);

                //Change targetPoint
                Vector3 targetPoint = cell.transform.position - offSet;
                _roomRaycastCheck.AssigRoomPoint(targetPoint);
                foreach (var blockRaycast in _blockRaycastings)
                {
                    Ray blockRay = new Ray(blockRaycast.Point.transform.position + Vector3.up * 5, Vector3.down);
                    Cell cellSlot = null;
                    if (Physics.RaycastNonAlloc(blockRay, _raycastHits, 10f, _gameView.GetCellLayerMask) > 0)
                    {
                        cellSlot = _raycastHits[0].collider.GetComponentInParent<Cell>();
                        if (cellSlot == null)
                        {
                            break;
                        }
                        blockRaycast.AssignTargetCellOrigin(cellSlot);
                        SetBlockRaycastToCell(blockRaycast.BlockOrigin, cellSlot);

                    }

                }
                if (CheckBlockRaycastPlaceable(_blockRaycastedToCellDict))
                {
                    return true;
                }
            }
        
        }

        return false;
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
            RemoveBlockPlacable(block);

        }
    }
    private void ClearAllRoom(List<Room> rooms, Architecture architecture)
    {
        foreach (var room in rooms)
        {
            room.OnComplete();
        }
        rooms.Clear();
        List<Cell> cells = _architecture.GetCells;
        foreach (var cell in cells)
        {
            if (cell.IsOccupier())
            {
                cell.ClearOccupiers();
            }
        }
        _blockPlacedOnCell.Clear();
        _rooms.Clear();
        _OnStuckRoom?.Invoke(false);
    }
    private bool CheckRoomMoveble(Room room)
    {
        if (room.GetPlaceableState == PlaceableState.Freeze) return false;
        if (!room.GetMoveableState) return false;
        return true;
    }
    public List<Block> GetlistBlock(Room targetRoom)
    {
        if (targetRoom == null) return null;
        return targetRoom.GetBlocks;
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
            if (!cell.IsOccupier()) continue;
            if (block.IsOccupier()) continue;
            if (block.GetData.CodeName != codeName) continue;
            cellPath = _pathFindingService.FindPath(startCell, cell);

            if (cellPath.Count == 0) continue;
            return cellPath.Count > 0 ? block : null;
        }
        return null;

    }
    private void InitNeighborBlockFromRoom(Room room)
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
    #region SPAWN ROOM
    private bool CheckSpawnRooms()
    {
        if (_roomSpawner.All(x => x.Value == null))
        {
            if (CheckEndGame()) return false;
            return true;
        }
        return false;   
    }
    private void RemoveRoomSpawner(Room targetRoom)
    {
        Vector3 currentRoomSpawnerPoint = _roomSpawner.FirstOrDefault(x => x.Value == _currentRoomInteract).Key;
        if (currentRoomSpawnerPoint != null)
        {
            _roomSpawner[currentRoomSpawnerPoint] = null;
        }
    }
    private void CheckStuckRoom()
    {
        foreach (var roomSpawnPair in _roomSpawner)
        {
            Room room = roomSpawnPair.Value;
            if (room == null)
            {
                continue;
            }
            bool canPlaceRoom = CheckCanPlaceRoom(room, _architecture);
            if (canPlaceRoom)
            {
                _OnStuckRoom.Invoke(false);
                return;
            }
        }
         _OnStuckRoom.Invoke(true);
    }
    public bool SpawnRooms(RoomSpawnerManager roomSpawnerManager, Transform[] spawnerPoints, Transform parent, int countA, int countB, Queue<Unit> UnitQueueValiable)
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
            int allRoomBlockCount = roomPrefab1.GetBlockCount + roomPrefab2.GetBlockCount + roomPrefab3.GetBlockCount + 10;
            UnitQueueValiable = GetAllUnitQueueRange(UnitQueueValiable, allRoomBlockCount);
            UnitQueueValiable = GameHelper.ShuffleQueue(UnitQueueValiable);
            //UnitQueueValiable = GetAllUnitSortedQueue(UnitQueueValiable);

            List<Block> blocksPlacable = GetBlocksPlacable();
            if (blocksPlacable.Count > 0)
            {
                foreach (var block in blocksPlacable)
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
                SpawnRoom(roomConfig, point, parent,
                    action: (room) =>
                    {
                        room.SetId(room.GetId() + "_" + Guid.NewGuid().ToString());
                        room.gameObject.SetActive(true);
                        room.SetMoveableState(true);
                        if (!_roomSpawner.ContainsKey(point)){
                            _roomSpawner.Add(point, room);

                        }
                        else
                        {
                            _roomSpawner[point] = room;
                        }
                        foreach (var block in room.GetBlocks)
                        {
                            Unit unit = null;
                            if (UnitQueueValiable.Count > 0)
                            {
                                unit = UnitQueueValiable.Dequeue();
                            }
                            block.Init(room.transform, unit == null ? CodeNameType.Blue.ToString() : unit.GetCodeNameType().ToString());
                        }
                        InitNeighborBlockFromRoom(room);
                    });
                roomSpawnerManager.DecreaseRoomConfigWeight(roomConfig, -1);
                index++;
            }
            return true;
        }
        return false;
    }
    private void SpawnRoom(Room roomConfig, Vector3 point, Transform parent, System.Action<Room> action = null)
    {
        Room room = _gameView.SpawnRoom(roomConfig.gameObject, point, Quaternion.identity, parent);
        if (room != null)
        {
            room.Init();
            room.OnCompleteRoom += CompleteRoom;
            action?.Invoke(room);
        }
    }
    #endregion 
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
                //Debug.Log($"Cell: {cell.name} has neighbor: {neighbor.name}");
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
    #region UNIT API
    private void CheckUnitMoveToBlock(List<Gateway> _gateWays)
    {
        foreach (var gateway in _gateWays)
        {
            bool canDequeueUnit = false;
            gateway.DequeueUnitLoop((unit) =>
            {
                if (unit != null)
                {
                    Block block = GetBlockValiable(gateway.GetConnectedCell, gateway.GetDirection(), unit.GetOccupierType(), out List<Cell> cellPath);
                    if (block == null)
                    {
                        return false;
                    }
                    canDequeueUnit = true;
                    block.SetOccupier(unit);
                    List<Vector3> pathPositions = cellPath.Select(cell => cell.transform.position).ToList();
                    unit.MoveTo(pathPositions, onDestination: true);
                    DequeueUnit(gateway);
                    AddUnitOnMoving(unit);
                    unit.OnDestination += HandleUnitOndestination;
                    return true;

                }
                return false;
            });
            if (canDequeueUnit)
            {
                gateway.UpdateUnitQueuePosition();
            }

        }

    }
    public void AddUnitOnMoving(Unit unit)
    {
        if (!_unitOnDestinationDict.ContainsKey(unit))
        {
            _unitOnDestinationDict.Add(unit, false);

        }
        else
        {
            _unitOnDestinationDict[unit] = false;
        }
    }
    public void SetUnitOnDestination(Unit unit)
    {
        if (!_unitOnDestinationDict.ContainsKey(unit)) return;
        _unitOnDestinationDict[unit] = true;
    }
    public bool CheckAllUnitOndestination()
    {
        return _unitOnDestinationDict.All(x => x.Value == true);
    }
    public List<Block> GetBlocksPlacable()
    {
        return _blockPlacedOnCell.Keys.Where(x => !x.IsFullOccupier()).ToList();
    }
    public List<Cell> GetCellPlaced()
    {
        return _architecture.GetCells.Where(x => x.IsOccupier()).ToList();
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
    public int GetAllUnitQueueCount(List<Gateway> _gateWays)
    {
        return _gateWays.SelectMany(gw => gw.GetUnitQueue).ToQueue().Count;
    }
    public void DequeueUnit(Gateway gateway)
    {
        Unit unit = gateway.DequeueUnit();
        if (unit == null) return;
        _totalUnitComplete++;
        _OnUnitQueueUpdate?.Invoke(_totalUnitComplete, _initTotalUnit);
    }
    public int GetACount()
    {
        int unitCount = GetAllUnitQueueCount(_gateWays);
        List<Block> blocksPlacable = GetBlocksPlacable();
        if (blocksPlacable == null) return 0;
        int blockEmptyCount = blocksPlacable.Count;
        int count = unitCount - blockEmptyCount;
        return count;
    }
    public int GetBCount()
    {
        List<Block> blocksPlacable = GetBlocksPlacable();
        if (blocksPlacable == null) return 0;
        int cellPlacableCount = _architecture.GetCells.Count - blocksPlacable.Count;

        return cellPlacableCount;
    }
    #endregion
    #region SAVE API
    public void SaveRoom(Room room)
    {
#if SAVEGAME
        //save Game
        RoomSaveGame roomSaveGame = new RoomSaveGame();
        roomSaveGame.CloneDataFromOriginal(room);
        SaveGameSystem.AddRoomSave(roomSaveGame);
        SaveGameSystem.SaveGameData();
#endif
    }
    #endregion
    #region HANDLE
    private int HandleLauch()
    {
        if (_lauchCount <= 0)
        {

            return -1;
        }
        bool result = false;
        foreach (var room in _rooms.ToList())
        {
            if (room.IsComplete())
            {
                result = true;
                CompleteRoom(room);
            }
        }

        if (result)
        {
            _lauchCount--;
            if (_lauchCount <= 0)
            {
                if (CheckAllUnitOndestination())
                {
                    GameEnd();
                }
            }

            return _lauchCount;

        }
        else
        {
            return -1;
        }
    }
    private int HandleLauchAll()
    {
        if (_lauchCount <= 0)
        {
            return -1;
        }
        List<Cell> cellsPlacable = GetCellPlaced();
        if (cellsPlacable.Count == 0) return -1;
        if (!CheckAllUnitOndestination()) return -1;
        bool result = true;
        ClearAllRoom(_rooms, _architecture);
        if (result)
        {
            _lauchCount--;
            if (CheckEndGame())
            {
                GameEnd();
            }
            return _lauchCount;
        }
        else
        {
            return -1;
        }
    }
    private bool HandleJump()
    {
        if (_jumpCount <= 0)
        {
            return false;
        }
        Queue<Unit> unitQueue = GetAllUnitQueue(_gateWays);
        if (unitQueue == null) return false;
        Unit firstUnit = unitQueue.Dequeue();
        if (firstUnit == null) return false;

        List<Block> blocksPlacable = GetBlocksPlacable();
        blocksPlacable = blocksPlacable.Where(x => x.GetCodeName() == firstUnit.GetCodeName()).ToList();

        if (blocksPlacable == null || blocksPlacable.Count == 0) return false;
        _currentUnitJump = firstUnit;
        SwitchRayCastMode(RaycastMode.JUMP);
        foreach (var block in blocksPlacable)
        {
            block.ActiveOutline(true);
        }
        return true;

    }
    private void HandleUnitOndestination(Unit unit)
    {
        SetUnitOnDestination(unit);
        if (!CheckEndGame()) return;
        if (!CheckAllUnitOndestination()) return;
        GameEnd();
    }
    private void HandleGateWayComplete()
    {
    }
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
                InitLevel(levelContainer.GetRoomConfigContainer, levelContainer.GetArchitectureContainer, levelContainer.GetGateWayContainer, levelContainer.GetRoomSpawnerPoints, levelContainer.GetRoomStaticContainer);
                //Save Game
                if (SaveGameSystem.GetGameData.RoomPlacedSaveGames == null)
                {
                    //SaveGameSystem.GetGameData.RoomPlacedSaveGames = new List<RoomSaveGame>();
                    InitRoomFromView(levelContainer.GetRoomConfigContainer, levelContainer.GetRoomStaticContainer,
                        levelContainer.GetArchitectureContainer, levelContainer.GetGateWayContainer,
                        levelContainer.GetRoomSpawnerPoints,
                        levelContainer.GetLauchCount, levelContainer.GetJumpCount, levelContainer.GetMinUnitWinCondition);

                }
                else
                {
                    InitRoomFromSave(SaveGameSystem.GetGameData);
                }
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
    public List<BlockRaycast> GetBlockRaycasting() => BlockRaycasts.Where(x => x.BlockOrigin != null).ToList();
    public BlockRaycast GetPreeBlockRaycast()
    {
        BlockRaycast blockFree = BlockRaycasts.FirstOrDefault(x => x.BlockOrigin == null);
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
    public BlockRaycast(GameObject point)
    {
        Point = point;
        BlockOrigin = null;
        TargetCell = null;
    }
    public bool IsRaycastToTarget() => TargetCell != null;
    public void AssignBlockOrigin(Block block)
    {
        BlockOrigin = block;
        Point.name = block.name;
    }
    public void AssignTargetCellOrigin(Cell cell)
    {
        TargetCell = cell;
    }
    public void ClearRaycast()
    {
        BlockOrigin = null;
        TargetCell = null;
        Point.name = "Block raycast free";
    }
}
public enum RaycastMode
{
    NORMAL,
    JUMP
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