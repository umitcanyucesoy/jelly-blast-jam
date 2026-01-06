using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public static class ExtraExtensions
{

    public static void SizeUpAnimation(this Transform button)
    {
        button.DOKill();
        button.DOScale(1.2f, 0.25f).SetUpdate(UpdateType.Normal, true).OnComplete(() =>
        {
            button.DOScale(1f, 0.25f).SetEase(Ease.InSine).SetUpdate(UpdateType.Normal, true);
        });
    }

    public static void SizeUpTransform(this Transform button)
    {
        button.DOKill();
        button.DOScale(1.4f, 0.25f).SetUpdate(UpdateType.Normal, true).OnComplete(() =>
        {
            button.DOScale(1f, 0.25f).SetEase(Ease.InSine).SetUpdate(UpdateType.Normal, true);
        });
    }

    public static void SizeUpTransformAndRotate(this Transform button)
    {
        button.DOKill();
        button.DOLocalRotate(new Vector3(0, 360, 0), 0.4f, RotateMode.FastBeyond360);
        button.DOScale(1.8f, 0.4f).SetUpdate(UpdateType.Normal, true).OnComplete(() =>
        {
            button.DOScale(1f, 0.4f).SetEase(Ease.InSine).SetUpdate(UpdateType.Normal, true);
        });
    }

    public static void Delay(this MonoBehaviour mono, Action method, float delay)
    {
        mono.StartCoroutine(CallWithDelayRoutine(method, delay));
    }

    static IEnumerator CallWithDelayRoutine(Action method, float delay)
    {
        yield return new WaitForSeconds(delay);
        method();
    }
}
