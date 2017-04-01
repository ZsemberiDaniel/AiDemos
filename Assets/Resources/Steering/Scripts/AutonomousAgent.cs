﻿using System;
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

        [SerializeField]
        internal float maxAcceleration = 2f;

        /// <summary>
        /// In degree
        /// </summary>
        [SerializeField]
        internal float maxAngularAcceleration = 2f;

        /// <summary>
        /// In degree
        /// </summary>
        [SerializeField]
        internal float maxRotation = 5f;

        /// <summary>
        /// The velocity of this agent
        /// </summary>
        private Vector3 velocity;
        public Vector3 Velocity { get { return velocity; } }

        /// <summary>
        /// How much this agent should rotate each frame
        /// </summary>
        private float rotationVelocity;
        /// <summary>
        /// How much this agent rotates each frame
        /// </summary>
        public float RotationVelocity { get { return rotationVelocity; } }

#region Unity methods
        private void Start() {
            NormalizeWeights();
        }

        private void FixedUpdate() {
            switch (blendingType) {

                case SteeringBlendingTypes.Single:
                    // Get the first and only steering we need
                    SteeringOutput steering = steeringBehaviours[0].behaviour.GetSteering(this, steeringBehaviours[0]);

                    steering.LimitOutputs(maxAcceleration, maxAngularAcceleration);

                    // Update pos and orientation
                    transform.position += velocity * Time.fixedDeltaTime;
                    transform.Rotate(0, 0, rotationVelocity * Time.fixedDeltaTime);

                    // update with steering
                    velocity += steering.linear * Time.fixedDeltaTime;
                    rotationVelocity += steering.angular * Time.fixedDeltaTime;

                    float speed = velocity.magnitude;
                    // Limit to velocity
                    if (speed > maxSpeed) {
                        velocity.Normalize();
                        velocity *= maxSpeed;
                    }

                    if (Mathf.Approximately((float) Math.Round(speed, 1), 0f)) {
                        velocity = new Vector3();
                    }

                    // Limit angular velocity
                    if (rotationVelocity > maxRotation) {
                        // * .... so it keeps the direction
                        rotationVelocity = maxRotation * (maxRotation / Mathf.Abs(maxRotation));
                    }
                    break;
            }
        }

        private void OnDrawGizmos() {
            for (int i = 0; i < steeringBehaviours.Count; i++)
                if (steeringBehaviours[i].behaviour != null)
                    steeringBehaviours[i].behaviour.DrawGizmos(transform);
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

    /// <summary>
    /// Contains data about the steering behaviour
    /// </summary>
    [Serializable]
    public class WeightedSteeringBehaviour : MonoBehaviour {
        [SerializeField]
        public float weight;

        [SerializeField]
        public SteeringBehaviour behaviour;

        [Align]
        [Seek]
        [SerializeField]
        [VelocityMatch]
        [Pursue]
        public Transform target;
    }

    /// <summary>
    /// So the weightedsteeringbehaviour component does not show anything on the gameobject
    /// </summary>
    [CustomEditor(typeof(WeightedSteeringBehaviour))]
    public class WeightedSteeringBehaviourDrawer : Editor {

    }

    /// <summary>
    /// Class for storing the attributes to the corresponding behaviours
    /// </summary>
    public static class AttributeOfBehaviour {
        // To what type of behaviour what type of attribute belongs
        public static Type[,] correspondingTypes = new Type[,] {
            { typeof(SeekSteeringBehaviour), typeof(Seek) },
            { typeof(FleeSteeringBehaviour), typeof(Flee) },
            { typeof(AlignSteeringBehaviour), typeof(Align) },
            { typeof(VelocityMatchSteeringBehaviour), typeof(VelocityMatch) },
            { typeof(PursueSteeringBehaviour), typeof(Pursue) }
        };
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

        void OnEnable() {
            blendingTypeProp = serializedObject.FindProperty("blendingType");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            AutonomousAgent agent = (AutonomousAgent) target;


            // Attributes
            EditorGUILayout.LabelField("Agent attributes");

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Max speed: ");
            agent.maxSpeed = EditorGUILayout.FloatField(agent.maxSpeed);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Max acceleration: ");
            agent.maxAcceleration = EditorGUILayout.FloatField(agent.maxAcceleration);
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Max rotation: ");
            agent.maxRotation = EditorGUILayout.FloatField(agent.maxRotation);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Max angular acceleration: ");
            agent.maxAngularAcceleration = EditorGUILayout.FloatField(agent.maxAngularAcceleration);
            GUILayout.EndHorizontal();

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

                        for (int i = 1; i < agent.steeringBehaviours.Count; i++)
                            DestroyImmediate(agent.steeringBehaviours[i]);

                        agent.steeringBehaviours.Clear();
                        agent.steeringBehaviours.Add(behaviour);
                    } else { // We have nothing in the array -> add a new one
                        agent.steeringBehaviours.Clear();
                        agent.steeringBehaviours.Add(agent.gameObject.AddComponent<WeightedSteeringBehaviour>());
                    }
                }
                agent.steeringBehaviours[0].weight = 1f; // Set steering behaviour's weight to 1

                DrawBehaviour(agent, 0);
#endregion
            } else {
#region Weighted custom editor

                proportionalEditing = EditorGUILayout.ToggleLeft("Proportional editing", proportionalEditing);

                // add behaviour button
                if (GUILayout.Button("Add new behaviour")) {
                    agent.steeringBehaviours.Add(agent.gameObject.AddComponent<WeightedSteeringBehaviour>());

                    // Only the first element can start on 1 because they need to be 1 summed up
                    if (agent.steeringBehaviours.Count == 1)
                        agent.steeringBehaviours[agent.steeringBehaviours.Count - 1].weight = 1f;
                    else
                        agent.steeringBehaviours[agent.steeringBehaviours.Count - 1].weight = 0f;
                }
                
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
                            DestroyImmediate(agent.steeringBehaviours[i]);
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
                    DrawBehaviour(agent, i);

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
        }

        

        private void DrawBehaviour(AutonomousAgent agent, int index) {
            agent.steeringBehaviours[index].behaviour
                = EditorGUILayout.ObjectField("Behaviour: ", agent.steeringBehaviours[index].behaviour, typeof(SteeringBehaviour), false) as SteeringBehaviour;

            // Steering behaviour settings
            if (agent.steeringBehaviours[index].behaviour != null) {
                // go through the corresponding types (to which class which attribute belongs)
                for (int j = 0; j < AttributeOfBehaviour.correspondingTypes.GetLength(0); j++) {
                    // agent's behaviour has the same type as we are examining
                    if (agent.steeringBehaviours[index].behaviour.GetType().Equals(AttributeOfBehaviour.correspondingTypes[j, 0])) {
                        // get all fields of the behaviour because we are going to check their attributes
                        var fields = agent.steeringBehaviours[index].GetType().GetFields();

                        for (int i = 0; i < fields.Length; i++) {
                            // all attributes of the current field
                            var attributes = fields[i].GetCustomAttributes(false);

                            for (int k = 0; k < attributes.Length; k++) {
                                // if the current field has the corresponding attribute (to the current class type)
                                if (attributes[k].GetType().Equals(AttributeOfBehaviour.correspondingTypes[j, 1])) {
                                    // serialize the behaviour to get the current field as property and
                                    // be able to display it to the editor
                                    SerializedObject serializedBehav = new SerializedObject(agent.steeringBehaviours[index]);
                                    SerializedProperty property = serializedBehav.FindProperty(fields[i].Name);

                                    // display the property
                                    serializedBehav.Update();
                                    EditorGUILayout.PropertyField(property);

                                    // set the property's value because for sme reason it doesn't update
                                    // even if I put the update after the field............................sjnosdjfdjfdjkdfsojfdojfdosko
                                    fields[i].SetValue(agent.steeringBehaviours[index], property.objectReferenceValue);
                                }
                            }
                        }
                    }
                }

                agent.steeringBehaviours[index].behaviour.DrawOnGUI();
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
