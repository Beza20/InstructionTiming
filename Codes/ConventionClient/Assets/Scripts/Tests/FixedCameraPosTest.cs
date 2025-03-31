using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace silab.conventions.tests {

	public class FixedCameraPosTest : MonoBehaviour {
	    
	    [SerializeField] GameObject camera_center;
	    [SerializeField] GameObject camera_right;
	    [SerializeField] GameObject camera_left;
	    [SerializeField] TextMeshProUGUI center_eye_text;
	    [SerializeField] TextMeshProUGUI right_eye_text;
	    [SerializeField] TextMeshProUGUI left_eye_text;

	    void Update() {
	    	right_eye_text.text = String.Format(
	    		"Right eye pos: x={0:F3}, y={1:F3}, z={2:F3}",
	    		camera_right.transform.position.x,
	    		camera_right.transform.position.y,
	    		camera_right.transform.position.z
	    	);
	    	left_eye_text.text = String.Format(
	    		"Left eye pos: x={0:F3}, y={1:F3}, z={2:F3}",
	    		camera_left.transform.position.x,
	    		camera_left.transform.position.y,
	    		camera_left.transform.position.z
	    	);
	    	Vector3 center_direction = camera_center.transform.forward;
	    	center_eye_text.text = String.Format(
	    		"Center eye dir: x={0:F3}, y={1:F3}, z={2:F3}",
	    		center_direction.x,
	    		center_direction.y,
	    		center_direction.z
	    	);
	    }

	    // void LateUpdate() {
	    // 	camera_center.transform.position = new Vector3(0, 1, 0);
	    // 	camera_center.transform.rotation = new Quaternion(1, 0, 0, 0);
	    // }
	}
}
