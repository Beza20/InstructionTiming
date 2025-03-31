using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace silab.conventions.utils {


    public class VectorUtils : MonoBehaviour {


        public static Vector3 CalculateNormal(Vector3 p1, Vector3 p2, Vector3 p3) {
            Vector3 v1 = p2 - p1;
            Vector3 v2 = p3 - p1;
            return Vector3.Cross(v1, v2).normalized;
        }

        public static Quaternion CalculateRotation(Vector3 p1, Vector3 p2, Vector3 p3) {
            Vector3 normal = CalculateNormal(p1, p2, p3);
            Vector3 v12 = p2 - p1;
            Vector3 x = Vector3.Cross(normal, v12).normalized;
            Vector3 z = Vector3.Cross(x, normal);
            return Quaternion.LookRotation(normal, z);
        }
    }
}
