using System;
using UnityEditor;
using UnityEngine;

namespace Steering {
    [CreateAssetMenu(menuName = "Steering Behaviours/Evade")]
    [System.Serializable]
    public class EvadeSteeringBehaviour : FleeSteeringBehaviour {

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
                output = Evade(character, agentLocalBehaviour.target.transform.position, targetLastPostion, Time.fixedDeltaTime);
            }
            targetLastPostion = agentLocalBehaviour.target.transform.position;

            return output;
        }

        /// <summary>
        /// Evade using the target's current position and it's last position. Plus the time between the two positions
        /// </summary>
        public SteeringOutput Evade(AutonomousAgent character, Vector3 targetPosition, Vector3 targetLastPostion, float timeBetween) {
            // v = s / t
            Vector3 velocityOfTarget = (targetPosition - targetLastPostion) / timeBetween;

            return Evade(character, targetPosition, velocityOfTarget);
        }

        /// <summary>
        /// Evade using the target's position and it's velocity
        /// </summary>
        public SteeringOutput Evade(AutonomousAgent character, Vector3 targetPosition, Vector3 velocityOfTarget) {
            SteeringOutput output = new SteeringOutput();

            // it calculates how long it would take the agent to get to the postion of the target
            // with the current speed. If that is too much it limits that down to maxPrediction.
            // We need that time to calculate where the target will be in the future in that time
            // so we can set the target to that position and flee from it.

            float distance = (targetPosition - character.transform.position).magnitude;
            float agentSpeed = character.Velocity.magnitude;

            float prediction;
            if (agentSpeed <= distance / maxPrediction) {
                prediction = maxPrediction;
            } else {
                prediction = distance / agentSpeed;
            }

            // in which direction to evade
            Vector3 evadeDirection = character.transform.position - (targetPosition + velocityOfTarget * prediction);

            // what velocity we want to reach
            Vector3 targetVelocity;
            // target is out of detection radius
            if (distance > detectionRadius) {
                targetVelocity = new Vector3();
            } else {
                targetVelocity = evadeDirection.normalized * character.maxSpeed;
            }

            // make acceleration
            output.linear = targetVelocity - character.Velocity;
            output.linear /= timeToTarget;

            return output;
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

    public class Evade : Attribute { }
}