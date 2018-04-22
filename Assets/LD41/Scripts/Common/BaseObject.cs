using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseObject : MonoBehaviour {
    public static List<BaseObject> s_allObjects = new List<BaseObject>();
    public static MaterialPropertyBlock s_sharedMaterialPropertyBlock;
    public readonly int COLOR_PROPERTY_ID = Shader.PropertyToID("_Color");

    public GameObject m_model;
    public Rigidbody m_rigidbody;

    protected float m_originallMass;
    protected float m_originalDrag;
    protected float m_originalAngularDrag;

    public Renderer[] m_renderers { get; protected set; }
    
    protected Color[] m_originalColors;
    protected bool m_colorsModified = false;

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

        m_renderers = m_model.GetComponentsInChildren<Renderer>(true);
        
        m_originalColors = new Color[m_renderers.Length];
        for(int i = 0; i < m_renderers.Length; ++i)
        {
            m_renderers[i].GetPropertyBlock(s_sharedMaterialPropertyBlock);
            m_originalColors[i] = s_sharedMaterialPropertyBlock.GetColor(COLOR_PROPERTY_ID);
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
    }

    public virtual void ResetColor()
    {
        if(!m_colorsModified)
        {
            return;
        }

        for (int i = 0; i < m_renderers.Length; ++i)
        {
            s_sharedMaterialPropertyBlock.SetColor(COLOR_PROPERTY_ID, m_originalColors[i]);
            m_renderers[i].SetPropertyBlock(s_sharedMaterialPropertyBlock);
        }
    }

    public virtual void SetColor(Color c)
    {
        Debug.Log("setting color");
        m_colorsModified = true;
        for (int i = 0; i < m_renderers.Length; ++i)
        {
            s_sharedMaterialPropertyBlock.SetColor(COLOR_PROPERTY_ID, c);
            m_renderers[i].SetPropertyBlock(s_sharedMaterialPropertyBlock);
        }
    }
}
