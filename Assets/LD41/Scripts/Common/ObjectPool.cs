using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ObjectPool
{
    [SerializeField] protected GameObject m_prefab;
    [SerializeField] protected Transform m_container;

    protected List<GameObject> m_pool = new List<GameObject>();
   
    public T GetAvailable<T>() where T:Component
    {
        GameObject obj = GetAvailable();
        return obj.GetComponent<T>();
    }

    public GameObject GetAvailable()
    {
        GameObject availableObj = null;
        for (int i = 0; i < m_pool.Count; ++i)
        {
            if(!m_pool[i].activeSelf)
            {
                availableObj = m_pool[i];
            }
        }

        if(availableObj == null)
        {
            availableObj = GameObject.Instantiate(m_prefab);
            availableObj.transform.parent = m_container;
            m_pool.Add(availableObj);
        }

        availableObj.SetActive(true);

        return availableObj;
    }
}
