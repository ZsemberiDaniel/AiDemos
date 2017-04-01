using System;
using UnityEditor;
using UnityEngine;

namespace Steering {
    [CreateAssetMenu(menuName = "Steering Behaviours/Velocity matching")]
    [System.Serializable]
    public class VelocityMatchSteeringBehaviour : SteeringBehaviour {

        /// <summary>
        /// How much time it should take to reach the target in theory
        /// </summary>
        private float timeToTarget = 0.1f;

        private Vector3 targetLastPosition;

        public override bool CanChangeVelocity() {
            return true;
        }

        public override bool CanChangeRotation() {
            return false;
        }

        public override SteeringOutput GetSteering(AutonomousAgent character, WeightedSteeringBehaviour agentLocalBehaviour) {
            SteeringOutput output = new SteeringOutput();

            if (targetLastPosition != null) {
                output = VelocityMatch(character, agentLocalBehaviour.target.transform.position, targetLastPosition, Time.fixedDeltaTime);
            }
            targetLastPosition = agentLocalBehaviour.target.transform.position;

            return output;
        }

        /// <summary>
        /// Velocity matching using the target's current position and it's last position. Plus the time between the two positions
        /// </summary>
        public SteeringOutput VelocityMatch(AutonomousAgent character, Vector3 targetPosition, Vector3 targetLastPosition, float timeBetween) {
            // get the velocity of the target based on last position v = s / t
            return VelocityMatch(character, (targetPosition - targetLastPosition) / timeBetween);
        }

        /// <summary>
        /// Velocity matching using the target's position and it's velocity
        /// </summary>
        public SteeringOutput VelocityMatch(AutonomousAgent character, Vector3 velocity) {
            SteeringOutput output = new SteeringOutput();

            // get acceleration a = deltaV / deltaT
            output.linear = (output.linear - character.Velocity) / timeToTarget;

            return output;
        }

        public override void DrawGizmos(Transform characterTransform) {
        }

        public override void DrawOnGUI() {
            // Time to target
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Time to reach target");
            timeToTarget = EditorGUILayout.FloatField(timeToTarget);
            GUILayout.EndHorizontal();
        }
    }

    public class VelocityMatch : Attribute { }
}