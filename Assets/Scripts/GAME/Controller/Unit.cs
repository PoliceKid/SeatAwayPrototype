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
    public System.Action OnUnitDestination = delegate { };
    private UnitData _data;

    public void Init()
    {
        _data = new UnitData();
        _data.CodeName = _codeNameType.ToString();
    }
    public Vector3 GetDirection()
    {
        return transform.forward.normalized;
    }

    public string GetOccupierType()
    {
        return _data.CodeName;
    }

    public void OnPlaceable(bool isValid)
    {
        gameObject.SetActive(false);
    }
    public void InitOccupier(Action callBack)
    {
        OnUnitDestination += callBack;
    }
    public void MoveTo(Vector3 pos)
    {
        
    }
    public void MoveThroughPoints(List<Vector3> cellPositions)
    {
        if(cellPositions == null) return;
        if (cellPositions.Count > 0)
        {
            StartCoroutine(StartMoveThroughPoints(cellPositions));
        }
    }
    IEnumerator StartMoveThroughPoints(List<Vector3> cellPositions)
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
        OnUnitDestination?.Invoke();
    }

 

    [System.Serializable]
    public class UnitData
    {
        public string CodeName;
    }
}
