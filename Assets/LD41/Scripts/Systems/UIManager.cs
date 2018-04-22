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
}
