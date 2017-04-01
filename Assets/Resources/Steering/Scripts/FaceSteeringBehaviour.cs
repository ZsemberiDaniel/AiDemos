using System;
using UnityEditor;
using UnityEngine;

namespace Steering {
    [CreateAssetMenu(menuName = "Steering Behaviours/Face")]
    [System.Serializable]
    public class FaceSteeringBehaviour : AlignSteeringBehaviour {

        public override bool CanChangeVelocity() {
            return false;
        }

        public override bool CanChangeRotation() {
            return true;
        }

        public override SteeringOutput GetSteering(AutonomousAgent character, WeightedSteeringBehaviour agentLocalBehaviour) {
            return Face(character, agentLocalBehaviour.target.transform.position);
        }

        public SteeringOutput Face(AutonomousAgent character, Vector3 position) {
            // get direction then with tangent we can calculate the angle
            Vector3 direction = position - character.transform.position;
            float toZAngle = 360f - Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;

            return Align(character, new Vector3(0, 0, toZAngle));
        }

        public override void DrawGizmos(Transform characterTransform) {
            base.DrawGizmos(characterTransform);
        }

        public override void DrawOnGUI() {
            base.DrawOnGUI();
        }
    }

    public class Face : Attribute { }
}