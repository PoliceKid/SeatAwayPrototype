using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Block : MonoBehaviour, IOccupier, IOccupierContainer<IOccupier>
{
    [SerializeField] private SpriteRenderer _spRd;
    [SerializeField] SpriteMask _indoorSrpiteMask;
    [SerializeField] SpriteMask _exdoorSrpiteMask;
    [SerializeField] private Transform _door;
    [SerializeField] private BlockType _blockType;
    [SerializeField] BoxCollider _collider;
    private Data _data;
    public Data GetData => _data;
    public Vector3 LocalDir;
    private Color initColor;
    public void Init(Transform parent)
    {
        _data = new Data();
        _data.initPoint = transform.localPosition;
        _data.initParent = parent;
        ApplyBlockType(_blockType);
        LocalDir = GetDirection();
        initColor = _spRd.color;
        name = $"Pos X: {transform.localPosition.x}, Pos Z: {transform.localPosition.z}";
    }
    public void ApplyBlockType(BlockType blockType)
    {
        switch (blockType)
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
    public void ResetPosition()
    {
        transform.localPosition = _data.initPoint;
    }

    public Vector3 GetDirection()
    {
        return transform.forward.normalized;
    }
    public void OnPlaceable(bool isPlaceable)
    {
        _spRd.color = !isPlaceable?Color.red: initColor;
        validPlacable = isPlaceable;
    }
    private bool validPlacable;
    public bool ValidPlaceable => validPlacable;
    #region NEIGHBOR
    public void SetNeighbor(Vector3 dir, Block block)
    {
        if (!_data.Neighbors.ContainsKey(dir))
        {
            _data.Neighbors.Add(dir, block);
        }
    }
    public Block GetBlockNeighborByDirection( Vector3 dir)
    {
        if (_data.Neighbors.ContainsKey(dir))
        {
           return _data.Neighbors[dir];
        }
        return null;
    }
    public Dictionary<Vector3, Block> GetNeighbors() => _data.Neighbors;
    #endregion
    #region IMPLEMENTATION INTERFACE
    public void SetOccupier(IOccupier occupier)
    {
        //if(occupier.GetType() == )
        if (!_data.Occupiers.Contains(occupier))
        {
            _data.Occupiers.Add(occupier);
        }
    }

    public void RemoveOccupier(IOccupier occupier)
    {
        if (_data.Occupiers.Contains(occupier))
        {
            _data.Occupiers.Remove(occupier);
        }
    }

    public void ClearOccupiers()
    {
        _data.Occupiers.Clear();
    }

    public IOccupier GetLastOccupier()
    {
        return _data.Occupiers.Last();
    }

    public bool IsFullOccupier()
    {
      return _data.Occupiers.Count == 1;
    }

    public bool IsPlaceable()
    {
        return _data.Occupiers.Count == 0;
    }

    public bool IsOccupier()
    {
        return _data.Occupiers.Count > 0;
    }

    public string GetOccupierType()
    {
        return _blockType.ToString();
    }
    #endregion

    [SerializeField] 
    public class Data
    {
        public Transform initParent;
        public Vector3 initPoint;
        public Dictionary<Vector3, Block> Neighbors = new Dictionary<Vector3, Block>();
        public List<IOccupier> Occupiers = new List<IOccupier>();
    }
  
}
