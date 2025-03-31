using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using silab.conventions.parameters;
using silab.conventions.utils;

namespace silab.conventions.tracking {

	public class MotiveTracking : MonoBehaviour {
	    
		[SerializeField] OptitrackStreamingClient streamingClient;

		[SerializeField] int grid_id;
		[SerializeField] GameObject grid;
		TransformInfo grid_transform;
		[SerializeField] GameObject grid_center_cube;

		[SerializeField] int builder_hmd_id;
		TransformInfo builder_hmd_transform;

		[SerializeField] GameObject hmd;
		[SerializeField] HMDOffsetDescriptor hmd_offset_descriptor;

		void Start() {
			grid_transform = new TransformInfo();
			builder_hmd_transform = new TransformInfo();
		}

		void Update() {
			OptitrackRigidBodyState builder_hmd_rigidbody_state = streamingClient.GetLatestRigidBodyState(
				builder_hmd_id,
				true
			);
			if (builder_hmd_rigidbody_state != null) {
				builder_hmd_transform.position = builder_hmd_rigidbody_state.Pose.Position;
				builder_hmd_transform.rotation = builder_hmd_rigidbody_state.Pose.Orientation;
			}

			OptitrackRigidBodyState grid_rigidbody_state = streamingClient.GetLatestRigidBodyState(
				grid_id,
				true
			);
			if (grid_rigidbody_state != null) {
				grid_transform.position = grid_rigidbody_state.Pose.Position;
				grid_transform.rotation = grid_rigidbody_state.Pose.Orientation;
			}

			grid.transform.position = grid_transform.position;
			grid.transform.rotation = grid_transform.rotation;

			grid_center_cube.transform.position = grid_transform.position;

			hmd.transform.position = builder_hmd_transform.position;
			hmd.transform.rotation = builder_hmd_transform.rotation;
		}

		public MotiveResults GetBuilderMotiveResults() {
			MotiveResults results = new MotiveResults();
			results.grid = grid_transform;
			results.builder_hmd = builder_hmd_transform;
			results.builder_offset = hmd_offset_descriptor.Offset;
			return results;
		}
	}
}
