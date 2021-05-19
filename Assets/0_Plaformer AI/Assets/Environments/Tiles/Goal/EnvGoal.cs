using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace APG.Environment
{
    public class EnvGoal : MonoBehaviour
    {
        public event Action OnGoalTriggered;

        private void OnTriggerEnter(Collider other)
        {
            PlayerAgent playerAgent = other.GetComponent<PlayerAgent>();
            if (playerAgent)
                OnGoalTriggered?.Invoke();
        }
    }
}
