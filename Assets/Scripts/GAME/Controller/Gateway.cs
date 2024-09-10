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
    private Queue<Unit> _unitQueue = new Queue<Unit>();

    List<Vector3> _unitQueuePos = new List<Vector3>();
    public Cell GetConnectedCell => _connectedCell;
    public void Init()
    {
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
