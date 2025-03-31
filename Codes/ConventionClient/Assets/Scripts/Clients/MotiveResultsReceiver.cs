using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using silab.conventions.communication;
using silab.conventions.parameters;
using silab.conventions.tracking;
using silab.conventions.utils;

namespace silab.conventions.clients {

	public class MotiveResultsReceiver : CommunicationBase {

	    [SerializeField] float interval;
	    float t;

	    [SerializeField] GameObject hmd_camera;

	    [SerializeField] GameObject camera_object;
	    [SerializeField] GameObject grid_object;

	    MotiveResults current_motive_results;

	    void Start() {
	    	current_motive_results = new MotiveResults();
	    }

	    void Update() {
	    	t += Time.deltaTime;
	    	if (t > interval) {
	    		GetData();
	    	}
	    }

	    public TransformInfo GetGridTransformInfo() {
	    	// Quaternion rot_diff = hmd_camera.transform.rotation * Quaternion.Inverse(current_motive_results.builder_hmd.rotation);
	    	// return new TransformInfo(
	    	// 	rot_diff * (current_motive_results.grid.position - current_motive_results.builder_hmd.position) + current_motive_results.builder_hmd.position,
	    	// 	rot_diff * current_motive_results.grid.rotation
	    	// );
	    	return new TransformInfo(current_motive_results.grid.position, current_motive_results.grid.rotation);
	    }

	    public TransformInfo GetHMDTransformInfo() {
	    	return new TransformInfo(current_motive_results.builder_hmd.position, current_motive_results.builder_hmd.rotation);
	    }

	    // public Vector3 GetHMDPosition() {
	    // 	return current_motive_results.builder_hmd.position;
	    // }

		public override void GetResultsFromServer(string parameter_json) {
			MotiveResults motive_results = JsonUtility.FromJson<MotiveResults>(parameter_json);
			current_motive_results = motive_results;

			grid_object.transform.position = motive_results.grid.position;
			grid_object.transform.rotation = motive_results.grid.rotation;

			camera_object.transform.position = motive_results.builder_hmd.position;
			camera_object.transform.rotation = motive_results.builder_hmd.rotation;
		}
	}
}
