﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : Singleton<LevelGenerator> {

    public GameObject m_roadTilePrefab;
    public GameObject m_groundTilePrefab;

    public GameObject m_buildingStoryPrefab;
    public GameObject m_buildingRoofPrefab;

    public int m_levelWidth = 4;
    public int m_levelHeight = 3;

    public int m_cityBlockSize = 3;
    public int m_maxBuildingHeight = 5;
    [Range(0.0f, 1.0f)]
    public float m_roofProbability = 0.5f;


    [SerializeField] protected BoxCollider m_groundCollider;

    public int m_totalWidth { get; protected set; }
    public int m_totalHeight { get; protected set; }
    protected BaseTile[,] m_tiles;

    public void GenerateLevel()
    {
        int stepSize = m_cityBlockSize + 1;
        m_totalWidth = m_levelWidth * stepSize + 1;
        m_totalHeight = m_levelHeight * stepSize + 1;
        m_tiles = new BaseTile[m_totalWidth, m_totalHeight];

        Debug.LogFormat("Generating Level: size: {0}", m_totalWidth);

        int spawnOffset = m_levelHeight / 2;

        // build blocks
        for (int blockX = 0; blockX < m_levelWidth; ++blockX)
        {
            for (int blockY = 0; blockY < m_levelHeight; ++blockY)
            {
                bool generateBuildings = !((blockX == 0 && blockY == spawnOffset) || (blockX == m_levelWidth-1 && blockY == m_levelHeight - 1 - spawnOffset));
                GenerateCityBlock(blockX * stepSize, blockY * stepSize, m_cityBlockSize, generateBuildings);
            }
        }

        // fill in roads
        for (int blockX = 0; blockX < m_totalWidth; ++blockX)
        {
            for (int blockY = 0; blockY < m_totalHeight; ++blockY)
            {
                GenerateRoad(blockX, blockY);
            }
        }
        
        m_groundCollider.center = new Vector3(Mathf.FloorToInt(m_totalWidth * 0.5f), -0.5f, Mathf.FloorToInt(m_totalHeight * 0.5f));
        m_groundCollider.size = new Vector3(m_totalWidth, 1, m_totalHeight);

        GetPlayer1SpawnTile().SetColor(Color.blue);
        GetPlayer2SpawnTile().SetColor(Color.red);
    }

    protected void GenerateCityBlock(int x, int y, int size, bool generateBuildings)
    {
        ++x;
        ++y;
        int endX = x + size;
        int endY = y + size;
        while(x < endX)
        {
            while(y < endY)
            {
                //Debug.LogFormat("x:{0}, y:{1}", x, y);
                BaseTile tile = CreateTile(m_groundTilePrefab, x, y);
                if(generateBuildings)
                {
                    GenerateBuilding(tile, Random.Range(0, m_maxBuildingHeight + 1), Random.value < m_roofProbability);
                }
                ++y;
            }
            y -= size;
            ++x;
        }
    }

    public void GenerateBuilding(BaseTile tile, int height, bool addRoof = false)
    {
        if(height <= 0)
        {
            return;
        }

        for(int i = 0; i < height; ++i)
        {
            GameObject storyObj = Instantiate(m_buildingStoryPrefab, transform);
            BaseBuildingPart story = storyObj.GetComponent<BaseBuildingPart>();
            tile.AddBuildingPart(story);
        }

        if(addRoof)
        {
            GameObject roofObj = Instantiate(m_buildingRoofPrefab, transform);
            BaseBuildingPart roof = roofObj.GetComponent<BaseBuildingPart>();
            roof.transform.rotation = Utils.GetRandomAlignedRotation();
            tile.AddBuildingPart(roof);
        }
    }

    protected void GenerateRoad(int x, int y)
    {
        // TODO: determine road tile based on surrounding tiles
        CreateTile(m_roadTilePrefab, x, y);
    }

    protected BaseTile  CreateTile(GameObject prefab, int x, int y)
    {
        if (m_tiles[x, y] != null)
        {
            return null;
        }

        GameObject tileObj = Instantiate(prefab, transform);
        tileObj.transform.position = new Vector3(x, 0.0f, y);
        BaseTile tile = tileObj.GetComponent<BaseTile>();
        m_tiles[x, y] = tile;
        return tile;
    }

    public BaseTile GetPlayer1SpawnTile()
    {
        int x = m_cityBlockSize / 2 + 1;
        int y = (m_levelHeight / 2 + 1) * (m_cityBlockSize + 1) - (m_cityBlockSize/2 + 1);
        return GetTile(x, y);
    }

    public BaseTile GetPlayer2SpawnTile()
    {
        int x = m_totalWidth - (m_cityBlockSize / 2 + 1) - 1;
        int y = m_totalHeight - ((m_levelHeight / 2 + 1) * (m_cityBlockSize + 1) - (m_cityBlockSize / 2 + 1)) - 1;
        return GetTile(x, y);
    }

    public BaseTile GetTile(int x, int y)
    {
        if(x < 0 || y < 0 || x >= m_totalWidth || y >= m_totalHeight)
        {
            return null;
        }
        return m_tiles[x, y];
    }

    public BaseTile GetClosestTile(Vector3 pos)
    {
        int x = Mathf.RoundToInt(pos.x);
        int y = Mathf.RoundToInt(pos.z);
        return GetTile(x, y);
    }
}