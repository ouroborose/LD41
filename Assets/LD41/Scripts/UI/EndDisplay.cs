using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndDisplay : BaseUIElement {
    public void Restart()
    {
        UIManager.Instance.m_blackout.Show();
        Destroy(gameObject, UIManager.Instance.m_blackout.m_transitionOutDelay + UIManager.Instance.m_blackout.m_transitionOutTime + 1.0f);
    }

    public void OnDestroy()
    {
        SceneManager.LoadScene(0);
    }
}
