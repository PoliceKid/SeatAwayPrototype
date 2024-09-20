using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DotweenAnimationHelper 
{
    public static Tween AnimationScaleLoop(GameObject targetObject,float scaleFactor,float duration)
    {
        Tween tween = targetObject.transform.DOScale(scaleFactor, duration).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        return tween;
    }
}
