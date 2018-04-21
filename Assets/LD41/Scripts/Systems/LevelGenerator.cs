using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : Singleton<LevelGenerator> {

    public GameObject m_roadTilePrefab;
    public GameObject m_groundTilePrefab;

    public GameObject m_buildingStoryPrefab;
    public GameObject m_buildingRoofPrefab;

    public int m_levelSize = 3;

    public int m_cityBlockSize = 3;
    public int m_maxBuildingHeight = 5;
    [Range(0.0f, 1.0f)]
    public float m_roofProbability = 0.5f;


    public BoxCollider m_groundCollider;

    protected int m_totalSize;
    protected BaseTile[,] m_tiles;

    public void GenerateLevel()
    {
        int stepSize = m_cityBlockSize + 1;
        m_totalSize = m_levelSize * stepSize + 1;
        m_tiles = new BaseTile[m_totalSize, m_totalSize];

        Debug.LogFormat("Generating Level: size: {0}", m_totalSize);

        // build blocks
        for (int blockX = 0; blockX < m_levelSize; ++blockX)
        {
            for (int blockY = 0; blockY < m_levelSize; ++blockY)
            {
                bool generateBuildings = !(blockX == 0 && blockY == m_levelSize - 1 || blockY == 0 && blockX == m_levelSize - 1);
                GenerateCityBlock(blockX * stepSize, blockY * stepSize, m_cityBlockSize, generateBuildings);
            }
        }

        // fill in roads
        for (int blockX = 0; blockX < m_totalSize; ++blockX)
        {
            for (int blockY = 0; blockY < m_totalSize; ++blockY)
            {
                GenerateRoad(blockX, blockY);
            }
        }

        int halfSize = Mathf.FloorToInt(m_totalSize * 0.5f);
        m_groundCollider.center = new Vector3(halfSize, -0.5f, halfSize);
        m_groundCollider.size = new Vector3(m_totalSize, 1, m_totalSize);

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
                    GenerateBuilding(tile, Random.Range(0, m_maxBuildingHeight), Random.value < m_roofProbability);
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
        return m_tiles[m_cityBlockSize / 2 + 1, m_levelSize * (m_cityBlockSize+1) - m_cityBlockSize/2 -1];
    }

    public BaseTile GetPlayer2SpawnTile()
    {
        return m_tiles[ m_levelSize * (m_cityBlockSize + 1) - m_cityBlockSize / 2 - 1, m_cityBlockSize / 2 + 1];
    }
}
