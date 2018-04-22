using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ScoreDisplay : BaseUIElement
{
    public PlayerDisplay m_player1Display;
    public PlayerDisplay m_player2Display;

    public RectTransform m_barHolder;
    public Image m_player1Bar;
    public Image m_player2Bar;

    public override void Show(bool instant = false)
    {
        //base.Show(instant);
        if(instant)
        {
            m_player1Display.Show(instant);
            m_player2Display.Show(instant);

            m_player1Bar.rectTransform.localPosition = Vector3.zero;
            m_player2Bar.rectTransform.localPosition = Vector3.zero;
            return;
        }

        m_player1Display.Show();
        m_player2Display.Show();

        m_barHolder.DOShakePosition(m_transitionInTime * 2, 30, 30);
        TransitionBarIn(m_player1Bar);
        TransitionBarIn(m_player2Bar);
    }

    protected void TransitionBarIn(Image bar)
    {
        bar.rectTransform.DOLocalMoveX(0.0f, m_transitionInTime).SetDelay(m_player1Display.m_transitionInTime * 0.5f).SetEase(m_transitionInEase);
    }

    public override void Hide(bool instant = false)
    {
        //base.Hide(instant);
        if (instant)
        {
            m_player1Display.Hide(instant);
            m_player2Display.Hide(instant);

            m_player1Bar.rectTransform.localPosition = Vector3.right * -Screen.width;
            m_player2Bar.rectTransform.localPosition = Vector3.right * Screen.width;
            return;
        }
    }
}
