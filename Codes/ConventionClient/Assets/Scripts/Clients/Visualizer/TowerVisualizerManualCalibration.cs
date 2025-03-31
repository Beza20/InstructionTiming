using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using silab.conventions.parameters;
using silab.conventions.utils;

namespace silab.conventions.clients {

    public class TowerVisualizerManualCalibration : TowerVisualizer {
        
        [SerializeField] string client_type;

        public override InstructorMetadata GetMetadata() {
            if (metadata.builder_p1 == Vector3.zero && metadata.builder_p2 == Vector3.zero && metadata.builder_p3 == Vector3.zero) {
                metadata.builder_p1 = new Vector3(0.105f, 0.7f, 0.2515f);
                metadata.builder_p2 = new Vector3(-0.105f, 0.7f, 0.2515f);
                metadata.builder_p3 = new Vector3(0, 0.7f, 0.4f);
            }
            return metadata;
        }

        public override void VisualizeTower(InstructorMetadata instructor_metadata) {
            if (client_type == "instructor") {
                this.gameObject.transform.position = metadata.instructor_p3;
                this.gameObject.transform.rotation = VectorUtils.CalculateRotation(
                    metadata.instructor_p1,
                    metadata.instructor_p2,
                    metadata.instructor_p3
                );
            } else if (client_type == "builder") {
                this.gameObject.transform.position = metadata.builder_p3;
                this.gameObject.transform.rotation = VectorUtils.CalculateRotation(
                    metadata.builder_p1,
                    metadata.builder_p2,
                    metadata.builder_p3
                );
            }
            base.VisualizeTower(instructor_metadata);
        }
    }
}
