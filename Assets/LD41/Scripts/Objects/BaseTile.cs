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

    public void RemoveBuildingPart(BaseBuildingPart part, bool shouldBreak = false)
    {
        if(m_buildingParts.Remove(part))
        {
            part.m_owner = PlayerData.PlayerId.UNASSIGNED;
            part.m_currentTile = null;
            if(shouldBreak)
            {
                part.Break(Vector3.up);
            }
        }
    }
    
    public void OnPartBreak(BaseBuildingPart part)
    {
        int start = m_buildingParts.IndexOf(part);
        if(start < 0)
        {
            return; // part not found
        }

        int numFloors = m_buildingParts.Count;
        bool roofRemoved = part.m_type == BaseBuildingPart.BuildingPartType.Roof;
        int scoreValue = start + 1;
        int scoreLost = -scoreValue;

        RemoveBuildingPart(part);
        while(start < m_buildingParts.Count)
        {
            scoreValue++;
            scoreLost -= scoreValue;

            part = m_buildingParts[start];
            if(part.m_type == BaseBuildingPart.BuildingPartType.Roof)
            {
                roofRemoved = true;
            }
            RemoveBuildingPart(part, true);
        }

        if(roofRemoved)
        {
            for(int i = numFloors; i > 0; --i)
            {
                scoreLost -= i;
            } 
        }

        AudioManager.Instance.PlayExplosionOneShot();

        Main.Instance.ModifyScore(m_owner, scoreLost);
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
        Color ownerColor = Main.Instance.GetPlayerData(owner).m_color;
        //SetColor(ownerColor);
        int scoreLost = 0;
        int scoreGained = 0;
        int totalValue = 0;

        bool roofed = GetTopPart().m_type == BaseBuildingPart.BuildingPartType.Roof;
        if(roofed)
        {
            ownerColor.r += 0.5f;
            ownerColor.g += 0.5f;
            ownerColor.b += 0.5f;
        }

        for(int i = 0; i < m_buildingParts.Count; ++i)
        {
            BaseBuildingPart part = m_buildingParts[i];
            part.SetColor(ownerColor);

            if(roofed)
            {
                part.RoofUpgrade();
            }

            int scoreValue = i + 1;
            if (part.m_owner != owner)
            {
                if (i < m_buildingParts.Count - 1)
                {
                    scoreLost -= scoreValue;
                    scoreGained += scoreValue;
                }
                else
                {
                    scoreGained += scoreValue;
                }
            }

            totalValue += scoreValue;
            part.m_owner = owner;
        }

        if (roofed)
        {
            scoreGained += totalValue;
        }

        Vector3 pos = m_buildingParts[m_buildingParts.Count - 1].transform.position;

        Main.Instance.ModifyScore(m_owner, scoreLost);
        m_owner = owner;
        Main.Instance.ModifyScore(m_owner, scoreGained);

        VFXManager.Instance.ThrowScoreDoober(pos, m_owner, scoreGained, roofed);
    }
}
