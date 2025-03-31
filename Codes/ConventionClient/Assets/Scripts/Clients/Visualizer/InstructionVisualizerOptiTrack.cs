using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using silab.conventions.parameters;

namespace silab.conventions.clients {

	public class InstructionVisualizerOptiTrack : InstructionVisualizer {
	    
	    protected override void VisualizeFingerPoints(MetaQuestHand hand, List<GameObject> spheres, List<LineRenderer> lines) {
            List<List<Vector3>> hand_connection = hand.GetConnections();
            int point_count = hand_connection.Count;
            CreateHandInstances(hand_connection, spheres, lines);

            // TODO: Add codes for hand motion transformation.

            EnableHandVisualization(true, spheres, lines);
	    }
	}
}
