using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BaseUIElement : MonoBehaviour {
    public enum TransitionMethod
    {
        MOVE,
        SCALE,
        CUSTOM,
    }

    public TransitionMethod m_transitionMethod = TransitionMethod.SCALE;

    public float m_transitionInTime = 0.33f;
    public float m_transitionInDelay = 0.0f;
    public Ease m_transitionInEase = Ease.InOutSine;

    public float m_transitionOutTime = 0.33f;
    public float m_transitionOutDelay = 0.0f;
    public Ease m_transitionOutEase = Ease.InOutSine;

    public RectTransform rectTransform { get { return transform as RectTransform;  } }

    public virtual void Show(bool instant = false)
    {
        gameObject.SetActive(true);
        DOTween.Kill(rectTransform);
        if (instant)
        {
            switch(m_transitionMethod)
            {
                case TransitionMethod.MOVE:
                    rectTransform.localPosition = Vector3.zero;
                    break;
                case TransitionMethod.SCALE:
                    rectTransform.localScale = Vector3.one;
                    break;
            }
            return;
        }

        switch (m_transitionMethod)
        {
            case TransitionMethod.MOVE:
                rectTransform.DOLocalMoveY(0.0f, m_transitionInTime, true).SetEase(m_transitionInEase).SetDelay(m_transitionInDelay);
                break;
            case TransitionMethod.SCALE:
                rectTransform.DOScale(1.0f, m_transitionInTime).SetEase(m_transitionInEase).SetDelay(m_transitionInDelay);
                break;
        }

    }


    public virtual void Hide(bool instant = false)
    {
        DOTween.Kill(rectTransform);
        if (instant)
        {
            gameObject.SetActive(false);
            switch (m_transitionMethod)
            {
                case TransitionMethod.MOVE:
                    rectTransform.localPosition = new Vector3(0, Screen.height * 2, 0);
                    break;
                case TransitionMethod.SCALE:
                    transform.localScale = Vector3.zero;
                    break;
            }
            return;
        }
        
        switch (m_transitionMethod)
        {
            case TransitionMethod.MOVE:
                rectTransform.DOLocalMoveY(Screen.height * 2, m_transitionOutTime, true).SetEase(m_transitionOutEase).SetDelay(m_transitionOutDelay).OnComplete(() => gameObject.SetActive(false));
                break;
            case TransitionMethod.SCALE:
                rectTransform.DOScale(0.0f, m_transitionOutTime).SetEase(m_transitionOutEase).SetDelay(m_transitionOutDelay).OnComplete(() => gameObject.SetActive(false));
                break;
        }
    }
}
