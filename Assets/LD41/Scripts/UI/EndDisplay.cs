using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndDisplay : BaseUIElement {
    protected bool m_doRestart = false;
    public void Restart()
    {
        m_doRestart = true;
        UIManager.Instance.m_blackout.Show();
        Destroy(gameObject, UIManager.Instance.m_blackout.m_transitionOutDelay + UIManager.Instance.m_blackout.m_transitionOutTime + 1.0f);
    }

    protected void OnDestroy()
    {
        if(m_doRestart)
        {
            SceneManager.LoadScene(0);
        }
    }
}
