using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using APG.Environment;

namespace APG
{
    [CreateAssetMenu(fileName = "Game State Decision Have All Environments Completed", menuName = "Scriptable Objects/GameStateDecisions/Have All Environments Completed")]
    public class HaveAllEnvironmentsCompleted : GameStateDecision
    {
        public override bool Decide(GameStateManager manager, GameState state)
        {
            List<EnvironmentManager> sceneEnvManagers = EnvironmentalManagers.Instance.Items;

            foreach (EnvironmentManager envManager in sceneEnvManagers)
            {
                if (envManager.envCompleted == false)
                {
                    return false;
                }
            }

            // If all of the environments have completed
            return true;
        }     
    }
}
