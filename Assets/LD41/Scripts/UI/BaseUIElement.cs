using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BaseUIElement : MonoBehaviour {
    public float m_transitionInTime = 0.33f;
    public Ease m_transitionInEase = Ease.InOutSine;

    public float m_transitionOutTime = 0.33f;
    public Ease m_transitionOutEase = Ease.InOutSine;

    public virtual void Show(bool instant = false)
    {
        gameObject.SetActive(true);
        DOTween.Kill(transform);
        if (instant)
        {
            transform.localScale = Vector3.one;
            return;
        }
        transform.DOScale(1.0f, m_transitionInTime).SetEase(m_transitionInEase);
    }

    public virtual void Hide(bool instant = false)
    {
        DOTween.Kill(transform);
        if (instant)
        {
            gameObject.SetActive(false);
            transform.localScale = Vector3.zero;
            return;
        }
        transform.DOScale(0.0f, m_transitionOutTime).SetEase(m_transitionOutEase).OnComplete(()=>gameObject.SetActive(false));
    }
}
