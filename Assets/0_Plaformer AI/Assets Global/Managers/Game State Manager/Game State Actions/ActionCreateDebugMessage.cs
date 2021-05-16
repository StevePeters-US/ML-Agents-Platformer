using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG
{
    [CreateAssetMenu(fileName = "Game State Action Debug Message", menuName = "Scriptable Objects/GameStateActions/Debug Message")]
    public class ActionCreateDebugMessage : GameStateAction
    {
        public string debugMessage = "Debug Action Message";
        public override void Act(GameStateManager manager, GameState state)
        {
            base.Act(manager, state);
            Debug.Log(debugMessage, this);
        }
    }
}
