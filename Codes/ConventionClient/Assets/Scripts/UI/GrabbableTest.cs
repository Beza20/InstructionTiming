using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace silab.conventions.ui {

    public class GrabbableTest : MetaQuestGrabbableBase {

        [SerializeField] GameObject cube;
        
        new void Start() {
            base.Start();
        }

        public override void SelectGrabbableFunc() {
            cube.transform.position = this.transform.position + new Vector3(0, 0.4f, 0);
        }

        public override void UnselectGrabbableFunc() {
            // cube.transform.position = this.transform.position + new Vector3(0, 0.4f, 0);
        }
    }
}
