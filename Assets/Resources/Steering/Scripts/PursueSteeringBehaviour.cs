using System;
using UnityEditor;
using UnityEngine;

namespace Steering {
    [CreateAssetMenu(menuName = "Steering Behaviours/Pursue")]
    [System.Serializable]
    public class PursueSteeringBehaviour : SeekSteeringBehaviour {

        private Vector3 targetLastPostion;
        private float maxPrediction = 1f;

        public override bool CanChangeVelocity() {
            return true;
        }

        public override bool CanChangeRotation() {
            return false;
        }

        public override SteeringOutput GetSteering(AutonomousAgent character, WeightedSteeringBehaviour agentLocalBehaviour) {
            SteeringOutput output = new SteeringOutput();

            if (targetLastPostion != null) {
                output = Pursue(character, agentLocalBehaviour.target.transform.position, targetLastPostion, Time.fixedDeltaTime);
            }
            targetLastPostion = agentLocalBehaviour.target.transform.position;

            return output;
        }

        /// <summary>
        /// Pursue using the target's current position and it's last position. Plus the time between the two positions
        /// </summary>
        public SteeringOutput Pursue(AutonomousAgent character, Vector3 targetPosition, Vector3 targetLastPosition, float timeBetween) {
            // v = s / t
            Vector3 targetVelocity = (targetPosition - targetLastPostion).normalized / timeBetween;

            return Pursue(character, targetPosition, targetVelocity);
        }

        /// <summary>
        /// Pursue using the target's position and it's velocity
        /// </summary>
        public SteeringOutput Pursue(AutonomousAgent character, Vector3 targetPosition, Vector3 targetVelocity) {
            // it calculates how long it would take the agent to get to the postion of the target
            // with the current speed. If that is too much it limits that down to maxPrediction.
            // We need that time to calculate where the target will be in the future in that time
            // so we can set the target to that position.

            float distance = (targetPosition - character.transform.position).magnitude;
            float agentSpeed = character.Velocity.magnitude;

            float prediction;
            if (agentSpeed <= distance / maxPrediction) {
                prediction = maxPrediction;
            } else {
                prediction = distance / agentSpeed;
            }


            return base.Seek(character, targetPosition + targetVelocity * prediction);
        }

        public override void DrawGizmos(Transform characterTransform) {
            base.DrawGizmos(characterTransform);
        }

        public override void DrawOnGUI() {
            base.DrawOnGUI();

            // Time to target
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Max prediction time");
            maxPrediction = EditorGUILayout.FloatField(maxPrediction);
            GUILayout.EndHorizontal();
        }
    }

    public class Pursue : Attribute { }
}