﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseObject : MonoBehaviour {
    public static List<BaseObject> s_allObjects = new List<BaseObject>();
    public static MaterialPropertyBlock s_sharedMaterialPropertyBlock;
    public readonly int COLOR_PROPERTY_ID = Shader.PropertyToID("_Color");

    public const float MIN_Y = -15.0f;

    public GameObject m_model;
    public Rigidbody m_rigidbody;

    public bool m_hasColorProperty = true;
    public bool m_isUsingInstancedMaterial = true;
    public string m_colorPropertyName = string.Empty;

    protected float m_originallMass;
    protected float m_originalDrag;
    protected float m_originalAngularDrag;

    public Renderer[] m_renderers { get; protected set; }

    protected int m_colorPropertyID = 0;
    protected Color[] m_originalColors;
    protected bool m_colorModified = false;
    protected Color m_color;

    protected virtual void Awake()
    {
        s_allObjects.Add(this);

        if(s_sharedMaterialPropertyBlock == null)
        {
            s_sharedMaterialPropertyBlock = new MaterialPropertyBlock();
        }

        if(m_model == null)
        {
            m_model = gameObject;
        }

        if(m_rigidbody == null)
        {
            m_rigidbody = GetComponentInChildren<Rigidbody>(true);
        }

        if(m_rigidbody != null)
        {
            m_originallMass = m_rigidbody.mass;
            m_originalDrag = m_rigidbody.drag;
            m_originalAngularDrag = m_rigidbody.angularDrag;
        }

        if (m_colorPropertyName == string.Empty)
        {
            m_colorPropertyID = COLOR_PROPERTY_ID;
        }
        else
        {
            m_colorPropertyID = Shader.PropertyToID(m_colorPropertyName);
        }

        m_renderers = m_model.GetComponentsInChildren<Renderer>(true);
        
        if(m_hasColorProperty)
        {
            m_originalColors = new Color[m_renderers.Length];
            for (int i = 0; i < m_renderers.Length; ++i)
            {
                m_originalColors[i] = m_renderers[i].sharedMaterial.GetColor(m_colorPropertyID);
            }
        }
        
    }


    protected virtual void OnDestroy()
    {
        s_allObjects.Remove(this);
    }

    public void RemoveRigidbody()
    {
        if(m_rigidbody == null)
        {
            return;
        }

        Destroy(m_rigidbody);
    }

    public void RestoreRigidbody()
    {
        if(m_rigidbody != null && m_originallMass > 0)
        {
            return;
        }

        m_rigidbody = gameObject.AddComponent<Rigidbody>();
        m_rigidbody.mass = m_originallMass;
        m_rigidbody.drag = m_originalDrag;
        m_rigidbody.angularDrag = m_originalAngularDrag;
    }

    public virtual void ControlledUpdate()
    {
        // for children to fill out
        if(transform.position.y <= MIN_Y)
        {
            OnKillPlaneHit();
        }
    }

    protected virtual void OnKillPlaneHit()
    {
        gameObject.SetActive(false);
    }

    public virtual void ResetColor()
    {
        if (!m_hasColorProperty)
        {
            return;
        }

        if (!m_colorModified)
        {
            return;
        }
        m_colorModified = false;

        if(m_isUsingInstancedMaterial)
        {
            for (int i = 0; i < m_renderers.Length; ++i)
            {
                s_sharedMaterialPropertyBlock.SetColor(m_colorPropertyID, m_originalColors[i]);
                m_renderers[i].SetPropertyBlock(s_sharedMaterialPropertyBlock);
            }
        }
        else
        {
            for (int i = 0; i < m_renderers.Length; ++i)
            {
                m_renderers[i].material.color = m_originalColors[i];
            }
        }
        
    }

    public virtual void SetTexture(Texture t)
    {
        for (int i = 0; i < m_renderers.Length; ++i)
        {
            m_renderers[i].material.mainTexture = t;
        }
    }

    public virtual void SetColor(Color c)
    {
        if (!m_hasColorProperty)
        {
            return;
        }

        if (m_colorModified && c == m_color)
        {
            return;
        }

        m_colorModified = true;
        m_color = c;
        if (m_isUsingInstancedMaterial)
        {
            for (int i = 0; i < m_renderers.Length; ++i)
            {
                s_sharedMaterialPropertyBlock.SetColor(m_colorPropertyID, c);
                m_renderers[i].SetPropertyBlock(s_sharedMaterialPropertyBlock);
            }
        }
        else
        {
            for (int i = 0; i < m_renderers.Length; ++i)
            {
                m_renderers[i].material.SetColor(m_colorPropertyID, c);
            }
        }
    }
}
