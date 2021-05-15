using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG
{
    [CreateAssetMenu(fileName = "Game State Decision Does Player Have More Lives", menuName = "Scriptable Objects/GameStateDecisions/Does Player Have More Lives")]
    public class DoesPlayerHaveMoreLives : GameStateDecision
    {
        public override bool Decide(GameStateManager manager, GameState state)
        {
            bool decision = true;
            Debug.Log("implement Lives system", this);

            if (state.displayStateDebugInfo)
                Debug.Log("Decision - Does Player Have More Lives : " + decision);

            return decision;
        }
    }
}
