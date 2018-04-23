using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Doober : BaseObject {
    public TextMeshPro m_text;
    public float m_lifeTime = 2.0f;
    public float m_popRange = 2.0f;

    protected float m_lifeTimer = 0.0f;
    protected Vector3 m_startPos;
    protected Vector3 m_midPos;
    protected Vector3 m_endPos;

    public void Throw(Vector3 pos, string text, Color color)
    {
        gameObject.SetActive(true);

        transform.position = pos;
        m_text.text = text;
        m_text.color = color;

        m_lifeTimer = m_lifeTime;

        m_startPos = transform.position;
        m_endPos = m_startPos + Random.insideUnitSphere * m_popRange;
        m_midPos = Vector3.Lerp(m_startPos, m_endPos, 0.5f) + Vector3.up * m_popRange;
    }

    public override void ControlledUpdate()
    {
        base.ControlledUpdate();

        m_lifeTimer -= Time.deltaTime;
        if(m_lifeTimer <= 0.0f)
        {
            gameObject.SetActive(false);
        }

        float scale = CameraController.Instance.m_viewDist/10.0f;
        transform.localScale = new Vector3(scale, scale, scale);

        float t = m_lifeTimer / m_lifeTime;
        transform.position = Utils.ThreePointLerp(m_startPos, m_midPos, m_endPos, 1.0f-t);
        m_text.alpha = t;
    }
}
