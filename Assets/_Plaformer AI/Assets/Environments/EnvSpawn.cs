using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace APG.Environment
{
    public class EnvSpawn : EnvironmentNode
    {
        [SerializeField] private Vector3 spawnOffset;

        public EnvSpawn(bool isTraversable, Vector3 worldPos, int gridX, int gridY) : base(isTraversable, worldPos, gridX, gridY) {
        }


#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Handles.DrawWireCube(transform.position + spawnOffset, new Vector3(0.5f, 0.5f, 0.5f));
        }
#endif

        public void ResetSpawnedAgent(PlayerAgent playerAgent)
        {
            playerAgent.Rigidbody.velocity = Vector3.zero;
            playerAgent.Rigidbody.angularVelocity = Vector3.zero;

            playerAgent.transform.position = transform.position + spawnOffset;
        }
    }
}
