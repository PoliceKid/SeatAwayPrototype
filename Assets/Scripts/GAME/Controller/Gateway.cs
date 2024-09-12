using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Gateway : MonoBehaviour
{
    [SerializeField] Transform _unitContainer;
    [SerializeField] Cell _connectedCell;
    private Queue<Unit> _unitQueue;
    public Queue<Unit> GetUnitQueue => _unitQueue;
    private List<Vector3> _unitQueuePos;
    public Cell GetConnectedCell => _connectedCell;
    //public Queue<Unit> GetUnitSortByCodename()
    //{
    //    Queue<Unit> sortedUnits = new Queue<Unit>();

    //    foreach (Unit unit in _unitQueue)
    //    {
    //        sortedUnits.Enqueue(unit);
    //    }

    //    sortedUnits = new Queue<Unit>(sortedUnits.OrderBy(unit => unit.GetCodeName()));

    //    return sortedUnits;
    //}
    public Queue<Unit> GetUnitSortByCodename()
    {
        Queue<Unit> sortedUnits = new Queue<Unit>();
        Dictionary<string, Queue<Unit>> unitSortByCodeName = new Dictionary<string, Queue<Unit>>();

        foreach (Unit unit in _unitQueue)
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
    public void Init()
    {
        _unitQueue = new Queue<Unit>();
        _unitQueuePos = new List<Vector3>();
        foreach (Transform child in _unitContainer)
        {
            Unit unit = child.GetComponent<Unit>();
            if (unit != null)
            {
                unit.Init();
                _unitQueue.Enqueue(unit);
                _unitQueuePos.Add(unit.transform.position);
            }
        }

        //StartCoroutine(MoveUnitsInQueue());
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

}
