using System;
using UnityEditor;
using UnityEngine;

namespace Steering { 
    public class AlignSteeringBehaviour : SteeringBehaviour {

        /// <summary>
        /// How much time it should take to reach the target in theory
        /// </summary>
        [SerializeField]
        private float timeToTarget = 0.1f;

        public override SteeringOutput GetSteering(AutonomousAgent character, WeightedSteeringBehaviour agentLocalBehaviour) {
            SteeringOutput output = new SteeringOutput();

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
}