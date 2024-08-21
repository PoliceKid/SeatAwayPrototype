using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
public class Cell : MonoBehaviour
{
    [SerializeField] SpriteRenderer _spRd;
    [SerializeField] private BlockType _cellType;
    //Current data
    private Data _data;
    private Color initColor;
    public int ConditionCount;
    public void Init()
    {
        _data = new Data();
        initColor = _spRd.color;
        ApplyCellType(_cellType);
        name = $"Pos X: {transform.position.x}, Pos Y: {transform.position.y}";
    }
    public void ApplyCellType(BlockType cellType)
    {
        switch (cellType)
        {
            case BlockType.Normal:
                _spRd.maskInteraction = SpriteMaskInteraction.None;
                break;
            case BlockType.Indoor:
                _spRd.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;

                break;
            case BlockType.Exdoor:
                _spRd.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                break;
            default:
                break;
        }
    }
    #region DATA
    #region OCCUPIER
    
    public void SetPlaceableCondition(IPlaceableCondition condition)
    {
        _data.occupierConditions.Add(condition);
        ConditionCount = _data.occupierConditions.Count;
    }
    public void ClearPlaceableCondition()
    {
        _data.occupierConditions.Clear();
    }
    private IOccupier _currentOccupierRaycast;
    public void SetCurrentOccupierRaycast(IOccupier currentOccupierRaycast)
    {
        _currentOccupierRaycast = currentOccupierRaycast;
    }
    public bool CheckPlaceableCondition()
    {
        if ( _data.occupierConditions.Count ==0) {
            return true;
        } 
        else
        {
            if(_data.occupierConditions.Count >0)
            {
                if (_currentOccupierRaycast == null) return false;
                if (_data.occupierConditions.All(x => x.IsMatchCondition(_currentOccupierRaycast)))
                {
                    return true;
                }
            }  
        }
        return false;
    }
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
    public List<IOccupier> GetOccupier => _data.Occupiers;
    public bool isOccupier() => _data.Occupiers.Count > 0;
    #endregion
    #region NEIGHBOR
    public void SetNeighbor(Cell cell)
    {
        if (!_data.Neighbors.Contains(cell))
        {
            _data.Neighbors.Add(cell);
        }
    }
    public List<Cell> GetNeighbors() => _data.Neighbors;
    #endregion
    #endregion
    #region VISUAL
    public void OnPlaceable()
    {
        _spRd.color = Color.green;
    }
    public void ResetColor()
    {
        _spRd.color = initColor;
    }
    #endregion
    [System.Serializable]
    public class Data
    {
        public List<IOccupier> Occupiers = new List<IOccupier>();
        public List<Cell> Neighbors = new List<Cell>();
        public List<IPlaceableCondition> occupierConditions = new List<IPlaceableCondition>();
    }
    [System.Serializable]
    public class PlaceableCondition :IPlaceableCondition
    {        
        public BlockType BlockType;
        public Vector3 Direction;
        public PlaceableCondition(BlockType blockType, Vector3 direction)
        {
            BlockType = blockType;
            Direction = direction;
        }
        public bool IsMatchCondition(IOccupier occupier)
        {
            return (occupier.GetBlockType() == BlockType ) && (occupier.GetDirection() == Direction || Direction == default);
        }
    }
    [System.Serializable]
    public class ExceptPlaceableCondition : IPlaceableCondition
    {
        public BlockType BlockType;
        public Vector3 Direction;
        public ExceptPlaceableCondition(BlockType blockType, Vector3 direction)
        {
            BlockType = blockType;
            Direction = direction;
        }
        public bool IsMatchCondition(IOccupier occupier)
        {
            return (occupier.GetBlockType() != BlockType) && (occupier.GetDirection() == Direction || Direction == default);
        }
    }
    [System.Serializable] 
    public class NeighborCondition : IPlaceableCondition
    {
        public List<Cell> Cells;

        public NeighborCondition(List<Cell> cells)
        {
            Cells = cells;
        }

        public bool IsMatchCondition(IOccupier occupier)
        {
            return Cells.All(x => x.CheckPlaceableCondition());
        }
    }
}

