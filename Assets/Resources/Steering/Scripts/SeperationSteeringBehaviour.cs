using System;
using UnityEditor;
using UnityEngine;

namespace Steering {
    [CreateAssetMenu(menuName = "Steering Behaviours/Seperate")]
    [Serializable]
    public class SeperateSteeringBehaviour : SteeringBehaviour {

        /// <summary>
        /// At what radius to the target the agent should stop
        /// </summary>
        [SerializeField]
        protected float targetRadius = 1f;

        protected float decayCoefficient = 0.5f;

        protected LayerMask layerMask;

        public override bool CanChangeVelocity() {
            return true;
        }

        public override bool CanChangeRotation() {
            return false;
        }

        public override SteeringOutput GetSteering(AutonomousAgent character, WeightedSteeringBehaviour agentLocalBehaviour) {
            return Seperate(character, agentLocalBehaviour.target.position);
        }

        Collider2D[] collidedWith = new Collider2D[7];
        public SteeringOutput Seperate(AutonomousAgent character, Vector3 targetPostion) {
            SteeringOutput steering = new SteeringOutput();

            int count = Physics2D.OverlapCircleNonAlloc(character.transform.position, targetRadius, collidedWith, 1 << layerMask);

            for (int i = 0; i < count; i++) {
                // not the same gameobject tht we are currently inspecting
                if (!collidedWith[i].gameObject.Equals(character.gameObject)) {
                    float distance = Vector3.Distance(character.transform.position, collidedWith[i].transform.position);
                    float strength = Mathf.Min(character.maxAcceleration, decayCoefficient * distance * distance);

                    steering.linear += strength * (character.transform.position - collidedWith[i].transform.position).normalized;
                }
            }

            return steering;
        }

        public override void DrawOnGUI() {
            // Target stop radius
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Stop radius");
            targetRadius = EditorGUILayout.FloatField(targetRadius);
            GUILayout.EndHorizontal();

            layerMask = EditorGUILayout.LayerField("Seperation layer", layerMask);
            decayCoefficient = EditorGUILayout.Slider("Decay coefficient", decayCoefficient, 0.1f, 5f);
        }

        public override void DrawGizmos(Transform carachterTransform) {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(carachterTransform.position, targetRadius);
        }
    }

    public class Seperation : Attribute { }
}
