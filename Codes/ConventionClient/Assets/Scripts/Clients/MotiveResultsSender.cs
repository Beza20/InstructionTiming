using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using silab.conventions.communication;
using silab.conventions.parameters;
using silab.conventions.tracking;

namespace silab.conventions.clients {

	public class MotiveResultsSender : CommunicationBase {

		[SerializeField] MotiveTracking motive_tracking;
		[SerializeField] float interval;
		float t;

		void Update() {
			t += Time.deltaTime;
			if (t > interval) {
				t = 0;
				MotiveResults motive_results = motive_tracking.GetBuilderMotiveResults();
				Send(motive_results);
			}
		}
	}
}
