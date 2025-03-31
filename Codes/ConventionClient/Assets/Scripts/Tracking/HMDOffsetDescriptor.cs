using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace silab.conventions.tracking {

	[CreateAssetMenu(fileName="Tracking", menuName="ScriptableObject/CreateHMDOffsetDescriptor")]
	public class HMDOffsetDescriptor : ScriptableObject {
	    
		[SerializeField] Vector3 offset;

		public Vector3 Offset => offset;
	}
}
