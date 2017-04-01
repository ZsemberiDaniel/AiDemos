﻿using UnityEditor;
using UnityEngine;

namespace Steering {

#region Default
    /// <summary>
    /// Basic steering behaviour
    /// </summary>
    public abstract class SteeringBehaviour : ScriptableObject {
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
            if (linear.magnitude > linearLimit) {
                linear.Normalize();
                linear *= linearLimit;
            }
            if (Mathf.Abs(angular) > angularLimit) {
                // *... so it keeps it's direction
                angular = angularLimit * (angular / Mathf.Abs(angular));
            }
        }
    }
}
