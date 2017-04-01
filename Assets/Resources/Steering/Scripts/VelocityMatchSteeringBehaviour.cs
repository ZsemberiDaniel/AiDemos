using System;
using UnityEditor;
using UnityEngine;

namespace Steering {
    [CreateAssetMenu(menuName = "Steering Behaviours/Velocity matching")]
    public class VelocityMatchSteeringBehaviour : SteeringBehaviour {

        /// <summary>
        /// How much time it should take to reach the target in theory
        /// </summary>
        private float timeToTarget = 0.1f;

        private Vector3 lastTargetPosition;

        public override SteeringOutput GetSteering(AutonomousAgent character, WeightedSteeringBehaviour agentLocalBehaviour) {
            SteeringOutput output = new SteeringOutput();

            if (lastTargetPosition != null) { 
                // get the velocity of the target based on last position v = s / t
                output.linear = (agentLocalBehaviour.target.transform.position - lastTargetPosition) / (Time.fixedDeltaTime);
                // get acceleration a = deltaV / deltaT
                output.linear = (output.linear - character.Velocity) / timeToTarget;
            }
            lastTargetPosition = agentLocalBehaviour.target.transform.position;

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