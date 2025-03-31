using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using silab.conventions.communication;
using silab.conventions.parameters;

namespace silab.conventions.clients {

    public class TowerVisualizer : CommunicationBase {

        [SerializeField] float interval;
        float t;
        protected InstructorMetadata metadata;

        void Start() {
            metadata = new InstructorMetadata();
        }
        
        void Update() {
            t += Time.deltaTime;
            if (t > interval) {
                GetData();
                t = 0;
            }
        }

        public virtual InstructorMetadata GetMetadata() {
            return metadata;
        }

        public virtual void VisualizeTower(InstructorMetadata instructor_metadata) {
            foreach (Transform child in transform) {
                if (child.name == "Frames") {
                    continue;
                }
                child.gameObject.SetActive(child.name == instructor_metadata.tower_id);
            }
        }

        public void VisualizeGrid(bool is_active) {
            this.gameObject.transform.Find("Frames").gameObject.SetActive(is_active);
        }

        public override void GetResultsFromServer(string parameter_json) {
            Debug.Log(parameter_json);
            InstructorMetadata instructor_metadata = JsonUtility.FromJson<InstructorMetadata>(parameter_json);
            metadata = instructor_metadata;
            Debug.Log("Tower ID: " + instructor_metadata.tower_id);
            VisualizeTower(instructor_metadata);
        }
    }
}
