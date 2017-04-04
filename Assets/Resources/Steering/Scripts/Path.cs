using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Steering {
    [ExecuteInEditMode]
    public class Path : MonoBehaviour {
        public List<GameObject> stations = new List<GameObject>();

        /// <summary>
        /// Angle between end of arrow line and the arrow line
        /// </summary>
        float endAngle = Mathf.PI * 0.2f;
        public void OnDrawGizmos() {
            Gizmos.color = Color.red;
            for (int i = 0; i < stations.Count - 1; i++) {
                Gizmos.DrawWireSphere(stations[i].transform.position, 0.2f);
                Gizmos.DrawLine(stations[i].transform.position, stations[i + 1].transform.position);

                // the vector defined by i + 1 and i positions
                Vector3 vector = stations[i + 1].transform.position - stations[i].transform.position;

                // the angle of the little end in the arrow from up vector (left)
                float thingyAngleLeft = (-Mathf.Atan2(vector.x, vector.y) + (Mathf.PI - endAngle)) * Mathf.Rad2Deg;
                // the same but right
                float thingyAngleRight = thingyAngleLeft + endAngle * 2 * Mathf.Rad2Deg;

                // the position where the end thingy will be drawn
                Vector3 thingyPos = stations[i].transform.position + vector / 2f;
                // end position of left thingy
                Vector3 endPositionLeft = thingyPos + Quaternion.Euler(0, 0, thingyAngleLeft) * Vector3.up;
                // end position of right thingy
                Vector3 endPositionRight = thingyPos + Quaternion.Euler(0, 0, thingyAngleRight) * Vector3.up;

                // drawing arrow end
                Gizmos.DrawLine(thingyPos, endPositionLeft);
                Gizmos.DrawLine(thingyPos, endPositionRight);
            }
            if (stations.Count > 0)
                Gizmos.DrawWireSphere(stations[stations.Count - 1].transform.position, 0.2f);
            Gizmos.color = Color.white;
        }

        public void Update() {
            // user added a station in game as child
            if (transform.childCount > stations.Count) {
                for (int i = 0; i < transform.childCount; i++) {
                    // we don't have it in station list
                    if (!stations.Contains(transform.GetChild(i).gameObject)) {
                        stations.Add(transform.GetChild(i).gameObject);
                        break;
                    }
                }
            // user removed a station in game
            } else if (transform.childCount < stations.Count) {
                for (int i = stations.Count - 1; i >= 0; i--) {
                    // this is the  station that has been destroyed
                    if (stations[i] == null) {
                        stations.RemoveAt(i);
                        break;
                    }
                }
            }
        }
    }

    [CustomEditor(typeof(Path))]
    public class PathEditor : Editor {

        private const string stationNames = "Station";
        private Color editModeColor = new Color(0.95686f, 0.26275f, 0.21176f);
        private Color normalModeColor = new Color(0.29804f, 0.68627f, 0.31373f);


        private ReorderableList list;
        private bool editMode = false;

        private Path path;

        public void OnEnable() {
            path = (Path) target;
            
            list = new ReorderableList(serializedObject, serializedObject.FindProperty("stations"));
            list.drawHeaderCallback = (Rect rect) => {
                EditorGUI.LabelField(rect, "Stations");
            };

            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                GameObject station = (GameObject) list.serializedProperty.GetArrayElementAtIndex(index).objectReferenceValue;

                EditorGUI.Vector2Field(rect, station.name + ":", station.transform.position);
            };

            list.onAddCallback = (ReorderableList list) => {
                Vector3 newStationPosition;

                // if no station yet use path parent position otherwise the last station position
                if (path.stations.Count == 0)
                    newStationPosition = path.transform.position;
                else
                    newStationPosition = path.stations[path.stations.Count - 1].transform.position;

                // set all attributes
                GameObject newStation = new GameObject(stationNames + (path.stations.Count + 1));
                newStation.transform.position = newStationPosition;
                newStation.transform.parent = path.transform;
                path.stations.Add(newStation);
                
                if (editMode)
                    Selection.activeGameObject = newStation;
            };

            list.onRemoveCallback = (ReorderableList list) => {
                GameObject selected = (GameObject) list.serializedProperty.GetArrayElementAtIndex(list.index).objectReferenceValue;

                // DELETE
                GUI.color = Color.red;
                {
                    DestroyImmediate(selected);
                    ((Path) target).stations.RemoveAt(list.index);

                    NameAllCorrectly(path);
                }
                GUI.color = Color.white;
            };

            list.onReorderCallback = (ReorderableList list) => {
                NameAllCorrectly(path);
            };

            list.onSelectCallback = (ReorderableList list) => {
                if (editMode)
                    Selection.activeGameObject = (GameObject) list.serializedProperty.GetArrayElementAtIndex(list.index).objectReferenceValue;
            };
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

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

            list.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Names all stations in this path correctly
        /// </summary>
        private void NameAllCorrectly(Path path) {
            for (int k = 0; k < path.stations.Count; k++) {
                // if it still has the default name
                if (path.stations[k].gameObject.name.StartsWith(stationNames))
                    path.stations[k].gameObject.name = stationNames + (k + 1);
            }
        }
    }
}