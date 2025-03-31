using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using silab.conventions.parameters;

namespace silab.conventions.clients {

	public class TowerVisualizerOptiTrack : TowerVisualizer {

		[SerializeField] GameObject hmd;
		[SerializeField] MotiveResultsReceiver motive_result_receiver;

		void Update() {
			TransformInfo grid_transform_info = motive_result_receiver.GetGridTransformInfo();
			this.gameObject.transform.position = grid_transform_info.position;
			this.gameObject.transform.rotation = grid_transform_info.rotation;
			TransformInfo hmd_transform_info = motive_result_receiver.GetHMDTransformInfo();
			hmd.transform.position = hmd_transform_info.position;
			hmd.transform.rotation = hmd_transform_info.rotation;
		}

		public override void VisualizeTower(InstructorMetadata instructor_metadata) {
			base.VisualizeTower(instructor_metadata);
		}
	}
}
