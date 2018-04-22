using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class ScoreBar : BaseUIElement {
    public int m_hideDir = 1;
    public TextMeshProUGUI m_score;

    public float m_scoreUpdateTIme = 0.5f;

    public float m_transitionScore = 0.0f;
    public int m_displayedScore = 0;
    public int m_trueScore = 0;

    public void Start()
    {
        m_score.text = "0";
    }

    public void Update()
    {
        if(m_displayedScore != m_trueScore)
        {
            m_transitionScore = Mathf.Lerp(m_transitionScore, m_trueScore, Time.deltaTime / m_scoreUpdateTIme);
            m_displayedScore = Mathf.RoundToInt(m_transitionScore);
            m_score.text = m_displayedScore.ToString();
        }
    }

    public void SetScore(int score)
    {
        if(score == m_trueScore)
        {
            return;
        }
        m_trueScore = score;
        DOTween.Kill(m_score.rectTransform, true);
        m_score.rectTransform.DOShakePosition(m_scoreUpdateTIme, 30, 100);
    }

    public override void Show(bool instant = false)
    {
        DOTween.Kill(rectTransform);
        if(instant)
        {
            rectTransform.localPosition = Vector3.zero;
            m_score.CrossFadeAlpha(1.0f, 0.0f, true);
            return;
        }
        rectTransform
            .DOLocalMoveX(0.0f, m_transitionInTime)
            .SetDelay(m_transitionInDelay).SetEase(m_transitionInEase)
            .OnComplete(() => m_score.CrossFadeAlpha(1.0f, m_transitionOutTime, false));
    }

    public override void Hide(bool instant = false)
    {
        DOTween.Kill(rectTransform);
        float toPos = Screen.width * m_hideDir;
        if (instant)
        {
            rectTransform.localPosition = new Vector3(toPos, 0,0);
            m_score.CrossFadeAlpha(0.0f, 0.0f, true);
            return;
        }
        m_score.CrossFadeAlpha(0.0f, m_transitionOutTime * 0.25f, false);
        rectTransform
            .DOLocalMoveX(toPos, m_transitionOutTime)
            .SetDelay(m_transitionOutDelay)
            .SetEase(m_transitionOutEase);
    }
}
