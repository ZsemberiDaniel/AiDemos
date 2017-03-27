using UnityEditor;
using UnityEngine;

namespace Steering {

#region Default
    /// <summary>
    /// Basic steering behaviour
    /// </summary>
    public abstract class SteeringBehaviour : ScriptableObject {
        /// <summary>
        /// Will be set in the start method of autonomousagent
        /// </summary>
        protected AutonomousAgent character;
        public AutonomousAgent Character {
            set {
                character = value;
            }
        }

        public abstract SteeringOutput GetSteering();

        public virtual void DrawOnGUI() { }
    }

    [CustomEditor(typeof(SteeringBehaviour), true)]
    public class SteeringBehaviourDrawer : Editor {

        public override void OnInspectorGUI() {
            (target as SteeringBehaviour).DrawOnGUI();
        }

    }
#endregion

#region Seek / Flee
    /// <summary>
    /// Specific steering behaviour for seek and flee
    /// </summary>
    public abstract class SeekFleeBehaviour : SteeringBehaviour {
        private Transform target;

        public override void DrawOnGUI() {
            base.DrawOnGUI();

            target = EditorGUILayout.ObjectField("Target: ", target, typeof(Transform), true) as Transform;
        }
    }
#endregion

    /// <summary>
    /// The output class the getSteering method produces
    /// </summary>
    public class SteeringOutput {
        public Vector3 linear;

        public SteeringOutput() {
            linear = new Vector3();
        }
    }
}
