using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class Main : Singleton<Main> {

    public GameObject m_player1Prefab;
    public GameObject m_player2Prefab;

    public float m_playerSpawnOffset = 5.0f;

    public List<PlayerController> m_players = new List<PlayerController>();
    public List<PlayerData> m_playerDatas = new List<PlayerData>();

    protected bool m_scoreDirty = false;

    public enum GameState
    {
        LOADING,
        TITLE,
        STARTED,
        ENDED,
        RESTARTING,
    }

    protected GameState m_state = GameState.LOADING;

    protected override void Awake()
    {
        base.Awake();

        DOTween.Init(true, false);
        Camera.main.eventMask = ~Camera.main.eventMask;
        SceneManager.LoadScene(1, LoadSceneMode.Additive);
    }

    protected void Start()
    {
        StartCoroutine(HandleLoading());
    }

    protected IEnumerator HandleLoading()
    {
        LevelGenerator.Instance.Init();
        CameraController.Instance.transform.position += new Vector3(LevelGenerator.Instance.m_totalWidth / 2.0f, 0.0f, LevelGenerator.Instance.m_totalHeight / 2.0f);
        LevelGenerator.Instance.GenerateLevel();

        yield return new WaitForEndOfFrame();

        BaseActor player1 = CreateActor(m_player1Prefab, LevelGenerator.Instance.GetPlayer1SpawnTile().transform.position + Vector3.up * m_playerSpawnOffset);
        player1.m_playerId = PlayerData.PlayerId.PLAYER_1;
        player1.transform.rotation = Utils.ALIGNED_ROTATIONS[1];
        m_players[0].m_character = player1;

        BaseActor player2 = CreateActor(m_player2Prefab, LevelGenerator.Instance.GetPlayer2SpawnTile().transform.position + Vector3.up * m_playerSpawnOffset);
        player2.m_playerId = PlayerData.PlayerId.PLAYER_2;
        player2.transform.rotation = Utils.ALIGNED_ROTATIONS[3];
        m_players[1].m_character = player2;

        m_state = GameState.TITLE;
        UIManager.Instance.m_blackout.Hide();
    }

    public void StartGame()
    {
        if(m_state == GameState.STARTED)
        {
            return;
        }

        for (int i = 0; i < m_players.Count; ++i)
        {
            m_players[i].m_character.Respawn();
        }
        UIManager.Instance.m_scoreDisplay.Show();
        UIManager.Instance.m_timerDisplay.Show();

        m_state = GameState.STARTED;
    }
    
    public void EndGame()
    {
        if(m_state == GameState.ENDED)
        {
            return;
        }
        m_state = GameState.ENDED;
        UIManager.Instance.m_timerDisplay.Hide();

        int p1Score = GetPlayerData(PlayerData.PlayerId.PLAYER_1).m_score;
        int p2Score = GetPlayerData(PlayerData.PlayerId.PLAYER_2).m_score;
        if(p1Score == p2Score)
        {
            // show tie screen
            UIManager.Instance.m_endTieDisplay.Show();
        }
        else if(p1Score > p2Score)
        {
            // show p1 win
            UIManager.Instance.m_endP1Display.Show();
            CameraController.Instance.m_targets.Remove(m_players[1].m_character.transform);
        }
        else
        {
            // show p2 win
            UIManager.Instance.m_endP2Display.Show();
            CameraController.Instance.m_targets.Remove(m_players[0].m_character.transform);
        }
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
        switch (m_state)
        {
            case GameState.ENDED:
            case GameState.STARTED:
                UpdateGame();
                break;
        }
    }

    protected void UpdateGame()
    {
        for (int i = 0; i < m_players.Count; ++i)
        {
            m_players[i].ControlledUpdate();
        }

        for (int i = 0; i < BaseObject.s_allObjects.Count; ++i)
        {
            BaseObject obj = BaseObject.s_allObjects[i];
            if (obj.gameObject.activeSelf)
            {
                obj.ControlledUpdate();
            }
        }

        if (m_scoreDirty && m_state == GameState.STARTED)
        {
            m_scoreDirty = false;
            EventManager.OnScoreChange.Dispatch();
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

    public void ModifyScore(PlayerData.PlayerId playerId, int scoreDelta)
    {
        if(scoreDelta == 0)
        {
            return;
        }
        //Debug.LogFormat("Score Change {0}: {1}", playerId, scoreDelta);

        PlayerData player = GetPlayerData(playerId);
        if(player != null)
        {
            player.m_score += scoreDelta;
            m_scoreDirty = true;
        }
    }

    public void RestartGame()
    {
        if(m_state == GameState.RESTARTING)
        {
            return;
        }

        m_state = GameState.RESTARTING;
        StartCoroutine(HandleRestartGane());
    }

    protected IEnumerator HandleRestartGane()
    {
        BaseUIElement blackout = UIManager.Instance.m_blackout;
        blackout.Show();
        yield return new WaitForSeconds(blackout.m_transitionInDelay + blackout.m_transitionInTime + 1.0f);

        SceneManager.LoadScene(0);
    }
}
