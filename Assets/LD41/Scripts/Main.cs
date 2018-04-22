using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main : Singleton<Main> {

    public GameObject m_player1Prefab;
    public GameObject m_player2Prefab;

    public float m_playerSpawnOffset = 5.0f;

    public List<PlayerController> m_players = new List<PlayerController>();
    public List<PlayerData> m_playerDatas = new List<PlayerData>();

    protected override void Awake()
    {
        base.Awake();

        SceneManager.LoadScene(1, LoadSceneMode.Additive);
    }

    protected void Start()
    {
        LevelGenerator.Instance.GenerateLevel();
        CameraController.Instance.transform.position += new Vector3(LevelGenerator.Instance.m_totalWidth / 2.0f, 0.0f, LevelGenerator.Instance.m_totalHeight / 2.0f);

        BaseActor player1 = CreateActor(m_player1Prefab, LevelGenerator.Instance.GetPlayer1SpawnTile().transform.position + Vector3.up * m_playerSpawnOffset);
        player1.m_playerId = PlayerData.PlayerId.PLAYER_1;
        m_players[0].m_character = player1;

        BaseActor player2 = CreateActor(m_player2Prefab, LevelGenerator.Instance.GetPlayer2SpawnTile().transform.position + Vector3.up * m_playerSpawnOffset);
        player2.m_playerId = PlayerData.PlayerId.PLAYER_2;
        m_players[1].m_character = player2;
    }
    

    protected BaseActor CreateActor(GameObject prefab, Vector3 spawnPos)
    {
        GameObject actorObj = Instantiate(prefab);
        BaseActor actor = actorObj.GetComponent<BaseActor>();
        actor.SetSpawnPos(spawnPos, true);
        CameraController.Instance.m_targets.Add(actor.transform);
        return actor;
    }

    protected void Update()
    {
        for(int i = 0; i < m_players.Count; ++i)
        {
            m_players[i].ControlledUpdate();
        }

        for(int i = 0; i < BaseObject.s_allObjects.Count; ++i)
        {
            BaseObject.s_allObjects[i].ControlledUpdate();
        }
    }

    public PlayerData GetPlayerData(PlayerData.PlayerId id)
    {
        if(id == PlayerData.PlayerId.UNASSIGNED || id == PlayerData.PlayerId.MAX_PLAYERS)
        {
            return null;
        }
        return m_playerDatas[(int)id];
    }
}
