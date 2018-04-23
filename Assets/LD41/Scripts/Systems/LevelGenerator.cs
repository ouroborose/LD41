using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : Singleton<LevelGenerator> {
    public static readonly int[] ROAD_INDEXES =
    {
        0, // Cross
        1,1, // Striaght
        2,2,2,2, // T
        3,3,3,3, // Corner
    };

    public static readonly Quaternion[] ROAD_ROTATIONS =
    {
        // Cross
        Quaternion.Euler(0,0,0),
        // Striaght
        Quaternion.Euler(0,0,0),
        Quaternion.Euler(0,90,0),
        // T
        Quaternion.Euler(0,0,0),
        Quaternion.Euler(0,90,0),
        Quaternion.Euler(0,180,0),
        Quaternion.Euler(0,270,0),
        // Corner
        Quaternion.Euler(0,0,0),
        Quaternion.Euler(0,90,0),
        Quaternion.Euler(0,180,0),
        Quaternion.Euler(0,270,0),
    };

    public static readonly bool[][] ROAD_PATTERNS =
    {
        // Cross
        new bool[]{true,true,true,true},
        // straights
        new bool[]{true,false,true,false},
        new bool[]{false,true,false,true},
        // T
        new bool[]{true,true,false,true},
        new bool[]{true,true,true,false},
        new bool[]{false,true,true,true},
        new bool[]{true,false,true,true},
        // Corners
        new bool[]{true,true,false,false},
        new bool[]{false,true,true,false},
        new bool[]{false,false,true,true},
        new bool[]{true,false,false,true},
    };

    public static bool[] s_sharedRoadPattern = new bool[4];

    public Texture[] m_damageTextures;

    public GameObject[] m_roadTilesPrefab;

    public GameObject m_groundTilePrefab;

    public GameObject m_buildingStoryPrefab;
    public GameObject m_buildingRoofPrefab;

    public Transform m_tilesContainer;
    public Transform m_buildingsContainer;

    public int m_levelWidth = 4;
    public int m_levelHeight = 3;

    public int m_cityBlockSize = 3;
    public int m_maxBuildingHeight = 5;
    [Range(0.0f, 1.0f)]
    public float m_roofProbability = 0.5f;


    public BoxCollider m_groundCollider;

    public int m_stepSize { get; protected set; }
    public int m_totalWidth { get; protected set; }
    public int m_totalHeight { get; protected set; }
    protected BaseTile[] m_tiles;

    public void Init()
    {
        m_stepSize = m_cityBlockSize + 1;
        m_totalWidth = m_levelWidth * m_stepSize + 1;
        m_totalHeight = m_levelHeight * m_stepSize + 1;
        m_tiles = new BaseTile[m_totalWidth * m_totalHeight];
    }

    public void GenerateLevel()
    {
        int spawnOffset = m_levelHeight / 2;

        // build blocks
        for (int blockX = 0; blockX < m_levelWidth; ++blockX)
        {
            for (int blockY = 0; blockY < m_levelHeight; ++blockY)
            {
                bool generateBuildings = !((blockX == 0 && blockY == spawnOffset) || (blockX == m_levelWidth-1 && blockY == m_levelHeight - 1 - spawnOffset));
                GenerateCityBlock(blockX * m_stepSize, blockY * m_stepSize, m_cityBlockSize, generateBuildings);
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

        BaseTile player1Tile = GetPlayer1SpawnTile();
        player1Tile.SetColor(Color.blue);
        BaseTile player2Tile = GetPlayer2SpawnTile();
        player2Tile.SetColor(Color.red);

        MeshFilter[] meshFilters = m_tilesContainer.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);
            i++;
        }
        m_tilesContainer.gameObject.AddComponent<MeshFilter>().mesh = new Mesh();
        m_tilesContainer.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
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
            GameObject storyObj = Instantiate(m_buildingStoryPrefab, m_buildingsContainer);
            BaseBuildingPart story = storyObj.GetComponent<BaseBuildingPart>();
            tile.AddBuildingPart(story);
        }

        if(addRoof)
        {
            GameObject roofObj = Instantiate(m_buildingRoofPrefab, m_buildingsContainer);
            BaseBuildingPart roof = roofObj.GetComponent<BaseBuildingPart>();
            roof.transform.rotation = Utils.GetRandomAlignedRotation();
            tile.AddBuildingPart(roof);
        }
    }

    protected void GenerateRoad(int x, int y)
    {
        if(IsOutOfBounds(x,y) || GetTile(x,y) != null)
        {
            return;
        }

        for(int i = 0; i < 4; ++i)
        {
            Vector2Int pos = Utils.CELL_DIRECTIONS[i];
            pos.x += x;
            pos.y += y;
            BaseTile tile = GetTile(pos.x, pos.y);
            s_sharedRoadPattern[i] = !IsOutOfBounds(pos.x, pos.y) && (tile == null || tile.m_type == BaseTile.TileType.ROAD);
        }

        int patternIndex = GetRoadPatternIndex(s_sharedRoadPattern);
        //Debug.LogFormat("{0} {1} - {2} {3} {4} {5} = {6}",x, y, s_sharedRoadPattern[0], s_sharedRoadPattern[1], s_sharedRoadPattern[2], s_sharedRoadPattern[3], patternIndex);
        CreateTile(m_roadTilesPrefab[ROAD_INDEXES[patternIndex]], x, y).transform.rotation = ROAD_ROTATIONS[patternIndex];
    }

    protected int GetRoadPatternIndex(bool[] pattern)
    {
        int patternIndex = 0;
        while (patternIndex < ROAD_PATTERNS.Length)
        {
            int i = 0;
            while(i < 4)
            {
                if (ROAD_PATTERNS[patternIndex][i] != pattern[i])
                {
                    break;
                }
                ++i;
            }

            if(i >= 4)
            {
                break;
            }
            patternIndex++;
        }
        return patternIndex;
    }

    protected int GetTileIndex(int x, int y)
    {
        return y * m_totalWidth + x;
    }

    protected BaseTile  CreateTile(GameObject prefab, int x, int y)
    {
        int index = GetTileIndex(x, y);
        if (IsOutOfBounds(x,y) || m_tiles[index] != null)
        {
            return null;
        }

        GameObject tileObj = Instantiate(prefab, m_tilesContainer);
        tileObj.transform.position = new Vector3(x, 0.0f, y);
        BaseTile tile = tileObj.GetComponent<BaseTile>();
        m_tiles[index] = tile;
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

    public bool IsOutOfBounds(int x, int y)
    {
        return x < 0 || y < 0 || x >= m_totalWidth || y >= m_totalHeight;
    }

    public BaseTile GetTile(int x, int y)
    {
        if(IsOutOfBounds(x,y))
        {
            return null;
        }
        return m_tiles[GetTileIndex(x, y)];
    }

    public BaseTile GetClosestTile(Vector3 pos)
    {
        int x = Mathf.RoundToInt(pos.x);
        int y = Mathf.RoundToInt(pos.z);
        return GetTile(x, y);
    }

    public Texture GetDamageTexture(int currentHp, int maxHp)
    {
        float hpPercent = (float)currentHp / maxHp;
        int index = Mathf.FloorToInt((1.0f - hpPercent) * (m_damageTextures.Length-1));
        return m_damageTextures[index];
    }
}
