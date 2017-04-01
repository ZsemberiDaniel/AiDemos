using System;
using UnityEditor;
using UnityEngine;

namespace Steering { 
    [CreateAssetMenu(menuName = "Steering Behaviours/Align")]
    public class AlignSteeringBehaviour : SteeringBehaviour {

        /// <summary>
        /// How much time it should take to reach the target in theory
        /// </summary>
        private float timeToTarget = 0.1f;
        
        private float targetRadius = 5f;
        private float slowdownRadius = 20f;

        public override SteeringOutput GetSteering(AutonomousAgent character, WeightedSteeringBehaviour agentLocalBehaviour) {
            SteeringOutput output = new SteeringOutput();

            // rotation between the target and the character
            // only accounts for the z rotation which counts in 2d
            float rotateOptionLeft = agentLocalBehaviour.target.transform.eulerAngles.z -
                character.transform.eulerAngles.z;
            // we count the right rotation like this because we get the ang between up and target
            // by subtracting it from 360f and we have to add target rotation to get the full right rotation
            float rotateOptionRight = character.transform.eulerAngles.z +
                (360f - agentLocalBehaviour.target.transform.eulerAngles.z);
            
            float rotation;
            // Decide which direction is better
            if (Mathf.Abs(rotateOptionLeft) > Mathf.Abs(rotateOptionRight))
                rotation = -rotateOptionRight;
            else
                rotation = rotateOptionLeft;

            float rotationSize = Mathf.Abs(rotation);
            float targetRotation; // how much we can rotate

            // we reached our target and we are within the given radius
            if (rotationSize < targetRadius) {
                output.angular = -character.RotationVelocity;
                return output;
            // we are withing the slowdown radius -> start the slowdown
            } else if (rotationSize < slowdownRadius) {
                // we add target radius so it multiplies by 0 not when it is exactly at desired rotation
                // but when it is inside the target radius
                targetRotation = character.maxRotation * (rotationSize / (slowdownRadius + targetRadius));
            // normal aligning without anything
            } else {
                targetRotation = character.maxRotation;
            }

            // apply direction
            targetRotation *= rotation / rotationSize;

            // add acceleration
            output.angular = targetRotation / timeToTarget;

            return output;
        }

        public override void DrawGizmos(Transform characterTransform) {
            float lineLength = 2f;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(characterTransform.position, 
                                              // Rotate the charactertransform up angle by targetradius
                characterTransform.position + Quaternion.Euler(0, 0, targetRadius) * (characterTransform.up * lineLength));
            Gizmos.DrawLine(characterTransform.position,
                characterTransform.position + Quaternion.Euler(0, 0, -targetRadius) * (characterTransform.up * lineLength));

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(characterTransform.position,
                characterTransform.position + Quaternion.Euler(0, 0, slowdownRadius) * (characterTransform.up * lineLength));
            Gizmos.DrawLine(characterTransform.position,
                characterTransform.position + Quaternion.Euler(0, 0, -slowdownRadius) * (characterTransform.up * lineLength));
        }

        public override void DrawOnGUI() {
            // Time to target
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Time to reach target");
            timeToTarget = EditorGUILayout.FloatField(timeToTarget);
            GUILayout.EndHorizontal();

            // Target radius
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Stop radius");
            targetRadius = EditorGUILayout.FloatField(targetRadius);
            GUILayout.EndHorizontal();

            // Slow down radius
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Slowing radius");
            slowdownRadius = EditorGUILayout.FloatField(slowdownRadius);
            GUILayout.EndHorizontal();
        }
    }

    public class Align : Attribute { }
}