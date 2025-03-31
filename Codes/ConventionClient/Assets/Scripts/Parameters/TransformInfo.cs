using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace silab.conventions.parameters {

	[System.Serializable]
	public class TransformInfo {
	    
	    public Vector3 position;
	    public Quaternion rotation;

	    public TransformInfo() {
	    	this.position = Vector3.zero;
	    	this.rotation = Quaternion.identity;
	    }

	    public TransformInfo(Vector3 pos, Quaternion rot) {
	    	this.position = pos;
	    	this.rotation = rot;
	    }
	}
}
