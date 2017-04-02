using System;
using UnityEditor;
using UnityEngine;

namespace Steering {
    [CreateAssetMenu(menuName = "Steering Behaviours/Wander")]
    [System.Serializable]
    public class WanderSteeringBehaviour : FaceSteeringBehaviour {

        /// <summary>
        /// The rate the wandering can change it's orientaton
        /// </summary>
        protected int wanderRate = 20;

        /// <summary>
        /// How much the circle is offset
        /// </summary>
        protected float wanderOffset = 1f;

        /// <summary>
        /// How big the wander circle is
        /// </summary>
        protected float wanderRadius = 0.5f;

        public override bool CanChangeVelocity() {
            return true;
        }
        public override bool CanChangeRotation() {
            return true;
        }

        public override SteeringOutput GetSteering(AutonomousAgent character, WeightedSteeringBehaviour agentLocalBehaviour) {
            return Wander(character, agentLocalBehaviour);
        }

        public SteeringOutput Wander(AutonomousAgent character, WeightedSteeringBehaviour agentLocalBehaviour) {
            SteeringOutput output = new SteeringOutput();

            // update wander angle with [-wanderRate;wanderRate]
            agentLocalBehaviour.wanderAngle += (UnityEngine.Random.value * 2f - 1f) * wanderRate;

            Vector3 targetCircleCenter = character.transform.position + character.transform.up * wanderOffset;
            // move from center of circle by a vector with wanderAngle angle and wanderRadius length
            Vector3 targetOnCircleLocation = targetCircleCenter + 
                Quaternion.Euler(0, 0, agentLocalBehaviour.wanderAngle) * character.transform.up * wanderRadius;

            if (character.showGizmos)
                Debug.DrawLine(character.transform.position, targetOnCircleLocation, Color.red);

            // get rotation
            output = Face(character, targetOnCircleLocation);
            // set linear to full acceleration ahead
            output.linear = character.transform.up * character.maxAcceleration;

            return output;
        }

        public override void DrawGizmos(Transform characterTransform) {
            // base.DrawGizmos(characterTransform);
            
            Vector3 targetCircleCenter = characterTransform.position + characterTransform.up * wanderOffset;
            Gizmos.DrawLine(characterTransform.position, targetCircleCenter);
            Gizmos.DrawWireSphere(targetCircleCenter, wanderRadius);
        }

        public override void DrawOnGUI() {
            wanderRate = EditorGUILayout.IntSlider("Wander rate", wanderRate, 1, 45);
            wanderOffset = EditorGUILayout.FloatField("Wander offset", wanderOffset);
            wanderRadius = EditorGUILayout.FloatField("Wander radius", wanderRadius);

            // base.DrawOnGUI();
        }
    }

    public class Wander : Attribute { }
}