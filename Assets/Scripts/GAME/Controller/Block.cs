using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class Block : MonoBehaviour, IOccupier, IOccupierContainer<IOccupier>
{
    #region PROPERTY
    [SerializeField] private CodeNameType _codeNameType;
    [SerializeField] MeshRenderer _renderer;
    [SerializeField] private SpriteRenderer _spRd;
    [SerializeField] SpriteMask _indoorSrpiteMask;
    [SerializeField] SpriteMask _exdoorSrpiteMask;
    [SerializeField] private Transform _door;
    [SerializeField] private BlockType _blockType;
    [SerializeField] private GameObject _unitContainer;
    [SerializeField] BoxCollider _collider;
    [SerializeField] GameObject _wallLeft;
    [SerializeField] GameObject _wallRight;
    [SerializeField] GameObject _wallTop;
    [SerializeField] GameObject _wallBottom;
    #endregion
    #region CURRENT DATA
    private Data _data;
    public Data GetData => _data;
    public Vector3 LocalDir;
    private Color initColor;
    public bool IsOnUnitDestination = false;
    public System.Action<Block> OnUnitDestionation = delegate { };
    public System.Action<Block> OnUnitStartOccpier = delegate { };
    private GameObject _parent;
    #endregion
    public void Init(Transform parent, CodeNameType codeNameType = default)
    {
        _data = new Data();
        _data.initPoint = transform.localPosition;
        _data.initParent = parent;
        if(codeNameType == default)
        {
           _data.CodeName = _codeNameType.ToString();
        }
        else
        {
            _data.CodeName = codeNameType.ToString();
        }
        _data.Id = System.Guid.NewGuid().ToString();
        ApplyBlockType(_blockType);
        ApplyColor(_data.CodeName);
        InitWall();
        LocalDir = GetDirection();
        initColor = _spRd.color;
        name = $"Block Pos X: {transform.localPosition.x}, Pos Z: {transform.localPosition.z}";
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
    public void InitWall()
    {
        if (_blockType != BlockType.Normal) return;
        _wallLeft.gameObject.SetActive(true);
        _wallRight.gameObject.SetActive(true);
        _wallTop.gameObject.SetActive(true);
        _wallBottom.gameObject.SetActive(true);
    }
    private static Vector3[] dir = new Vector3[] { Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
    public void SetWallActive(Vector3 dirNotFaceWall, BlockType type)
    {
        bool canSetNeighborWall = type == BlockType.Normal;
        //dirNotFaceWall *= GetDirection().z;
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
    public void ApplyColor(string codeName)
    {
        if (Enum.TryParse(codeName, true, out CodeNameType parsedCodeName))
        {
            switch (parsedCodeName)
            {
                case CodeNameType.Blue:
                    _renderer.material.color = Color.blue;
                    break;
                case CodeNameType.Red:
                    _renderer.material.color = Color.red;
                    break;
                case CodeNameType.Yellow:
                    _renderer.material.color = Color.yellow;
                    break;
                case CodeNameType.Green:
                    _renderer.material.color = Color.green;
                    break;
                case CodeNameType.Purple:
                    _renderer.material.color = Color.white;
                    break;
                default:
                    _renderer.material.color = Color.blue;
                    break;
            }
        }
        else
        {
            Debug.LogError($"Invalid CodeName: {codeName}");
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
            SetWallActive(dir, block._blockType);
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
        if (!_data.Occupiers.Contains(occupier))
        {
            _data.Occupiers.Add(occupier);
            OnUnitStartOccpier?.Invoke(this);
            occupier.InitOccupier(this.gameObject,() =>
            {
                OnUnitDestionation.Invoke(this);
                
            });

        }
    }

    public void RemoveOccupier(IOccupier occupier)
    {
        if (_data.Occupiers.Contains(occupier))
        {
            _data.Occupiers.Remove(occupier);
            occupier.ClearParent();
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
    public string GetCodeName()
    {
        return _data.CodeName;
    }
    public void InitOccupier(GameObject parent, Action callBack = null)
    {
        _parent = parent;
    }

    public T GetParent<T>() where T : MonoBehaviour
    {
        return transform.parent.GetComponent<T>();
    }
    public void ClearParent()
    {
        _parent = null;
    }

 
    #endregion


    [SerializeField] 
    public class Data
    {
        public string Id;
        public string CodeName;
        public Transform initParent;
        public Vector3 initPoint;
        public Dictionary<Vector3, Block> Neighbors = new Dictionary<Vector3, Block>();
        public List<IOccupier> Occupiers = new List<IOccupier>();
    }
  
}
