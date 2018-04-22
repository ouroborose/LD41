using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils {

    public static readonly LayerMask ENVIRONMENT_LAYER_MASK = LayerMask.GetMask("Environment");

    public enum Direction : int
    {
        UP = 0,
        RIGHT = 1,
        DOWN = 2,
        LEFT = 3,
        NUM_DIRECTION = 4,
    }

    public static Vector2Int[] CELL_DIRECTIONS = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
    public static readonly Quaternion[] ALIGNED_ROTATIONS = { Quaternion.identity, Quaternion.Euler(0, 90, 0), Quaternion.Euler(0, 180, 0), Quaternion.Euler(0, 270, 0) };

    public static Quaternion GetClosestAlignedRotation(Quaternion rotation)
    {
        Quaternion best = Quaternion.identity;
        float bestAngle = float.MaxValue;
        for(int i = 0; i < ALIGNED_ROTATIONS.Length; ++i)
        {
            float angle = Quaternion.Angle(rotation, ALIGNED_ROTATIONS[i]);
            if (angle < bestAngle)
            {
                best = ALIGNED_ROTATIONS[i];
                bestAngle = angle;
            }
        }

        return best;
    }

    public static Quaternion GetRandomAlignedRotation()
    {
        return ALIGNED_ROTATIONS[Random.Range(0, 4)];
    }
}
