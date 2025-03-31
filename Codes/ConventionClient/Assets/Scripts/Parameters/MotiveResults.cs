using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace silab.conventions.parameters {

	[System.Serializable]
	public class MotiveResults {

		public TransformInfo grid;
		public TransformInfo builder_hmd;
		public TransformInfo instructor_hmd;
		public Vector3 builder_offset;
		public Vector3 instructor_offset;

	    public MotiveResults() {
	    	this.grid = new TransformInfo();
	    	this.builder_hmd = new TransformInfo();
	    	this.instructor_hmd = new TransformInfo();
	    	this.builder_offset = new Vector3();
	    	this.instructor_offset = new Vector3();
	    }
	}
}
