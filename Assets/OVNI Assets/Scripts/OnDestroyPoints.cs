using UnityEngine;
using System.Collections;

public class OnDestroyPoints : MonoBehaviour
{
    void OnDestroy()
    {
        GameVariables.player_points += 100;
    }
}