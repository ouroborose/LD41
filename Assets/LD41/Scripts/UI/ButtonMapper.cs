using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ButtonMapper : MonoBehaviour {
    public readonly System.Array ENUM_VALUES = System.Enum.GetValues(typeof(KeyCode));

    public ButtonMapping m_buttonMapping;

    public TextMeshProUGUI m_up;
    public TextMeshProUGUI m_down;
    public TextMeshProUGUI m_left;
    public TextMeshProUGUI m_right;
    
    public TextMeshProUGUI m_action;
    public TextMeshProUGUI m_jump;

    protected Coroutine m_assignmentCoroutine;
    protected TextMeshProUGUI m_lastLabel;

    protected void Start()
    {
        m_up.text = m_buttonMapping.m_upKey.ToString();
        m_down.text = m_buttonMapping.m_downKey.ToString();
        m_left.text = m_buttonMapping.m_leftKey.ToString();
        m_right.text = m_buttonMapping.m_rightKey.ToString();

        m_action.text = m_buttonMapping.m_actionKey.ToString();
        m_jump.text = m_buttonMapping.m_jumpKey.ToString();
    }

    public void AssignUp()
    {
        StartCoroutine(HandleAssignUp(m_up, (x) => { m_buttonMapping.m_upKey = x; }));
    }

    public void AssignDown()
    {
        StartCoroutine(HandleAssignUp(m_down, (x) => { m_buttonMapping.m_downKey = x; }));
    }

    public void AssignLeft()
    {
        StartCoroutine(HandleAssignUp(m_left, (x) => { m_buttonMapping.m_leftKey = x; }));
    }

    public void AssignRight()
    {
        StartCoroutine(HandleAssignUp(m_right, (x) => { m_buttonMapping.m_rightKey = x; }));
    }

    public void AssignAction()
    {
        StartCoroutine(HandleAssignUp(m_action, (x) => { m_buttonMapping.m_actionKey = x; }));
    }

    public void AssignJump()
    {
        StartCoroutine(HandleAssignUp(m_jump, (x) => { m_buttonMapping.m_jumpKey = x; }));
    }

    protected IEnumerator HandleAssignUp(TextMeshProUGUI label, System.Action<KeyCode> assignmentOperator)
    {
        m_lastLabel = label;
        label.color = Color.red;
        yield return new WaitForEndOfFrame();
        bool keyAssigned = false;
        while (!keyAssigned)
        {
            yield return new WaitForFixedUpdate();
            if(Input.GetMouseButtonDown(0))
            {
                CancelMapping();
                break; // quit because user clicked somewhere else
            }

            foreach(KeyCode key in ENUM_VALUES)
            {
                if(Input.GetKeyDown(key))
                {
                    assignmentOperator.Invoke(key);
                    label.text = key.ToString();
                    keyAssigned = true;
                    break;
                }
            }
        }

        label.color = Color.white;
    }

    public void CancelMapping()
    {
        if(m_assignmentCoroutine != null)
        {
            StopCoroutine(m_assignmentCoroutine);
        }

        if(m_lastLabel != null)
        {
            m_lastLabel.color = Color.white;
        }
    }
}
