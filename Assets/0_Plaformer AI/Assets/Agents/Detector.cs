using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace APG
{
    [RequireComponent(typeof(BoxCollider))]
    public class Detector : MonoBehaviour
    {
        private PlayerAgent playerAgent;

        [SerializeField, Range(0f, 100f)]
        private float maxAcceleration = 10f, maxAirAcceleration = 1f;

        [SerializeField, Range(0f, 90f)]
        float maxGroundAngle = 25f;

        private float minGroundDotProduct;

        private bool onGround;

        void OnValidate()
        {
            //store the ground threshold in a field and compute it 
            minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        }

        private void Awake()
        {
            playerAgent = GetComponent<PlayerAgent>();
            OnValidate();
        }

        void FixedUpdate()
        {
            float acceleration = onGround ? maxAcceleration : maxAirAcceleration;
            float maxSpeedChange = acceleration * Time.deltaTime;
        }

        void OnCollisionEnter(Collision collision)
        {
            EvaluateCollision(collision);
        }

        void OnCollisionStay(Collision collision)
        {
            EvaluateCollision(collision);
        }

        void EvaluateCollision(Collision collision)
        {
            for (int i = 0; i < collision.contactCount; i++)
            {
                Vector3 normal = collision.GetContact(i).normal;
                onGround |= normal.y >= minGroundDotProduct;
            }
        }
    }
}
