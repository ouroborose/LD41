using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EventManager {
    public static readonly EventHandler OnScoreChange = new EventHandler();
}

public class EventHandler
{
    protected System.Action m_callbacks;

    public void Register(System.Action callBack)
    {
        m_callbacks += callBack;
    }

    public void Unregister(System.Action callback)
    {
        m_callbacks -= callback;
    }

    public void Dispatch()
    {
        if(m_callbacks != null)
        {
            m_callbacks.Invoke();
        }
    }
}

