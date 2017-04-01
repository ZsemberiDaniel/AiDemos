using System;
using UnityEditor;
using UnityEngine;

namespace Steering {
    [CreateAssetMenu(menuName = "Steering Behaviours/Fleeing")]
    [System.Serializable]
    public class FleeSteeringBehaviour : SteeringBehaviour {

        public override bool CanChangeVelocity() {
            return true;
        }

        public override bool CanChangeRotation() {
            return false;
        }

        /// <summary>
        /// In what radius the agent should detect enemies
        /// </summary>
        [SerializeField]
        protected float detectionRadius = 5f;
        public float DetectionRadius {
            set {
                detectionRadius = value;
            }
        }

        /// <summary>
        /// How much time it should take to reach the target in theory
        /// </summary>
        [SerializeField]
        protected float timeToTarget = 0.1f;

        public override SteeringOutput GetSteering(AutonomousAgent character, WeightedSteeringBehaviour agentLocalBehaviour) {
            return Flee(character, agentLocalBehaviour.target.transform.position);
        }

        public SteeringOutput Flee(AutonomousAgent character, Vector3 targetPosition) {
            SteeringOutput steering = new SteeringOutput();

            Vector3 direction = character.transform.position - targetPosition;
            float distance = direction.magnitude;

            Vector3 targetVelocity;
            // if the target is outside of detection radius stop
            if (distance > detectionRadius) {
                targetVelocity = new Vector3();
            } else if (distance / detectionRadius >= 0.9f) {
                // () will be a [0;0.1] number, to get a [0;1] multiply it by ten
                // the reverse it so the most outer point gets the 0
                float slowdown = 1f - (distance / detectionRadius - 0.9f) * 10f;

                targetVelocity = direction.normalized * character.maxSpeed * slowdown;
            } else {
                // go in that direction with tha max speed
                targetVelocity = direction.normalized * character.maxSpeed;
            }

            // make it acceleration
            steering.linear = targetVelocity - character.Velocity;
            steering.linear /= timeToTarget;

            return steering;
        }

        public override void DrawOnGUI() {
            // Deetction radius
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Detection radius");
            DetectionRadius = EditorGUILayout.FloatField(detectionRadius);
            GUILayout.EndHorizontal();

            // Time to target
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Time to reach target");
            timeToTarget = EditorGUILayout.FloatField(timeToTarget);
            GUILayout.EndHorizontal();
        }

        public override void DrawGizmos(Transform carachterTransform) {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(carachterTransform.position, detectionRadius);
        }
    }

    public class Flee : Attribute { }
}
