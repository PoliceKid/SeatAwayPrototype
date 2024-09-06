using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineHelper : MonoBehaviour
{
    public void DoActionEndFrame(System.Action calllBack)
    {
        StartCoroutine(WaitEndFrameToDo(() =>
        {
            calllBack?.Invoke();
        }));
    }
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

    IEnumerator WaitEndFrameToDo(System.Action calllBack)
    {
        yield return new WaitForEndOfFrame();
        calllBack?.Invoke();
    }
}
