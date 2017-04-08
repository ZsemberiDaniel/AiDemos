using System;
using UnityEditor;
using UnityEngine;

namespace Steering {
    [CreateAssetMenu(menuName = "Steering Behaviours/Follow path")]
    [Serializable]
    public class FollowPathSteeringBehaviour : SeekSteeringBehaviour {

        [SerializeField]
        private FollowType followType;

        [SerializeField]
        private float followAheadPercent = 0.1f;

        [SerializeField]
        private float predictTime = 0.1f;

        public override bool CanChangeVelocity() {
            return true;
        }
        public override bool CanChangeRotation() {
            return false;
        }

        public override SteeringOutput GetSteering(AutonomousAgent character, WeightedSteeringBehaviour agentLocalBehaviour) {
            return FollowPath(character, agentLocalBehaviour);
        }

        public SteeringOutput FollowPath(AutonomousAgent character, WeightedSteeringBehaviour agentLocalBehaviour) {
            SteeringOutput output = new SteeringOutput();

            // Debug.Log(agentLocalBehaviour.currentPathStation + " " + agentLocalBehaviour.path.GetNextStationIndex(agentLocalBehaviour.currentPathStation));
            switch (followType) {
                case FollowType.Station:
                    #region Station
                    // in agentLocalBehaviour we store the current station that is being followed
                    // so we need to check whether the agent is closer to that station or the next one
                    // to check that we need to get the position 

                    // if the current path is farther away then the next path
                    if ((agentLocalBehaviour.path.GetDistanceFromPathTo(character.transform.position, agentLocalBehaviour.currentPathStation) >
                        agentLocalBehaviour.path.GetDistanceFromPathTo(character.transform.position, agentLocalBehaviour.path.GetNextStationIndex(agentLocalBehaviour.currentPathStation)))
                        // or we have reached the station
                        || Vector3.Distance(character.transform.position, agentLocalBehaviour.path.GetStationPosition(agentLocalBehaviour.currentPathStation)) <= targetRadius) {
                        // make the next station the target
                        agentLocalBehaviour.currentPathStation = agentLocalBehaviour.path.GetNextStationIndex(agentLocalBehaviour.currentPathStation);
                    }

                    output = Seek(character, agentLocalBehaviour.path.GetStationPosition(agentLocalBehaviour.currentPathStation));
                #endregion
                    break;
                case FollowType.AlwaysReachStation:
                    #region AlwaysReachStation
                    // we have reached the station
                    if (Vector3.Distance(character.transform.position, agentLocalBehaviour.path.GetStationPosition(agentLocalBehaviour.currentPathStation)) <= targetRadius) {
                        // make the next station the target
                        agentLocalBehaviour.currentPathStation = agentLocalBehaviour.path.GetNextStationIndex(agentLocalBehaviour.currentPathStation);
                    }

                    output = Seek(character, agentLocalBehaviour.path.GetStationPosition(agentLocalBehaviour.currentPathStation));
                    #endregion
                    break;
                case FollowType.Path:
                    #region Path
                    // here we look ahead on the path with followAheadPercent and set that as a target

                    float percent = agentLocalBehaviour.path.GetClosestPointOnPathPercent(character.transform.position);
                    Vector3 target = agentLocalBehaviour.path.GetPointOnPathPercent(percent + followAheadPercent);
                    output = Seek(character, target);

                    Debug.DrawLine(character.transform.position, target, Color.green);
                    #endregion
                    break;
                case FollowType.PredictivePath:
                    #region Predictive Path
                    percent = agentLocalBehaviour.path.GetClosestPointOnPathPercent(character.transform.position + character.Velocity * predictTime);
                    target = agentLocalBehaviour.path.GetPointOnPathPercent(percent + followAheadPercent);
                    output = Seek(character, target);

                    Debug.DrawLine(character.transform.position, character.transform.position + character.Velocity * predictTime, Color.yellow);
                    Debug.DrawLine(character.transform.position, target, Color.green);
                    #endregion
                    break;
            }

            return output;
        }

        public override void DrawGizmos(Transform characterTransform) {
            base.DrawGizmos(characterTransform);
        }

        public override void DrawOnGUI() {
            followType = (FollowType) EditorGUILayout.EnumPopup("Follow type", followType);
            followAheadPercent = EditorGUILayout.Slider("How much to look ahead on path (%)", followAheadPercent, 0.01f, 0.9f);

            if (followType == FollowType.PredictivePath)
                predictTime = EditorGUILayout.FloatField("How much to predict (s)", predictTime);
            base.DrawOnGUI();
        }
    }

    public class FollowPath : Attribute { }

    internal enum FollowType {
        Station = 0,
        AlwaysReachStation = 1,
        Path = 2, 
        PredictivePath = 3
    }
}