using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchingCameras : MonoBehaviour {

    [SerializeField] GameObject CenterEyeAnchor;
    [SerializeField] GameObject Headset;
    [SerializeField] GameObject Camerarig;
    Vector3 initial_camerarig_pos;
    [SerializeField] bool is_calibration;

    // Start is called before the first frame update
    void Start() {
        is_calibration = false;
        initial_camerarig_pos = Camerarig.transform.position;
    }

    // Update is called once per frame
    void Update() {
       if (is_calibration) {
        UpdateCameraRigTransform();
       }
    }

    void UpdateCameraRigTransform() {
        Quaternion headTocam_rot_diff = Headset.transform.rotation * Quaternion.Inverse(CenterEyeAnchor.transform.rotation);
        Camerarig.transform.rotation = headTocam_rot_diff * Camerarig.transform.rotation;

        Vector3 headTocam_pos_diff = CenterEyeAnchor.transform.position - Headset.transform.position;
        Camerarig.transform.position = Camerarig.transform.position - headTocam_pos_diff;
    }
}
