using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class TimerDisplay : BaseUIElement {
    public TextMeshProUGUI m_timer;
    public bool m_started = false;

    public float m_timeLeft = 90.0f;

    public void Update()
    {
        if(!m_started)
        {
            return;
        }

        m_timeLeft -= Time.deltaTime;

        if (m_timeLeft <= 0.0f)
        {
            m_started = false;
            m_timeLeft = 0.0f;
            Main.Instance.EndGame();
            rectTransform.DOPunchPosition(Vector3.right * 5f, m_transitionOutDelay, 40);
        }

        m_timer.text = string.Format("{0:00.00}", m_timeLeft);
    }

    public override void Show(bool instant = false)
    {
        base.Show(instant);
        m_started = true;
    }
}
