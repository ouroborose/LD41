using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseBuildingPart : BaseObject {
    public enum BuildingPartType
    {
        Story,
        Roof,
    }

    public BuildingPartType m_type = BuildingPartType.Story;
    public BaseTile m_currentTile;

    public int m_maxHp = 3;

    protected int m_currentHp = 0;

    public virtual void Reset()
    {
        m_currentHp = m_maxHp;
    }
}
