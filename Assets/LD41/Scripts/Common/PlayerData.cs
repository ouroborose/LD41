using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData  {

    public enum PlayerId : int
    {
        UNASSIGNED = -1,
        PLAYER_1 = 0,
        PLAYER_2 = 1,
        MAX_PLAYERS,
    }

    public Color m_color = Color.white;
    public int m_score = 0;
    public PlayerController m_player;
}
