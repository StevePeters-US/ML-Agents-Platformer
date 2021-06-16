using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG
{
    [CreateAssetMenu(fileName = "Game State Training Player Agent", menuName = "Scriptable Objects/GameStates/Training Player Agent")]
    public class GameStateTrainingPlayerAgent : GameState
    {
        public override void InitializeGameState(GameStateManager manager)
        {
            base.InitializeGameState(manager);

            if (displayStateDebugInfo)
                Debug.Log("Switched to Game State Training Player Agent", this);
        }
    }
}
