using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : Singleton<UIManager> {
    public ScoreDisplay m_scoreDisplay;

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
        m_scoreDisplay.Hide(true);
        m_scoreDisplay.Show();
    }

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
    }
}
