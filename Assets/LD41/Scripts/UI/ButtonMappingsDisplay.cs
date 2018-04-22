using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonMappingsDisplay : BaseUIElement {

    public ButtonMapper m_player1Mapper;
    public ButtonMapper m_player2Mapper;

    public override void Hide(bool instant = false)
    {
        base.Hide(instant);
        m_player1Mapper.CancelMapping();
        //m_player2Mapper.CancelMapping();
    }
}
