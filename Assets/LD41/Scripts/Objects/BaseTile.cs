using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseTile : BaseObject {
    protected List<BaseBuildingPart> m_buildingParts = new List<BaseBuildingPart>();

    public void AddBuildingPart(BaseBuildingPart part)
    {
        Vector3 pos = transform.position;
        part.transform.position = pos + Vector3.up * m_buildingParts.Count;
        part.transform.rotation = Utils.GetClosestAlignedRotation(part.transform.rotation);
        m_buildingParts.Add(part);
        part.m_currentTile = this;
        part.m_rigidbody.isKinematic = true;
    }
    
}
