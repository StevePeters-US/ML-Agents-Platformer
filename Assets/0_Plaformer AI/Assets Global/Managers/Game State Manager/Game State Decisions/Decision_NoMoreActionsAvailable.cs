using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG
{
    [CreateAssetMenu(fileName = "Game State Decision Does State Have Any More Actions", menuName = "Scriptable Objects/GameStateDecisions/Does State Have Any More Actions")]
    public class Decision_NoMoreActionsAvailable : GameStateDecision
    {
        public override bool Decide(GameStateManager manager, GameState state)
        {
            bool decision = state.CurrentActionIndex > state.actions.Length - 1;

            if (state.displayStateDebugInfo)
                Debug.Log("Decision - No more actions available : " + decision, this);

            return decision;
        }
    }
}
