using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseTile : BaseObject {
    public enum TileType
    {
        GROUND,
        ROAD,
    }

    public TileType m_type = TileType.GROUND;

    public PlayerData.PlayerId m_owner = PlayerData.PlayerId.UNASSIGNED;
    protected List<BaseBuildingPart> m_buildingParts = new List<BaseBuildingPart>();

    public void AddBuildingPart(BaseBuildingPart part)
    {
        Vector3 pos = transform.position;
        part.transform.position = pos + Vector3.up * m_buildingParts.Count;
        part.transform.rotation = Utils.GetClosestAlignedRotation(part.transform.rotation);
        m_buildingParts.Add(part);
        part.Reset();
        part.m_currentTile = this;
        part.m_rigidbody.isKinematic = true;
    }
    
    public void OnPartBreak(BaseBuildingPart part)
    {
        int start = m_buildingParts.IndexOf(part);
        if(start < 0)
        {
            return; // part not found
        }

        m_buildingParts.Remove(part);
        while(start < m_buildingParts.Count)
        {
            BaseBuildingPart upperPart = m_buildingParts[start];
            upperPart.Break(Vector3.up);
            m_buildingParts.Remove(upperPart);
        }
    }

    public BaseBuildingPart GetTopPart()
    {
        if(m_buildingParts.Count <= 0)
        {
            return null;
        }

        return m_buildingParts[m_buildingParts.Count - 1];
    }

    public void Claim(PlayerData.PlayerId owner)
    {
        m_owner = owner;
        Color ownerColor = Main.Instance.GetPlayerData(m_owner).m_color;
        //SetColor(ownerColor);
        for(int i = 0; i < m_buildingParts.Count; ++i)
        {
            m_buildingParts[i].m_owner = m_owner;
            m_buildingParts[i].SetColor(ownerColor);
        }
    }
}
