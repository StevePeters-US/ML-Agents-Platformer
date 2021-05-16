using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG
{    public abstract class GameStateDecision : ScriptableObject
    {
        // True to move to next state
        public abstract bool Decide(GameStateManager manager, GameState state);
    }
}
