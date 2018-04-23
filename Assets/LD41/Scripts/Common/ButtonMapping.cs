using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ButtonMapping", menuName = "Data/ButtonMapping", order = 1)]
public class ButtonMapping : ScriptableObject
{
    public KeyCode m_upKey = KeyCode.UpArrow;
    public KeyCode m_downKey = KeyCode.DownArrow;
    public KeyCode m_rightKey = KeyCode.RightArrow;
    public KeyCode m_leftKey = KeyCode.LeftArrow;

    public KeyCode m_jumpKey = KeyCode.Comma;
    public KeyCode m_actionKey = KeyCode.Period;
}
