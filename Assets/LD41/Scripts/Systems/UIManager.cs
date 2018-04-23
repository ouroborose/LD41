using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : Singleton<UIManager> {
    public BaseUIElement m_blackout;
    public ScoreDisplay m_scoreDisplay;
    public TitleDisplay m_titleDisplay;
    public EndDisplay m_endP1Display;
    public EndDisplay m_endP2Display;
    public EndDisplay m_endTieDisplay;
    public ButtonMappingsDisplay m_buttomMappingDisplay;
    public TimerDisplay m_timerDisplay;

    protected override void Awake()
    {
        if (SceneManager.GetActiveScene().buildIndex != 0)
        {
            SceneManager.LoadScene(0);
            return;
        }
        base.Awake();
    }

    protected void Start()
    {
        m_endP1Display.Hide(true);
        m_endP2Display.Hide(true);
        m_endTieDisplay.Hide(true);
        m_buttomMappingDisplay.Hide(true);
        m_scoreDisplay.Hide(true);
        m_timerDisplay.Hide(true);
        m_titleDisplay.Show(true);
        m_blackout.Show(true);
    }

#if UNITY_EDITOR
    bool m_scoreDisplayVisible = true;
    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if(m_scoreDisplayVisible)
            {
                m_scoreDisplay.Hide();
            }
            else
            {
                m_scoreDisplay.Show();
            }
            m_scoreDisplayVisible = !m_scoreDisplayVisible;
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            Main.Instance.ModifyScore(PlayerData.PlayerId.PLAYER_2, 100);
        }

        if(Input.GetKeyDown(KeyCode.P))
        {
            m_timerDisplay.m_timeLeft = 0;
        }
    }
#endif
}
