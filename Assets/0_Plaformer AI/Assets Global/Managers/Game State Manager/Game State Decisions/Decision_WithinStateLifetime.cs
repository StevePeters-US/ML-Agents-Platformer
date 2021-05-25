using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG
{
    [CreateAssetMenu(fileName = "Game State Decision Within State Lifetime", menuName = "Scriptable Objects/GameStateDecisions/Within State Lifetime")]

    public class Decision_WithinStateLifetime : GameStateDecision
    {
        public float lifeTime = 5.0f;

        public override bool Decide(GameStateManager manager, GameState state)
        {
            float stateTimeElapsed = Time.time - manager.TimeStateStarted;
            
            bool decision = !(stateTimeElapsed < lifeTime);

            if (state.displayStateDebugInfo)
                Debug.Log("Decision - Within State Lifetime : " + decision);

            return decision;
        }
    }
}
