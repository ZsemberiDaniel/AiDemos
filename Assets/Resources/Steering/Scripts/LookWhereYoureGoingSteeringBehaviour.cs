using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Steering {
    [CreateAssetMenu(menuName = "Steering Behaviours/Look where you're going")]
    [System.Serializable]
    public class LookWhereYoureGoingSteeringBehaviour : AlignSteeringBehaviour {

        public override bool CanChangeVelocity() {
            return false;
        }

        public override bool CanChangeRotation() {
            return true;
        }

        public override SteeringOutput GetSteering(AutonomousAgent character, WeightedSteeringBehaviour agentLocalBehaviour) {
            return LookWhereYoureGoing(character);
        }

        public SteeringOutput LookWhereYoureGoing(AutonomousAgent character) {
            // get direction then with tangent we can calculate the angle
            float toZAngle = 360f - Mathf.Atan2(character.Velocity.x, character.Velocity.y) * Mathf.Rad2Deg;

            return Align(character, new Vector3(0, 0, toZAngle));
        }

        public override void DrawGizmos(Transform characterTransform) {
            base.DrawGizmos(characterTransform);
        }

        public override void DrawOnGUI() { }
    }
}
