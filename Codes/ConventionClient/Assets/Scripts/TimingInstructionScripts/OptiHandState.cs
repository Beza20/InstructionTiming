using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class OptileftHandState {
    public Vector3 leftHandPosition;
    public Quaternion leftHandRotation;
    public float wristVelocity;
    public float leftRotationDifference;
    //public List<GameObject> items = new List<GameObject>();
   // public bool is_grabbed;
    //public Vector3 PosThreshold;
}

[System.Serializable]
public class OptirightHandState {
    public Vector3 rightHandPosition;
    public Quaternion rightHandRotation;
    public float wristVelocity;
    public float rightRotationDifference;
    //public List<GameObject> items = new List<GameObject>();
   // public bool is_grabbed;
    //public Vector3 PosThreshold;
}

public class OptiHandState : MonoBehaviour
{
    public OptileftHandState leftHandState = new OptileftHandState();
    public OptirightHandState rightHandState = new OptirightHandState();
    [SerializeField] GameObject lefthand;
    [SerializeField] GameObject righthand;
    private Vector3 prev_LeftwristPos;
    private Vector3 prev_RightwristPos;
    private Quaternion prev_LeftRotation;
    private Quaternion prev_RightRotation;
    private Vector3 prev_headPos;
    private float velocityUpdateInterval = 0.2f; // Set the interval in seconds
    private float velocityTimer = 0f;
    string leftTruncatedVelocity  = "";
    string rightTruncatedVelocity = "";
    string leftTruncatedRotationDifference = "";
    string rightTruncatedRotationDifference = "";

    Vector3 leftBoneOrigin;
    Vector3 rightBoneOrigin;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 leftWristBonePosition = lefthand.transform.position;
        Vector3 rightWristBonePosition = righthand.transform.position;
        Quaternion leftRotation = lefthand.transform.rotation;
        Quaternion rightRotation = righthand.transform.rotation;
        velocityTimer += Time.deltaTime;

        if (velocityTimer >= velocityUpdateInterval)
        {
            leftHandState.wristVelocity = (leftWristBonePosition - prev_LeftwristPos).magnitude / velocityUpdateInterval;
            rightHandState.wristVelocity = (rightWristBonePosition - prev_RightwristPos).magnitude / velocityUpdateInterval;
           
            leftTruncatedVelocity = leftHandState.wristVelocity.ToString("F2");
            rightTruncatedVelocity = rightHandState.wristVelocity.ToString("F2");

            // Calculate rotational differences
            leftHandState.leftRotationDifference = Quaternion.Angle(prev_LeftRotation, leftRotation);
            rightHandState.rightRotationDifference = Quaternion.Angle(prev_RightRotation, rightRotation);

            leftTruncatedRotationDifference = leftHandState.leftRotationDifference.ToString("F2");
            rightTruncatedRotationDifference = rightHandState.rightRotationDifference.ToString("F2");


            // Update previous positions
            prev_LeftwristPos = leftWristBonePosition;
            prev_RightwristPos = rightWristBonePosition;
            prev_LeftRotation = leftRotation;
            prev_RightRotation = rightRotation;

               

            // Reset the timer
            velocityTimer = 0f;

        }
    }
    public float GetUserMovement()
    {
        // Assign weight factors for velocity and rotation
        float velocityWeight = 0.7f; // Weight for velocity
        float rotationWeight = 0.3f; // Weight for rotational difference

        // Calculate average velocity
        float avgVelocity = (leftHandState.wristVelocity + rightHandState.wristVelocity) / 2f;

        // Calculate average rotational difference
        float avgRotationDifference = (leftHandState.leftRotationDifference + rightHandState.rightRotationDifference) / 2f;

        // Normalize the rotational difference 
        float normalizedRotationDifference = avgRotationDifference / 180f; // 180 degrees as max difference

        // Combine the metrics using the weights
        float userMovement = (velocityWeight * avgVelocity) + (rotationWeight * normalizedRotationDifference);

        return userMovement;
    }

}
