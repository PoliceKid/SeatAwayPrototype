using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using static Cell;
using Unity.VisualScripting;
public class Cell :MonoBehaviour, IOccupierContainer<IOccupier> 
{
    #region PROPERTIES
    [SerializeField] SpriteRenderer _spRd;
    [SerializeField] SpriteMask _indoorSrpiteMask;
    [SerializeField] SpriteMask _exdoorSrpiteMask;
    [SerializeField] private BlockType _cellType;
    [SerializeField] GameObject _wallLeft;
    [SerializeField] GameObject _wallRight;
    [SerializeField] GameObject _wallTop;
    [SerializeField] GameObject _wallBottom;
    #endregion
    #region CURRENT DATA
    //Current data
    private Data _data;
    private Color initColor;
    private IOccupier _currentOccupierRaycast;
    public int ConditionCount;
    public int ConditionCountStatic;
    public BlockType GetCellType => _cellType;
    #endregion
    public void Init()
    {
        _data = new Data();
        initColor = _spRd.color;
        ApplyCellType(_cellType);
        name = $"Pos X: {transform.position.x}, Pos Z: {transform.position.z}";
        InitWall();
    }
    public void InitWall()
    {
        if (GetCellType != BlockType.Normal) return;
        _wallLeft.gameObject.SetActive(true);
        _wallRight.gameObject.SetActive(true);
        _wallTop.gameObject.SetActive(true);
        _wallBottom.gameObject.SetActive(true);
    }
    private static Vector3[] dir = new Vector3[] { Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
    public void SetWallActive(Vector3 dirNotFaceWall,Cell neighbor)    
    {
        bool canSetNeighborWall = neighbor.GetCellType == BlockType.Normal;
        dirNotFaceWall *= GetDirection().z;
        if (dirNotFaceWall.normalized == Vector3.left && canSetNeighborWall)
        {
            _wallLeft.gameObject.SetActive(false);
        }
        if (dirNotFaceWall.normalized == Vector3.right && canSetNeighborWall)
        {
            _wallRight.gameObject.SetActive(false);
        }
        if (dirNotFaceWall.normalized == Vector3.forward && canSetNeighborWall)
        {
            _wallTop.gameObject.SetActive(false);
        }
        if (dirNotFaceWall.normalized == Vector3.back && canSetNeighborWall)
        {
            _wallBottom.gameObject.SetActive(false);
        }
    }
    public void InitStaticCondition()
    {
        var dirNeighBors = _data.Neighbors.Keys.ToList();
        var dirFaceWall = dir.Where(x => !dirNeighBors.Contains(x)).ToList();
        foreach (var dir in dirFaceWall)
        {
            if (_cellType == BlockType.Normal)
            {
                ExceptPlaceableCondition exceptPlaceableAction = new ExceptPlaceableCondition(BlockType.Indoor, dir);
                SetStaticPlaceableCondition(exceptPlaceableAction); 
            }
        }
        if (_cellType != BlockType.Normal)
        {
            PlaceableCondition placeableCondition = new PlaceableCondition(_cellType, GetDirection());
            SetStaticPlaceableCondition(placeableCondition);
        }     
    }
    public void ApplyCellType(BlockType cellType)
    {
        switch (cellType)
        {
            case BlockType.Normal:
                _spRd.maskInteraction = SpriteMaskInteraction.None;
                _indoorSrpiteMask.gameObject.SetActive(false);
                _exdoorSrpiteMask.gameObject.SetActive(false);
                break;
            case BlockType.Indoor:
                _spRd.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
                _indoorSrpiteMask.gameObject.SetActive(true);
                _exdoorSrpiteMask.gameObject.SetActive(false);

                break;
            case BlockType.Exdoor:
                _spRd.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                _indoorSrpiteMask.gameObject.SetActive(false);
                _exdoorSrpiteMask.gameObject.SetActive(true);
                break;
            default:
                break;
        }
       
    }
    #region DATA
    #region PLACEABLE CONDITION
    // Static placeable condition
    public void SetStaticPlaceableCondition(IPlaceableCondition condition)
    {
        _data.StaticPlaceableConditions.Add(condition);
        ConditionCountStatic = _data.StaticPlaceableConditions.Count;
    }
    //
    //Placeable condition
    public void SetPlaceableCondition(Cell cellOwner, IPlaceableCondition condition)
    {
        ConditionCount = 0;
        if (!_data.PlaceableConditions.ContainsKey(cellOwner))
        {
            _data.PlaceableConditions.Add(cellOwner, new List<IPlaceableCondition> { condition });
        }
        if (!_data.PlaceableConditions[cellOwner].Contains(condition))
        {
            _data.PlaceableConditions[cellOwner].Add(condition);

        }
        Debug.Log("owner: " + name + " condition: " + condition);
    }
    public void ClearPlaceableCondition()
    {
        foreach (var neighborValueKey in GetNeighbors())
        {
            neighborValueKey.Value.ClearPlaceableCondition(this);
        }
        ClearPlaceableCondition(this);
    }
    public void ClearPlaceableCondition(Cell cellOwner)
    {
        if (_data.PlaceableConditions.ContainsKey(cellOwner))
        {
            _data.PlaceableConditions.Remove(cellOwner);
            ConditionCount = _data.PlaceableConditions.Count;
        }

    }
    public bool CheckPlaceableCondition()
    {
        if (IsFullOccupier()) return false;
        if (_currentOccupierRaycast == null) return false;

        if (_data.StaticPlaceableConditions.Count > 0 && _data.StaticPlaceableConditions.Any(x => !x.CheckMatchCondition(_currentOccupierRaycast)))
        {
            return false;
        }

        if (_data.PlaceableConditions.Count == 0)
        {
            return true;
        }

        if (_data.PlaceableConditions.Values.All(x => x.All(condition => condition.CheckMatchCondition(_currentOccupierRaycast))))
        {
            return true;
        }

        return false;
    }
    #endregion
    #region OCCUPIER
    public void SetCurrentBlockRaycast(IOccupier currentOccupierRaycast)
    {
        _currentOccupierRaycast = currentOccupierRaycast;
        Debug.Log("Current occupier raycast: " + _currentOccupierRaycast);
    }

    #region IMPLEMENTATION INTERFACE
    public void SetOccupier(IOccupier occupier)
    {
        if (!_data.Occupiers.Contains(occupier))
        {
            _data.Occupiers.Add(occupier);
        }

    }
    public void ResetOccupier()
    {
        _data.Occupiers.Clear();
    }
    public void RemoveOccupier(IOccupier occupier)
    {
        if (_data.Occupiers.Contains(occupier))
        {
            _data.Occupiers.Remove(occupier);
        }
    }
    public List<IOccupier> GetOccupier() => _data.Occupiers;
    public IOccupier GetLastOccupier() => _data.Occupiers.Last();
    public bool IsOccupier()
    {
        return _data.Occupiers.Count > 0;
    }
    public bool IsFullOccupier()
    {
        if(_cellType == BlockType.Normal)
        {
            return _data.Occupiers.Count == 2 || (_data.Occupiers.Count == 1 && GetOccupier().First().GetOccupierType() == BlockType.Normal.ToString());

        }
        else
        {
            return _data.Occupiers.Count == 1;
        }

    }
    public Vector3 GetDirection()
    {
        return transform.forward.normalized;
    }

    public void ClearOccupiers()
    {
        _data.Occupiers.Clear();
    }

    public bool IsPlaceable()
    {
       return !IsFullOccupier();
    }
    #endregion
    #endregion
    #region NEIGHBOR
    public void SetNeighbor(Vector3 dir,Cell cell)
    {
        if (!_data.Neighbors.ContainsKey(dir))
        {
            _data.Neighbors.Add(dir, cell);
            SetWallActive(dir,cell);
          
        }
    }
    public Dictionary<Vector3, Cell> GetNeighbors() => _data.Neighbors;
    #endregion
    #region PATHFINDING
    public void SetNextOnPath(Cell nextOnPath)
    {
        _data.NextOnPath = nextOnPath;
    }
    public Cell GetNextOnPath() => _data.NextOnPath;
    public bool HasNextOnPath() => _data.NextOnPath != null;
    public void ClearPath()
    {
        _data.Distance = int.MaxValue;
        _data.NextOnPath = null;
    }
    #endregion

    #region VISUAL
    public void OnPlaceable(bool isPlaceable)
    {
        _spRd.color = isPlaceable ? Color.green : initColor;
    }

    #endregion
    #region OTHER




    #endregion
    #endregion
    [System.Serializable]
    public class Data
    {
        public Cell NextOnPath;
        public int Distance;
        public List<IOccupier> Occupiers = new List<IOccupier>();
        public Dictionary<Vector3, Cell> Neighbors = new Dictionary<Vector3, Cell>();
        public List<IPlaceableCondition> StaticPlaceableConditions = new List<IPlaceableCondition>();
        public Dictionary<Cell, List<IPlaceableCondition>> PlaceableConditions = new Dictionary<Cell, List<IPlaceableCondition>>();


    }
    [System.Serializable]
    public class PlaceableCondition :IPlaceableCondition
    {
        protected BlockType _blockType;
        protected Vector3 _direction;
        public PlaceableCondition(BlockType blockType, Vector3 direction = default)
        {
            _blockType = blockType;
            _direction = direction;
        }
        public virtual bool CheckMatchCondition(IOccupier occupier)
        {
            Debug.Log("Except condition: " + occupier);

            if (occupier == null) return false;
            return (occupier.GetOccupierType() == _blockType.ToString()) && (occupier.GetDirection() == _direction || _direction == default);
        }
    }
    public class ExceptPlaceableCondition : IPlaceableCondition
    {
        protected BlockType _blockType;
        protected Vector3 _direction;
        public ExceptPlaceableCondition(BlockType blockType, Vector3 direction = default)
        {
            _blockType = blockType;
            _direction = direction;
        }
        public virtual bool CheckMatchCondition(IOccupier occupier)
        {
            if (occupier == null) return false;
            //if (occupier.GetBlockType() == BlockType.Normal) return false;
            if (occupier.GetOccupierType() == _blockType.ToString() && (occupier.GetDirection() == _direction || _direction == default))
            {
                return false;
            }
            return true;
        }
    }

    [System.Serializable]
    public class PlaceableWithNeighborCondition : ExceptPlaceableCondition
    {
        private Cell _cellNeighbor;
        public PlaceableWithNeighborCondition(BlockType blockType, Cell cellNeighbor, Vector3 direction = default) : base (blockType, direction)
        {
            _cellNeighbor = cellNeighbor;
        }

        public override bool CheckMatchCondition(IOccupier occupier)
        {
            if (!_cellNeighbor.CheckPlaceableCondition()) return base.CheckMatchCondition(occupier)&& occupier.GetOccupierType() != BlockType.Normal.ToString();
            return true;        
        }
    }
}

