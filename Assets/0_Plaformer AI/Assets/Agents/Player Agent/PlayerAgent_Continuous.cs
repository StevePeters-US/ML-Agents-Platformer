using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

namespace APG
{
    public class PlayerAgent_Continuous : PlayerAgent
    {
        // 4 Continuous Actions
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            // If we want to use more traditional ai methods we'd overwrite the actions here
             var continuousActionsOut = actionsOut.ContinuousActions;
            continuousActionsOut.Clear();

             continuousActionsOut[0] = _playerInputHandler.moveDirection.x;
             continuousActionsOut[1] = _playerInputHandler.moveDirection.z;
             continuousActionsOut[2] = _playerInputHandler.lookRight;
             continuousActionsOut[3] = _playerInputHandler.lookUp;        
        }

        // Convert output from model into usable variables that can be used to pilot the agent.
        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            agentMoveInputDirection.x = Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f);
            agentMoveInputDirection.y = 0;
            agentMoveInputDirection.z = Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f);

            agentLookDir.x = Mathf.Clamp(actionBuffers.ContinuousActions[2], -1f, 1f);
            agentLookDir.y = Mathf.Clamp(actionBuffers.ContinuousActions[3], -1f, 1f);
        }
    }
}