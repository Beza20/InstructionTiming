using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace silab.conventions.targets {

    public class TargetCalibration : MonoBehaviour {
        
        [SerializeField] GameObject p1, p2, p3;
        [SerializeField] GameObject target;

        void Update() {
            target.transform.position = p3.transform.position;
            target.transform.rotation = CalculateRotation();
        }

        Vector3 CalculateNormal() {
            Vector3 v1 = p2.transform.position - p1.transform.position;
            Vector3 v2 = p3.transform.position - p1.transform.position;
            return Vector3.Cross(v1, v2).normalized;
        }

        Quaternion CalculateRotation() {
            Vector3 normal = CalculateNormal();
            Vector3 forward = p2.transform.position - p1.transform.position;
            return Quaternion.LookRotation(forward, normal);
        }
    }
}
