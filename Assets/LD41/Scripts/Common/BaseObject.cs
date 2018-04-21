using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseObject : MonoBehaviour {
    public static List<BaseObject> s_allObjects = new List<BaseObject>();
    
    public GameObject m_model;
    public Rigidbody m_rigidbody;

    public Renderer[] m_renderers { get; protected set; }

    protected Color[] m_originalColors;
    protected bool m_colorsModified = false;

    protected virtual void Awake()
    {
        s_allObjects.Add(this);

        if(m_model == null)
        {
            m_model = gameObject;
        }

        if(m_rigidbody == null)
        {
            m_rigidbody = GetComponentInChildren<Rigidbody>(true);
        }

        m_renderers = m_model.GetComponentsInChildren<Renderer>(true);

        m_originalColors = new Color[m_renderers.Length];
        
        for(int i = 0; i < m_renderers.Length; ++i)
        {
            m_originalColors[i] = m_renderers[i].sharedMaterial.color;
        }
    }


    protected virtual void OnDestroy()
    {
        s_allObjects.Remove(this);
    }

    public virtual void ControlledUpdate()
    {
        // for children to fill out
    }

    public virtual void ResetColor()
    {
        if(!m_colorsModified)
        {
            return;
        }

        for (int i = 0; i < m_renderers.Length; ++i)
        {
            m_renderers[i].material.color = m_originalColors[i];
        }
    }

    public virtual void SetColor(Color c)
    {
        m_colorsModified = true;
        for (int i = 0; i < m_renderers.Length; ++i)
        {
            m_renderers[i].material.color = c;
        }
    }
}
