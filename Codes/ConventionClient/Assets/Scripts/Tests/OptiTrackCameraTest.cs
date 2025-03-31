using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace silab.conventions.tests {

	public class OptiTrackCameraTest : MonoBehaviour {

		void Start() {
			Transform ovr_camera = this.transform.GetChild(0).Find("TrackingSpace");
			Destroy(ovr_camera.gameObject);
		}
	}
}
