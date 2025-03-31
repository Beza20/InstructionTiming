using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using silab.conventions.parameters;

namespace silab.conventions.clients {

    public class InstructionVisualizer : MonoBehaviour {

        [SerializeField] GameObject sphere, bone_line;
        List<GameObject> spheres_left, spheres_right;
        List<LineRenderer> lines_left, lines_right;
        [SerializeField] protected TowerVisualizer tower_visualizer;

        void Start() {
            spheres_left = new List<GameObject>();
            spheres_right = new List<GameObject>();
            lines_left = new List<LineRenderer>();
            lines_right = new List<LineRenderer>();
        }

        public void VisualizeFinger(MetaQuestHand hand, string hand_type) {
            if (hand_type == "left") {
                VisualizeFingerPoints(hand, spheres_left, lines_left);
            } else if (hand_type == "right") {
                VisualizeFingerPoints(hand, spheres_right, lines_right);
            } else {
                Debug.LogError("Invalid hand type: " + hand_type);
            }
        }

        protected void CreateHandInstances(List<List<Vector3>> hand_connection, List<GameObject> spheres, List<LineRenderer> lines) {
            int point_count = hand_connection.Count;
            while (spheres.Count < point_count) {
                GameObject new_sphere = Instantiate(sphere);
                spheres.Add(new_sphere);
            }

            int bone_count = 0;
            for (int i=0; i<point_count; i++) {
                List<Vector3> connections = hand_connection[i];
                int connection_count = connections.Count - 1;
                bone_count += connection_count;
            }
            while (lines.Count < bone_count) {
                GameObject new_line = Instantiate(bone_line);
                lines.Add(new_line.GetComponent<LineRenderer>());
            }
        }
        
        protected virtual void VisualizeFingerPoints(MetaQuestHand hand, List<GameObject> spheres, List<LineRenderer> lines) {
            List<List<Vector3>> hand_connection = hand.GetConnections();
            int point_count = hand_connection.Count;
            CreateHandInstances(hand_connection, spheres, lines);

            int bone_index = 0;
            for (int i=0; i<point_count; i++) {
                List<Vector3> connections = hand_connection[i];
                Vector3 pos = connections[0];
                spheres[i].transform.position = pos;
                int connection_count = connections.Count - 1;

                if (connection_count > 0) {
                    for (int j=1; j<connections.Count; j++) {
                        Vector3 start = connections[0];
                        Vector3 end = connections[j];
                        lines[bone_index].SetPosition(0, start);
                        lines[bone_index].SetPosition(1, end);
                        bone_index++;
                    }
                }
            }

            EnableHandVisualization(true, spheres, lines);
        }

        protected void EnableHandVisualization(bool enable, List<GameObject> spheres, List<LineRenderer> lines) {
            spheres.ForEach(sphere => sphere.SetActive(enable));
            lines.ForEach(line => line.enabled = enable);
        }

        public void EnableInstructionVisualization(bool enable) {
            tower_visualizer.VisualizeGrid(enable);
            EnableHandVisualization(enable, spheres_left, lines_left);
            EnableHandVisualization(enable, spheres_right, lines_right);
        }
    }
}
