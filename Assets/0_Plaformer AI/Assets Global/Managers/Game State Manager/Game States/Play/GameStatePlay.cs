using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG
{
    [CreateAssetMenu(fileName = "Game State Play", menuName = "Scriptable Objects/GameStates/Play")]
    public class GameStatePlay : GameState
    {
        public override void InitializeGameState(GameStateManager manager)
        {
            base.InitializeGameState(manager);

            if (displayStateDebugInfo)
                Debug.Log("Switched to Game State Play", this);
        }

    }
}
