using System;
using UnityEditor;
using UnityEngine;

namespace Steering {
    [CreateAssetMenu(menuName = "Steering Behaviours/Pursue")]
    public class PursueSteeringBehaviour : SeekSteeringBehaviour {

        private Vector3 targetLastPostion;
        private float maxPrediction;

        public override SteeringOutput GetSteering(AutonomousAgent character, WeightedSteeringBehaviour agentLocalBehaviour) {
            SteeringOutput output = new SteeringOutput();

            if (targetLastPostion != null) { 
                // it calculates how long it would take the agent to get to the postion of the target
                // with the current speed. If that is too much it limits that down to maxPrediction.
                // We need that time to calculate where the target will be in the future in that time
                // so we can set the target to that position.

                float distance = (agentLocalBehaviour.target.transform.position - character.transform.position).magnitude;
                float agentSpeed = character.Velocity.magnitude;

                float prediction;
                if (agentSpeed <= distance / maxPrediction) {
                    prediction = maxPrediction;
                } else {
                    prediction = distance / agentSpeed;
                }

                // v = s / t
                Vector3 targetVelocity = (agentLocalBehaviour.target.position - targetLastPostion).normalized / Time.fixedDeltaTime;

                output = base.GetSteering(character, agentLocalBehaviour.target.transform.position + targetVelocity * prediction);
            }
            targetLastPostion = agentLocalBehaviour.target.transform.position;

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

    public class Pursue : Attribute { }
}