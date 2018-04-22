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
    public ScoreBar m_player1Bar;
    public ScoreBar m_player2Bar;

    public float m_maxBarMoveDist = 600.0f;
    public float m_barMoveStep = 10.0f;
    public float m_barMoveTime = 0.5f;
    public Ease m_barMoveEase = Ease.InOutSine;

    protected void Awake()
    {
        EventManager.OnScoreChange.Register(UpdateScore);
    }

    protected void OnDestroy()
    {
        EventManager.OnScoreChange.Unregister(UpdateScore);
    }

    public void UpdateScore()
    {
        PlayerData p1 = Main.Instance.GetPlayerData(PlayerData.PlayerId.PLAYER_1);
        PlayerData p2 = Main.Instance.GetPlayerData(PlayerData.PlayerId.PLAYER_2);

        m_player1Bar.SetScore(p1.m_score);
        m_player2Bar.SetScore(p2.m_score);

        float scoreDelta = p1.m_score - p2.m_score;
        m_barHolder.DOLocalMoveX(Mathf.Clamp(scoreDelta * m_barMoveStep, -m_maxBarMoveDist, m_maxBarMoveDist), m_barMoveTime).SetEase(m_barMoveEase);
    }

    public override void Show(bool instant = false)
    {
        //base.Show(instant);
        DOTween.Kill(m_barHolder, true);

        m_player1Display.Show(instant);
        m_player2Display.Show(instant);
        m_player1Bar.Show(instant);
        m_player2Bar.Show(instant);

        if (!instant)
        {
            m_barHolder.DOShakePosition(2.0f, 50, 100).SetDelay(m_transitionInDelay);
        }
    }

    public override void Hide(bool instant = false)
    {
        //base.Hide(instant);
        DOTween.Kill(m_barHolder, true);
        m_player1Display.Hide(instant);
        m_player2Display.Hide(instant);
        m_player1Bar.Hide(instant);
        m_player2Bar.Hide(instant);
    }
}
