using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : Singleton<Main> {

    public GameObject m_player1Prefab;
    public GameObject m_player2Prefab;
    
    protected List<PlayerController> m_players = new List<PlayerController>();


    protected void Start()
    {
        LevelGenerator.Instance.GenerateLevel();

        CreatePlayer(m_player1Prefab, LevelGenerator.Instance.GetPlayer1SpawnTile().transform.position + Vector3.up * 2.0f).m_character.SetColor(Color.blue);
        CreatePlayer(m_player2Prefab, LevelGenerator.Instance.GetPlayer2SpawnTile().transform.position + Vector3.up * 2.0f).m_character.SetColor(Color.red);
    }

    protected PlayerController CreatePlayer(GameObject prefab, Vector3 spawnPos)
    {
        GameObject playerObj = Instantiate(prefab);
        PlayerController player = playerObj.GetComponent<PlayerController>();
        player.m_character.SetSpawnPos(spawnPos, true);
        m_players.Add(player);
        CameraController.Instance.m_targets.Add(player.transform);
        return player;
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
}
