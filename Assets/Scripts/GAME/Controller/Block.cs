using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour, IOccupier
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
        return transform.up.normalized;
    }
    public void OnPlaceable(bool isPlaceable)
    {
        //_collider.enabled = !isPlaceable;
    }
    public BlockType GetBlockType()
    {
        return _blockType;
    }
    public void OnValidPlaceable(bool isValid)
    {
        _spRd.color = !isValid?Color.red: initColor;
    }
    [SerializeField] 
    public class Data
    {
        public Transform initParent;
        public Vector3 initPoint;
    }
  
}
