using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace APG.Environment
{
    public class EnvGoal : MonoBehaviour
    {
        public event Action OnGoalTriggered;
        public event Action OnGoalDestroyed;

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("Goal Entered");
            PlayerAgent playerAgent = other.GetComponent<PlayerAgent>();
            if (playerAgent)
                OnGoalTriggered?.Invoke();
        }

        private void OnDestroy() {
            OnGoalDestroyed?.Invoke();
        }
    }
}
