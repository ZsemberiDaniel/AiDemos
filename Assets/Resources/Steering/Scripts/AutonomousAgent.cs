using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Steering { 
    [RequireComponent(typeof(SpriteRenderer))]
    [Serializable]
    public class AutonomousAgent : MonoBehaviour {

        /// <summary>
        /// What kind of steering behaviours this autonomous agent has
        /// </summary>
        [Tooltip("All the steering behaviours this autonomous agent has.")]
        [SerializeField]
        internal List<WeightedSteeringBehaviour> steeringBehaviours = new List<WeightedSteeringBehaviour>();

        /// <summary>
        /// What type of blending to use with the steering behaviours
        /// </summary>
        [Tooltip("What type of blending to use with the steering behaviours.")]
        [SerializeField]
        internal SteeringBlendingTypes blendingType;

        [SerializeField]
        internal float maxSpeed = 5f;

        /// <summary>
        /// The velocity of this agent
        /// </summary>
        private Vector3 velocity;

        #region Unity methods
        private void Start() {
            // Set te character for the behaviours
            for (int i = 0; i < steeringBehaviours.Count; i++) {
                steeringBehaviours[i].behaviour.Character = this;
            }

            NormalizeWeights();
        }

        private void Update() {
            switch (blendingType) {

                case SteeringBlendingTypes.Single:
                    // Get the first and only steering we need
                    SteeringOutput steering = steeringBehaviours[0].behaviour.GetSteering();

                    // Update pos and orientation
                    transform.position += velocity;

                    // update with steering
                    velocity += steering.linear;

                    // Limit to velocity
                    if (velocity.magnitude > maxSpeed) {
                        velocity.Normalize();
                        velocity *= maxSpeed;
                    }
                    break;
            }
        }
        #endregion

        /// <summary>
        /// Normalizes the weights of the behaviours so they add up to 1
        /// </summary>
        private void NormalizeWeights() {
            float currentSum = 0f;
            for (int i = 0; i < steeringBehaviours.Count; i++)
                currentSum += steeringBehaviours[i].weight;

            // Now we know what is all of the added up together so we can get weight of one with
            // currentWeight / sum

            for (int i = 0; i < steeringBehaviours.Count; i++)
                steeringBehaviours[i].weight /= currentSum;
        }
    }

    [Serializable]
    internal class WeightedSteeringBehaviour {
        [SerializeField]
        public float weight;

        [SerializeField]
        public SteeringBehaviour behaviour;
    }

    /// <summary>
    /// All the blending types that we can use with the steering behaviours.
    /// </summary>
    [Serializable]
    public enum SteeringBlendingTypes {
        Single = 0,
        Weighted = 1
    }

#region Custom Editor
    [CustomEditor(typeof(AutonomousAgent))]
    public class AutonomousAgentDrawer : Editor {

        private float[] lastWeights;
        private bool proportionalEditing = true;

        private SerializedProperty blendingTypeProp;
        private SerializedProperty maxSpeedProp;

        void OnEnable() {
            blendingTypeProp = serializedObject.FindProperty("blendingType");
            maxSpeedProp = serializedObject.FindProperty("maxSpeed");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            AutonomousAgent agent = (AutonomousAgent) target;

            // Attributes
            EditorGUILayout.LabelField("Attributes");
            EditorGUILayout.PropertyField(maxSpeedProp);
            agent.maxSpeed = maxSpeedProp.floatValue;

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Behaviour");
            // Blending type
            EditorGUILayout.PropertyField(blendingTypeProp);
            agent.blendingType = (SteeringBlendingTypes) blendingTypeProp.enumValueIndex;
            
            GUILayoutUtility.GetRect(200f, 10f);

            // We don't need an add button which adds a behaviours because we only have one
            if ((SteeringBlendingTypes) blendingTypeProp.enumValueIndex == SteeringBlendingTypes.Single) {
#region Single custom editor
                // If we don't have only one behaviour then make it so
                if (agent.steeringBehaviours.Count != 1) {
                    // if we have something in the array already pick the first one
                    if (agent.steeringBehaviours.Count > 0) {
                        WeightedSteeringBehaviour behaviour = agent.steeringBehaviours[0];

                        agent.steeringBehaviours.Clear();
                        agent.steeringBehaviours.Add(behaviour);
                    } else { // We have nothing in the array -> add a new one
                        agent.steeringBehaviours.Clear();
                        agent.steeringBehaviours.Add(new WeightedSteeringBehaviour());
                    }
                }
                agent.steeringBehaviours[0].weight = 1f; // Set steering behaviour's weight to 1

                agent.steeringBehaviours[0].behaviour =
                    EditorGUILayout.ObjectField("Behaviour: ", agent.steeringBehaviours[0].behaviour, typeof(SteeringBehaviour), false) as SteeringBehaviour;

                agent.steeringBehaviours[0].behaviour.DrawOnGUI();

#endregion
            } else {
#region Weighted custom editor
                GUILayout.BeginHorizontal();
                { 
                    GUILayout.FlexibleSpace();
                    // add behaviour button
                    if (GUILayout.Button("Add new behaviour")) {
                        agent.steeringBehaviours.Add(new WeightedSteeringBehaviour());

                        // Only the first element can start on 1 because they need to be 1 summed up
                        if (agent.steeringBehaviours.Count == 1)
                            agent.steeringBehaviours[agent.steeringBehaviours.Count - 1].weight = 1f;
                        else
                            agent.steeringBehaviours[agent.steeringBehaviours.Count - 1].weight = 0f;
                    }

                    proportionalEditing = EditorGUILayout.ToggleLeft("Proportional editing", proportionalEditing);
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();

                for (int i = agent.steeringBehaviours.Count - 1; i >= 0; i--) {
                    EditorGUILayout.Separator();


                    // delete button
                    EditorGUILayout.BeginHorizontal();
                    {
                        // weight slider
                        agent.steeringBehaviours[i].weight = EditorGUILayout.Slider("Weight: ", agent.steeringBehaviours[i].weight, 0.01f, 1f);

                        GUI.color = Color.red;
                        if (GUILayout.Button("X", GUILayout.Width(30f))) {
                            float updateAmount = agent.steeringBehaviours[i].weight;
                            agent.steeringBehaviours.RemoveAt(i);
                            
                            // Update because we deleted
                            UpdateWeightsProportionallyExcept(agent, updateAmount, -1);
                            // Update the last weights so it doesn't detect change later down the code
                            UpdateLastWeights(agent);
                            continue;
                        }
                        GUI.color = Color.white;
                    }
                    EditorGUILayout.EndHorizontal();
                    agent.steeringBehaviours[i].behaviour =
                        EditorGUILayout.ObjectField("Behaviour: ", agent.steeringBehaviours[i].behaviour, typeof(SteeringBehaviour), true) as SteeringBehaviour;

                    // Steering behaviour settings
                    if (agent.steeringBehaviours[i].behaviour != null) { 
                        agent.steeringBehaviours[i].behaviour.DrawOnGUI();
                    }

                    GUILayoutUtility.GetRect(200f, 10f);
                }

                // Here we are doing the magic of updating all other weights when we change one
                if (lastWeights != null) {
                    int till = Mathf.Min(lastWeights.Length, agent.steeringBehaviours.Count);

                    // Go through each one and check whether it has changed
                    for (int i = 0; i < till; i++) {
                        if (!Mathf.Approximately(lastWeights[i], agent.steeringBehaviours[i].weight)) {
                            // If it has changed
                            // Calculate how much it did
                            float changeOthers = (lastWeights[i] - agent.steeringBehaviours[i].weight);

                            UpdateWeightsProportionallyExcept(agent, changeOthers, i);
                            break;
                        }
                    }
                }

                // Store what the weights are
                UpdateLastWeights(agent);
#endregion
            }

            switch ((SteeringBlendingTypes) blendingTypeProp.enumValueIndex) {
                case SteeringBlendingTypes.Single:

                    break;
                default:

                    break;
            }
        }

        /// <summary>
        /// Update the weight proportionally to each other
        /// </summary>
        /// <param name="agent">The agent in which we want to update the weights</param>
        /// <param name="amount">How much to update</param>
        /// <param name="except">To which we don't want to add</param>
        private void UpdateWeightsProportionallyExcept(AutonomousAgent agent, float amount, int except) {
            if (!proportionalEditing) return;

            int till = agent.steeringBehaviours.Count;

            // Add all the other weights together (excluding the except one)
            // We are going to use this to calculate how much the other weights should change
            float sumWithoutChanging = 0f;
            for (int k = 0; k < till; k++)
                if (except != k)
                    sumWithoutChanging += agent.steeringBehaviours[k].weight;

            // Update all the other weights
            for (int k = 0; k < till; k++) {
                if (except != k) {
                    agent.steeringBehaviours[k].weight += amount *
                            agent.steeringBehaviours[k].weight / sumWithoutChanging;
                }
            }

            if (agent.steeringBehaviours.Count > 0) { 
                // If they don't sum up to 1 then add the missing amount to 0f
                float sum = 0f;
                for (int i = 0; i < agent.steeringBehaviours.Count; i++) sum += agent.steeringBehaviours[i].weight;

                agent.steeringBehaviours[0].weight += (1f - sum);
            }
        }

        private void UpdateLastWeights(AutonomousAgent agent) {
            lastWeights = new float[agent.steeringBehaviours.Count];
            for (int i = 0; i < agent.steeringBehaviours.Count; i++) lastWeights[i] = agent.steeringBehaviours[i].weight;
        }
    }
#endregion
}
