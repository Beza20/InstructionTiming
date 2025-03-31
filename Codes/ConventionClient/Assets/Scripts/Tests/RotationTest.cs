using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using silab.conventions.utils;

namespace silab.conventions.tests {

    public class RotationTest : MonoBehaviour {

        [SerializeField] GameObject p1, p2, p3;
        [SerializeField] GameObject q1, q2, q3;

        [SerializeField] GameObject mp1, mp2, mp3;
        [SerializeField] GameObject mq1, mq2, mq3;

        [SerializeField] GameObject op, oq;

        void Update() {
            Vector3 normal_p = VectorUtils.CalculateNormal(
                p1.transform.position,
                p2.transform.position,
                p3.transform.position
            );
            Vector3 base_p12 = (p2.transform.position - p1.transform.position).normalized;
            Vector3 base_p13 = (p3.transform.position - p1.transform.position).normalized;
            Vector3 x_p = Vector3.Cross(normal_p, base_p12).normalized;
            Vector3 y_p = Vector3.Cross(x_p, normal_p);
            Quaternion rot_p = Quaternion.LookRotation(normal_p, y_p);

            Vector3 normal_q = VectorUtils.CalculateNormal(
                q1.transform.position,
                q2.transform.position,
                q3.transform.position
            );
            Vector3 base_q12 = (q2.transform.position - q1.transform.position).normalized;
            Vector3 base_q13 = (q3.transform.position - q1.transform.position).normalized;
            Vector3 x_q = Vector3.Cross(normal_q, base_q12).normalized;
            Vector3 y_q = Vector3.Cross(x_q, normal_q);
            Quaternion rot_q = Quaternion.LookRotation(normal_q, y_q);

            // Quaternion rot_diff = Quaternion.FromToRotation(base_p12, base_q12) * Quaternion.FromToRotation(normal_p, normal_q);
            // Quaternion rot_diff = Quaternion.FromToRotation(base_p12, base_q12) * Quaternion.FromToRotation(base_p13, base_q13);

            // Quaternion rot_p = GetRotation(
            //     p1.transform.position,
            //     p2.transform.position,
            //     p3.transform.position
            // );
            // Quaternion rot_q = GetRotation(
            //     q1.transform.position,
            //     q2.transform.position,
            //     q3.transform.position
            // );

            Quaternion rot_diff = rot_q * Quaternion.Inverse(rot_p);
            mq1.transform.position = rot_diff * (mp1.transform.position - p1.transform.position) + q1.transform.position;
            mq2.transform.position = rot_diff * (mp2.transform.position - p1.transform.position) + q1.transform.position;
            mq3.transform.position = rot_diff * (mp3.transform.position - p1.transform.position) + q1.transform.position;
        }

        Quaternion GetRotation(Vector3 p1, Vector3 p2, Vector3 p3) {
            Vector3 base_normal = new Vector3(1, 0, 0);
            Vector3 base_edge = new Vector3(0, 0, 1);
            Vector3 normal = VectorUtils.CalculateNormal(p1, p2, p3);
            Vector3 edge = p2 - p1;
            return Quaternion.FromToRotation(base_normal, normal) * Quaternion.FromToRotation(base_edge, edge);
        }
    }
}
