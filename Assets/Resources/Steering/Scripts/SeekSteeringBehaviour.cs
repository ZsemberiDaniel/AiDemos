using System;
using UnityEditor;
using UnityEngine;

namespace Steering {
    [CreateAssetMenu(menuName = "Steering Behaviours/Seeking")]
    public class SeekSteeringBehaviour : SteeringBehaviour {

        /// <summary>
        /// At what radius to the target the agent should start slowing down
        /// </summary>
        [SerializeField]
        private float slowDownRadius = 3f;

        /// <summary>
        /// At what radius to the target the agent should stop
        /// </summary>
        [SerializeField]
        private float targetRadius = 1f;

        /// <summary>
        /// How much time it should take to reach the target in theory
        /// </summary>
        [SerializeField]
        private float timeToTarget = 0.1f;

        public override SteeringOutput GetSteering(AutonomousAgent character, WeightedSteeringBehaviour agentLocalBehaviour) {
            SteeringOutput steering = new SteeringOutput();

            // Get the direction
            Vector3 direction = agentLocalBehaviour.target.position - character.transform.position;
            float distance = direction.magnitude;

            // We are inside stopping radius
            if (distance < targetRadius) {
                steering.linear = -character.Velocity;
                return steering;
            }

            float targetSpeed;
            // We are inside the slowdown radius but not the stopping radius
            if (distance < slowDownRadius) {
                // start slowing down linearly
                targetSpeed = distance / (slowDownRadius - targetRadius) * character.maxSpeed;
            } else { // we are outside of slow down radius
                targetSpeed = character.maxSpeed;
            }

            // combines the direction and the speed
            Vector3 targetVelocity = direction.normalized;
            targetVelocity *= targetSpeed;

            // Calculate acceleration because we can't just change speed immediatly
            // Calculate delta v then divide it by t because a = v / t
            steering.linear = targetVelocity - character.Velocity;
            steering.linear /= timeToTarget;

            // Limit acceleration to max acceleration
            if (steering.linear.magnitude > character.maxAcceleration) {
                steering.linear.Normalize();
                steering.linear *= character.maxAcceleration;
            }

            return steering;
        }

        public override void DrawOnGUI() {
            // Slow down radius
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Slow down radius");
            slowDownRadius = EditorGUILayout.FloatField(slowDownRadius);
            GUILayout.EndHorizontal();

            // Target stop radius
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Stop radius");
            targetRadius = EditorGUILayout.FloatField(targetRadius);
            GUILayout.EndHorizontal();

            // Time to target
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Time to reach target");
            timeToTarget = EditorGUILayout.FloatField(timeToTarget);
            GUILayout.EndHorizontal();
        }

        public override void DrawGizmos(Transform carachterTransform) {
            Gizmos.color = new Color(1f, 0.59608f, 0f);
            Gizmos.DrawWireSphere(carachterTransform.position, slowDownRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(carachterTransform.position, targetRadius);
        }
    }

    public class Seek : Attribute { }
}
