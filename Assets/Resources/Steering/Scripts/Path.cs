using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Steering {
    [ExecuteInEditMode]
    public class Path : MonoBehaviour {
        #region Stations list
        [SerializeField]
        private List<Station> stations = new List<Station>();

        public int StationsCount {
            get { return stations.Count; }
        }
        public GameObject GetStation(int index) {
            return stations[index].gameObject;
        }
        public ReadOnlyCollection<Station> GetAllStations() {
            return stations.AsReadOnly();
        }

        /// <summary>
        /// THIS SHOULD NEVER EVER BE USED BY ANYTHING ELSE THAN THE REORDERABLELIST
        /// </summary>
        /// <returns></returns>
        public List<Station> GetAllStationWARNING() {
            return stations;
        }

        public void AddStation(GameObject station) {
            stations.Add(new Station(station));
            ReCalculateStationWeights();
        }
        public void RemoveStationAt(int index) {
            stations.RemoveAt(index);
            ReCalculateStationWeights();
        }

        internal void ReCalculateStationWeights() {
            // calculate distances and the sum
            float sum = 0f;
            for (int i = (isCircle ? 0 : 1); i < stations.Count; i++) {
                stations[i].distanceToHere = Vector3.Distance(stations[i].gameObject.transform.position, stations[GetPreviousStationIndex(i)].gameObject.transform.position);
                sum += stations[i].distanceToHere;
            }

            // calcuate weights
            for (int i = (isCircle ? 0 : 1); i < stations.Count; i++) {
                stations[i].weight = stations[i].distanceToHere / sum;
            }
        }
        #endregion

        [SerializeField]
        internal bool isCircle = false;
        /// <summary>
        /// Returns position of station with the given index
        /// </summary>
        public Vector3 GetStationPosition(int index) {
            if (index >= stations.Count || index < 0)
                throw new ArgumentException(index + " is not a valid station index for this method!");

            return stations[index].gameObject.transform.position;
        }

        /// <summary>
        /// Returns the index of the next station. Takes the circle into consideration
        /// </summary>
        public int GetNextStationIndex(int current) {
            if (current >= stations.Count || current < 0)
                throw new ArgumentException(current + " is not a valid station index for this method!");

            if (current < stations.Count - 1)
                return current + 1;
            else { // it is the last station that has been given
                if (isCircle) return 0;
                else return stations.Count - 1;
            }
        }

        /// <summary>
        /// Returns the index of the previous station. Takes the circle into consideration
        /// </summary>
        public int GetPreviousStationIndex(int current) {
            if (current >= stations.Count || current < 0)
                throw new ArgumentException(current + " is not a valid station index for this method!");

            if (current > 0)
                return current - 1;
            else { // current is 0
                if (isCircle) return stations.Count - 1;
                else return 0;
            }
        }

        #region Unity methods
        /// <summary>
        /// Angle between end of arrow line and the arrow line
        /// </summary>
        float endAngle = Mathf.PI * 0.2f;
        public void OnDrawGizmos() {
            Gizmos.color = Color.red;
            for (int i = 0; i < (isCircle ? stations.Count : stations.Count - 1); i++) {
                int toIndex = i == stations.Count - 1 ? 0 : i + 1;

                Gizmos.DrawWireSphere(stations[i].gameObject.transform.position, 0.2f);
                Gizmos.DrawLine(stations[i].gameObject.transform.position, stations[toIndex].gameObject.transform.position);

                // the vector defined by i + 1 and i positions
                Vector3 vector = stations[toIndex].gameObject.transform.position - stations[i].gameObject.transform.position;

                // the angle of the little end in the arrow from up vector (left)
                float thingyAngleLeft = (-Mathf.Atan2(vector.x, vector.y) + (Mathf.PI - endAngle)) * Mathf.Rad2Deg;
                // the same but right
                float thingyAngleRight = thingyAngleLeft + endAngle * 2 * Mathf.Rad2Deg;

                // the position where the end thingy will be drawn
                Vector3 thingyPos = stations[i].gameObject.transform.position + vector / 2f;
                // end position of left thingy
                Vector3 endPositionLeft = thingyPos + Quaternion.Euler(0, 0, thingyAngleLeft) * Vector3.up;
                // end position of right thingy
                Vector3 endPositionRight = thingyPos + Quaternion.Euler(0, 0, thingyAngleRight) * Vector3.up;

                // drawing arrow end
                Gizmos.DrawLine(thingyPos, endPositionLeft);
                Gizmos.DrawLine(thingyPos, endPositionRight);

                Handles.Label(stations[i].gameObject.transform.position, stations[i].gameObject.name);
            }
            if (stations.Count > 0)
                Gizmos.DrawWireSphere(stations[stations.Count - 1].gameObject.transform.position, 0.2f);
            Gizmos.color = Color.white;
        }

        public void Update() {
            // user added a station in game as child
            if (transform.childCount > stations.Count) {
                for (int i = 0; i < transform.childCount; i++) {
                    // we don't have it in station list
                    var station = from st in stations
                                           where st.gameObject.Equals(transform.GetChild(i).gameObject)
                                           select st;
                    
                    if (station.Count() == 0) {
                        stations.Add(new Station(transform.GetChild(i).gameObject));
                        ReCalculateStationWeights();
                        break;
                    }
                }
            // user removed a station in game
            } else if (transform.childCount < stations.Count) {
                for (int i = stations.Count - 1; i >= 0; i--) {
                    // this is the  station that has been destroyed
                    if (stations[i] == null) {
                        stations.RemoveAt(i);
                        ReCalculateStationWeights();
                        break;
                    }
                }
            }

            ReCalculateStationWeights();
        }
        #endregion

        /// <summary>
        /// Returns the ditance from the vector of (station[index-1] -> stations[index]) from point
        /// </summary>
        public float GetDistanceFromPathTo(Vector3 point, int index) {
            if (index >= stations.Count || index < 0)
                throw new ArgumentException(index + " is not a valid station index for this method!");

            return Vector3.Distance(point, GetClosestPointOnPathTo(index, point));
        }

        /// <summary>
        /// Returns the clostest point on the vector of (station[index-1] -> stations[index]) from point
        /// </summary>
        public Vector3 GetClosestPointOnPathTo(int index, Vector3 point) {
            if (index >= stations.Count || index < 0)
                throw new ArgumentException(index + " is not a valid station index for this method!");

            int lastStationIndex = GetPreviousStationIndex(index);
            // the vector of the path to index station
            Vector3 pathVector = stations[lastStationIndex].gameObject.transform.position - stations[index].gameObject.transform.position;
            float pathLengthSquared = Mathf.Pow(pathVector.magnitude, 2);

            // we project the (index station -> point) vector down to the pathVector
            // and if it is out of range make it one of the endpoints
            float projectionAt = Mathf.Min(1f, Mathf.Max(0f,
                Vector3.Dot(point - stations[index].gameObject.transform.position, pathVector) / pathLengthSquared
            ));
            Vector3 projectedPoint = stations[index].gameObject.transform.position + projectionAt * pathVector;

            return projectedPoint;

        }

        /// <summary>
        /// Returns the closest point on the path from point
        /// </summary>
        public Vector3 GetClosestPointOnPath(Vector3 point) {
            // Go through each and choose the smallest distance

            Vector3 closestPoint = GetClosestPointOnPathTo(stations.Count - 1, point);
            float distance = Vector3.Distance(point, closestPoint);

            for (int i = (isCircle ? 0 : 1); i < stations.Count - 1; i++) {
                Vector3 tempClosestPoint = GetClosestPointOnPathTo(i, point);
                float tempDist = Vector3.Distance(point, tempClosestPoint);

                if (tempDist < distance) {
                    distance = tempDist;
                    closestPoint = tempClosestPoint;
                }
            }

            return closestPoint;
        }

        /// <summary>
        /// Gets the percentage of the closest point on path
        /// </summary>
        public float GetClosestPointOnPathPercent(Vector3 point) {
            // Go through each and choose the smallest distance
            // while that's happening store the closest point' percent
            
            float distance = Mathf.Infinity;
            float percent = 0f;

            float percentSumSoFar = 0f;
            for (int i = (isCircle ? 0 : 1); i < stations.Count; i++) {
                Vector3 tempClosestPoint = GetClosestPointOnPathTo(i, point);
                float tempDist = Vector3.Distance(point, tempClosestPoint);

                if (tempDist < distance) {
                    distance = tempDist;
                    percent = percentSumSoFar + 
                        (tempClosestPoint - stations[GetPreviousStationIndex(i)].gameObject.transform.position).magnitude / stations[i].distanceToHere * stations[i].weight;
                }

                percentSumSoFar += stations[i].weight;
            }

            return percent;
        }

        /// <summary>
        /// Returns point on path from precentage
        /// </summary>
        public Vector3 GetPointOnPathPercent(float percentage) {
            percentage %= 1f;

            // go till if we add the stations's weight we go over percentage with the currentSum
            // then we know that the point is in the given path so calculate it where it is
            float currentSum = 0f;
            for (int i = (isCircle ? 0 : 1); i < stations.Count; i++) {
                if (currentSum + stations[i].weight < percentage) { 
                    currentSum += stations[i].weight;
                } else {
                    percentage -= currentSum;
                    int previousIndex = GetPreviousStationIndex(i);
                    return stations[previousIndex].gameObject.transform.position +
                        (stations[i].gameObject.transform.position - stations[previousIndex].gameObject.transform.position) * percentage / stations[i].weight;
                }
            }

            return new Vector3();
        }
    }

    [Serializable]
    public class Station {
        public GameObject gameObject;
        public float weight;
        /// <summary>
        /// Distance to this station from the previous one
        /// </summary>
        public float distanceToHere;

        public Station(GameObject gameObject) {
            this.gameObject = gameObject;
            weight = 0f;
        }
    }

    [CustomEditor(typeof(Path))]
    public class PathEditor : Editor {

        private const string stationNames = "Station";
        private Color editModeColor = new Color(0.95686f, 0.26275f, 0.21176f);
        private Color normalModeColor = new Color(0.29804f, 0.68627f, 0.31373f);
        private GUIStyle errorLabel;

        private ReorderableList list;
        private bool editMode = false;

        private Path path;
        private SerializedProperty isCircleProperty;

        public void OnEnable() {
            path = (Path) target;
            isCircleProperty = serializedObject.FindProperty("isCircle");

            // ====== GUI STYLES ===========
            errorLabel = new GUIStyle();
            errorLabel.fontSize = 16;
            errorLabel.normal.textColor = Color.red;
            
            // ======= REORDERABLE LIST ==========
            list = new ReorderableList(path.GetAllStationWARNING(), typeof(Station));
            list.drawHeaderCallback = (Rect rect) => {
                EditorGUI.LabelField(rect, "Stations");
            };

            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                GameObject station = ((Station) list.list[index]).gameObject;

                EditorGUI.Vector2Field(rect, station.name + ":", station.transform.position);
            };

            list.onAddCallback = (ReorderableList list) => {
                Vector3 newStationPosition;

                // if no station yet use path parent position otherwise the last station position
                if (path.StationsCount == 0)
                    newStationPosition = path.transform.position;
                else
                    newStationPosition = path.GetStation(path.StationsCount - 1).transform.position;

                // set all attributes
                GameObject newStation = new GameObject(stationNames + (path.StationsCount + 1));
                newStation.transform.position = newStationPosition;
                newStation.transform.parent = path.transform;
                path.AddStation(newStation);
                
                if (editMode) { 
                    Selection.activeGameObject = newStation;
                    list.index = list.count;
                }
            };

            list.onRemoveCallback = (ReorderableList list) => {
                GameObject selected = (GameObject) list.serializedProperty.GetArrayElementAtIndex(list.index).objectReferenceValue;

                // DELETE
                GUI.color = Color.red;
                {
                    DestroyImmediate(selected);
                    ((Path) target).RemoveStationAt(list.index);

                    NameAllCorrectly(path);
                }
                GUI.color = Color.white;
            };

            list.onReorderCallback = (ReorderableList list) => {
                NameAllCorrectly(path);
                path.ReCalculateStationWeights();
            };

            list.onSelectCallback = (ReorderableList list) => {
                if (editMode)
                    Selection.activeGameObject = ((Station) list.list[list.index]).gameObject;
            };
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUILayout.PropertyField(isCircleProperty);
            
            if (editMode) GUI.color = editModeColor;
            else GUI.color = normalModeColor;
            { 
                if (GUILayout.Button((editMode ? "Close " : "Enter ") + "edit mode")) {
                    if (editMode) {
                        ActiveEditorTracker.sharedTracker.isLocked = false;
                        Selection.activeGameObject = path.gameObject;
                        editMode = false;
                    } else {
                        ActiveEditorTracker.sharedTracker.isLocked = true;
                        editMode = true;
                    }
                }
            }
            GUI.color = Color.white;

            if (path.StationsCount < 2) {
                GUILayout.Label("You need at least two stations for a path!", errorLabel);
            }

            list.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Names all stations in this path correctly
        /// </summary>
        private void NameAllCorrectly(Path path) {
            for (int k = 0; k < path.StationsCount; k++) {
                // if it still has the default name
                if (path.GetStation(k).gameObject.name.StartsWith(stationNames))
                    path.GetStation(k).gameObject.name = stationNames + (k + 1);
            }
        }
    }
}