using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG {
    [CreateAssetMenu(fileName = "Game State Training Environment Agent", menuName = "Scriptable Objects/GameStates/Training Environment Agent")]

    public class GameStateTrainingEnvAgent : GameState {
        public override void InitializeGameState(GameStateManager manager) {
            base.InitializeGameState(manager);

            if (displayStateDebugInfo)
                Debug.Log("Switched to Game State Training Environment Agent", this);
        }
    }
}
