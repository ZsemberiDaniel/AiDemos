using UnityEditor;
using UnityEngine;

namespace Steering {

#region Default
    /// <summary>
    /// Basic steering behaviour
    /// </summary>
    [System.Serializable]
    public abstract class SteeringBehaviour : ScriptableObject {
        public abstract bool CanChangeVelocity();
        public abstract bool CanChangeRotation();

        public abstract SteeringOutput GetSteering(AutonomousAgent character, WeightedSteeringBehaviour agentLocalBehaviour);

        public abstract void DrawGizmos(Transform characterTransform);
        public abstract void DrawOnGUI();
    }

    [CustomEditor(typeof(SteeringBehaviour), true)]
    public class SteeringBehaviourDrawer : Editor {

        public override void OnInspectorGUI() {
            (target as SteeringBehaviour).DrawOnGUI();
        }

    }
#endregion

    /// <summary>
    /// The output class the getSteering method produces
    /// </summary>
    public class SteeringOutput {
        public Vector3 linear;
        public float angular;

        public SteeringOutput() {
            linear = new Vector3();
        }

        public void LimitOutputs(float linearLimit, float angularLimit) {
            LimitLinear(linearLimit);
            LimitAngular(angularLimit);
        }

        public void LimitAngular(float angularLimit) {
            if (Mathf.Abs(angular) > angularLimit) {
                // *... so it keeps it's direction
                angular = angularLimit * (angular / Mathf.Abs(angular));
            }
        }

        public void LimitLinear(float linearLimit) {
            if (linear.magnitude > linearLimit) {
                linear.Normalize();
                linear *= linearLimit;
            }
        }
    }
}
