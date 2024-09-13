using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour, IOccupier
{
     #region PROPERTIES
    [SerializeField] MeshRenderer _renderer;
    [SerializeField] CodeNameType _codeNameType;
    [SerializeField] private float _moveSpeed = 5f;
    #endregion
    public System.Action<Unit> OnUnitDestination = delegate { };
    private UnitData _data;
    private GameObject _parent;

    public void Init()
    {
        _data = new UnitData();
        _data.CodeName = _codeNameType.ToString();
        ApplyColor(_codeNameType);
    }
    public Vector3 GetDirection()
    {
        return transform.forward.normalized;
    }
    public CodeNameType GetCodeNameType() => _codeNameType;
    public string GetOccupierType()
    {
        return _data.CodeName;
    }
    public string GetCodeName()
    {
        return _codeNameType.ToString();
    }
    public void InitOccupier(GameObject parent,Action callBack)
    {
        OnUnitDestination += (unit) => callBack();
        _parent = parent;
        transform.parent = _parent.transform;
    }

    public void OnPlaceable(bool isValid)
    {
        gameObject.SetActive(false);
    }
    public GameObject GetParent()
    {
       return _parent;
    }
    public void ClearParent()
    {
        _parent = null;
    }
    public void MoveTo(Vector3 pos)
    {
        
    }
    private Coroutine _moveCoroutine;
    public void MoveTo(List<Vector3> cellPositions, bool onDestination = false)
    {
        if(cellPositions == null) return;
        if (cellPositions.Count > 0)
        {
            if (_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
            }
            _moveCoroutine =StartCoroutine(StartMove(cellPositions, onDestination));
        }
    }
    IEnumerator StartMove(List<Vector3> cellPositions, bool onDestination = false)
    {
        int currentCellIndex = 0;
        while (currentCellIndex < cellPositions.Count)
        {
            Vector3 targetPosition = cellPositions[currentCellIndex];
            while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, _moveSpeed * Time.deltaTime);
                yield return null;
            }

            // Đảm bảo Unit đến chính xác vị trí đích
            transform.position = targetPosition;

            currentCellIndex++;
           
        }
        if(onDestination)
        OnUnitDestination?.Invoke(this);
    }
    #region VISUAL
    public void ApplyColor(CodeNameType codeNameType)
    {
        switch (codeNameType)
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
                break;
        }
    }

  
    #endregion


    [System.Serializable]
    public class UnitData
    {
        public string CodeName;
    }
}
