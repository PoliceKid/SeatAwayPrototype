using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineHelper : MonoBehaviour
{
    public void DoActionOnTime(System.Action calllBack, float second)
    {
        StartCoroutine(WaitSecondToDo(() =>
        {
            calllBack?.Invoke();
        }, second));
    }
    IEnumerator WaitSecondToDo(System.Action calllBack, float second)
    {
        yield return new WaitForSeconds(second);
        calllBack?.Invoke();
    }
}
