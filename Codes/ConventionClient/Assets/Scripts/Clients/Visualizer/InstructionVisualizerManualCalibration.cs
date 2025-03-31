using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using silab.conventions.parameters;
using silab.conventions.utils;

namespace silab.conventions.clients {

    public class InstructionVisualizerManualCalibration : InstructionVisualizer {

        protected override void VisualizeFingerPoints(MetaQuestHand hand, List<GameObject> spheres, List<LineRenderer> lines) {
            InstructorMetadata metadata = tower_visualizer.GetMetadata();
            Vector3 instructor_p3 = metadata.instructor_p3;
            Vector3 builder_p3 = metadata.builder_p3;
            Quaternion instructor_rot = VectorUtils.CalculateRotation(
                metadata.instructor_p1,
                metadata.instructor_p2,
                metadata.instructor_p3
            );
            Quaternion builder_rot = VectorUtils.CalculateRotation(
                metadata.builder_p1,
                metadata.builder_p2,
                metadata.builder_p3
            );
            Quaternion rot_diff = Quaternion.Inverse(instructor_rot) * builder_rot;

            List<List<Vector3>> hand_connection = hand.GetConnections();
            int point_count = hand_connection.Count;
            CreateHandInstances(hand_connection, spheres, lines);

            int bone_index = 0;
            for (int i=0; i<point_count; i++) {
                List<Vector3> connections = hand_connection[i];
                Vector3 pos = rot_diff * (connections[0] - instructor_p3) + builder_p3;
                spheres[i].transform.position = pos;
                int connection_count = connections.Count - 1;

                if (connection_count > 0) {
                    for (int j=1; j<connections.Count; j++) {
                        Vector3 start = rot_diff * (connections[0] - instructor_p3) + builder_p3;
                        Vector3 end = rot_diff * (connections[j] - instructor_p3) + builder_p3;
                        lines[bone_index].SetPosition(0, start);
                        lines[bone_index].SetPosition(1, end);
                        bone_index++;
                    }
                }
            }

            EnableHandVisualization(true, spheres, lines);
        }
    }
}