using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Gateway : MonoBehaviour
{
    #region PROPERTIES
    [SerializeField] Transform _unitContainer;
    [SerializeField] Cell _connectedCell;
    #endregion
    #region CURRENT DATA
    private Queue<Unit> _unitQueue;
    private Dictionary<Unit, bool> _unitCompleteWay;
    private List<Vector3> _unitQueuePos;

    public Queue<Unit> GetUnitQueue => _unitQueue;
    public Cell GetConnectedCell => _connectedCell;
    public System.Action OnCompleteWay = delegate { };
    #endregion
    public void Init()
    {
        _unitQueue = new Queue<Unit>();
        _unitQueuePos = new List<Vector3>();
        _unitCompleteWay = new Dictionary<Unit, bool>();
        foreach (Transform child in _unitContainer)
        {
            Unit unit = child.GetComponent<Unit>();
            if (unit != null)
            {
                unit.Init();
                _unitQueue.Enqueue(unit);
                _unitQueuePos.Add(unit.transform.position);
                _unitCompleteWay.Add(unit, false);
                unit.OnUnitDestination += HandleOnUnitDestination;
            }
        }
    }

    private void HandleOnUnitDestination(Unit unit)
    {
        if (_unitCompleteWay.ContainsKey(unit))
        {
            _unitCompleteWay[unit] = true;
        }
        if (IsCompleteWay())
        {
            OnCompleteWay?.Invoke();
        }
    }

    public Queue<Unit> GetUnitRange(int range)
    {
        if (range > _unitQueue.Count)
        {
            range = _unitQueue.Count;
        }

        return new Queue<Unit>(_unitQueue.Take(range));
    }
    public Queue<Unit> GetUnitSortByCodename(Queue<Unit> unitQueue)
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
    public Unit DequeueUnit()
    {
        Unit unit = _unitQueue.Dequeue();       
        return unit;
    }
    public void MoveUnitsInQueue()
    {
        Queue<Unit> _unitQueueTemp = new Queue<Unit>(_unitQueue);
        foreach (var queuePos in _unitQueuePos)
        {
            if (_unitQueueTemp.Count > 0)
            {
                Unit nextUnit = _unitQueueTemp.Dequeue();
                nextUnit.MoveTo(new List<Vector3> { queuePos });
            }
        }
    }

    public void DequeueUnitLoop(System.Func<Unit,bool> callBack = null)
    {
        if (_unitQueue.Count == 0) return;
        foreach (Unit unit in _unitQueue.ToList()) {     
            //callBack?.Invoke(unit);
            bool canContinue = callBack(unit);
            if (!canContinue) return;
        }
    }
    public bool IsCompleteWay()
    {
        return _unitCompleteWay.All(x => x.Value == true);
    }
}
