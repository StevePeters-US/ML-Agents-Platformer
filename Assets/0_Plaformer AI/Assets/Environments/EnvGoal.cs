using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace APG.Environment
{
    public class EnvGoal : EnvironmentNode
    {
        public event Action OnGoalTriggered;

        public EnvGoal(bool isTraversable, Vector3 worldPos, int gridX, int gridY) : base(isTraversable, worldPos, gridX, gridY) {
        }

        /*        private EnvironmentManager _envManager;
       public EnvironmentManager envManager { get => _envManager; }*/


    /*    private void Awake()
        {
            if (!UpdateEnvironmentManager())
                Debug.Log("No environment manager found", this);
        }
        public bool UpdateEnvironmentManager()
        {
            *//*_envManager = GetComponentInParent<EnvironmentManager>();
            return _envManager;*//*
        }*/

        private void OnTriggerEnter(Collider other)
        {
            PlayerAgent playerAgent = other.GetComponent<PlayerAgent>();
            if (playerAgent)
                OnGoalTriggered?.Invoke();
        }
    }
}
