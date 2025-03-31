using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using silab.conventions.parameters;
using UnityEngine.UI;
using TMPro;



[System.Serializable]
public class leftHandState {
    public List<Vector3> leftHandPositions = new List<Vector3>();
    public List<Quaternion> leftHandRotations = new List<Quaternion>();
    public float wristVelocity;
    public List<GameObject> items = new List<GameObject>();
    public bool is_grabbed;
    public Vector3 PosThreshold;
}

[System.Serializable]
public class rightHandState {
    public List<Vector3> rightHandPositions = new List<Vector3>();
    public List<Quaternion> rightHandRotations = new List<Quaternion>();
    public float wristVelocity;
    public List<GameObject> items = new List<GameObject>();
    public bool is_grabbed;
    public Vector3 PosThreshold;
}

[System.Serializable]
public class HeadState {
    public Vector3 HeadPosition;
    public Quaternion HeadRotation;
    public float HeadVelocity;
}


   
public class UserState : MonoBehaviour {

    [SerializeField] OVRSkeleton skeleton_left;
    [SerializeField] OVRSkeleton skeleton_right;
    [SerializeField] GameObject camerarig;
 
    [SerializeField] List<GameObject> FurniturePieces;
    [SerializeField] float grabDistanceThreshold = 0.05f;

    [SerializeField] TextMeshProUGUI HandStateText;
    [SerializeField] TextMeshProUGUI GrabbedItemsText;
    
    // Start is called before the first frame update

    [SerializeField] public leftHandState leftHandState = new leftHandState();
    [SerializeField] public rightHandState rightHandState = new rightHandState();
    [SerializeField] public HeadState headState = new HeadState();

    private Vector3 prev_LeftwristPos;
    private Vector3 prev_RightwristPos;
    private Vector3 prev_headPos;
    private float velocityUpdateInterval = 0.2f; // Set the interval in seconds
    private float velocityTimer = 0f;
    string leftTruncatedVelocity  = "";
    string rightTruncatedVelocity = "";

    Vector3 leftBoneOrigin;
    Vector3 rightBoneOrigin;
    public bool loadedHands = false;
   

   
    void Start()
    {
        leftHandState.is_grabbed = false;
        rightHandState.is_grabbed = false; 
        
        
    
        for (int i = 0; i < 24; i++)
        {
            leftHandState.leftHandPositions.Add(Vector3.zero);
            leftHandState.leftHandRotations.Add(Quaternion.identity);
        }
    
        for (int i = 0; i < 24; i++)
        {
            rightHandState.rightHandPositions.Add(Vector3.zero);
            rightHandState.rightHandRotations.Add(Quaternion.identity);
        }

        leftBoneOrigin = skeleton_left.transform.position;
        rightBoneOrigin = skeleton_right.transform.position;
        string leftOrigin = leftBoneOrigin.magnitude.ToString();
        Debug.Log("here");
        Debug.Log(leftOrigin);
        string rightOrigin = rightBoneOrigin.magnitude.ToString();
        Debug.Log(rightOrigin);

    }

    void Update()
    {
       
        string userMessage = "";
        string grabbedItems = "";
        

    
        headState.HeadPosition = camerarig.transform.position;
        headState.HeadRotation = camerarig.transform.rotation;
    
        IList<OVRBone> bones_left = skeleton_left.Bones;
        int bone_count_left = bones_left.Count;
        IList<OVRBone> bones_right = skeleton_right.Bones;
        int bone_count_right = bones_right.Count;
        if (bone_count_left == 0 || bone_count_right == 0)
        {
            HandStateText.text = "didn't detect any bones";
            leftHandState.wristVelocity = 0;
            rightHandState.wristVelocity = 0;
            headState.HeadVelocity = 0;
            return; 
        } 
        else {
            loadedHands = true;
            for (int i = 0; i < bone_count_left; i++) 
            {
                OVRBone bone = bones_left[i];
                Transform bone_transform = bone.Transform;
                leftHandState.leftHandPositions[i] = bone_transform.position;
                leftHandState.leftHandRotations[i] = bone_transform.rotation;
               
            }
        
            
            for (int i = 0; i < bone_count_right; i++) 
            {
                OVRBone bone = bones_right[i];
                Transform bone_transform = bone.Transform;
                rightHandState.rightHandPositions[i] = bone_transform.position;
                rightHandState.rightHandRotations[i] = bone_transform.rotation;
              
            }
        
            Vector3 leftWristBonePosition = leftHandState.leftHandPositions[0];
            Vector3 rightWristBonePosition = rightHandState.rightHandPositions[0];
            userMessage += "hands are detected and loaded \n";

            velocityTimer += Time.deltaTime;

            if (velocityTimer >= velocityUpdateInterval)
            {
                // Calculate velocities
                if (leftWristBonePosition != leftBoneOrigin && rightWristBonePosition != rightBoneOrigin)
                {
                    leftHandState.wristVelocity = (leftWristBonePosition - prev_LeftwristPos).magnitude / velocityUpdateInterval;
                    rightHandState.wristVelocity = (rightWristBonePosition - prev_RightwristPos).magnitude / velocityUpdateInterval;
                    headState.HeadVelocity = (headState.HeadPosition - prev_headPos).magnitude / velocityUpdateInterval;

                    leftTruncatedVelocity = leftHandState.wristVelocity.ToString("F2");
                    rightTruncatedVelocity = rightHandState.wristVelocity.ToString("F2");

                    // Update previous positions
                    prev_LeftwristPos = leftWristBonePosition;
                    prev_RightwristPos = rightWristBonePosition;
                    prev_headPos = headState.HeadPosition;

                    // Reset the timer
                    velocityTimer = 0f;

                }
                else 
                {
                    Debug.Log("detection of hands lost and not filtering");

                }
               
            }


           

            userMessage += $"left wrist velocity is {leftTruncatedVelocity} and right wrist velocity is {rightTruncatedVelocity} \n";
            HandStateText.text = userMessage;

            foreach (GameObject obj in FurniturePieces)
            {
                MeshCollider meshCollider = obj.GetComponent<MeshCollider>();
                if (meshCollider == null) continue; // Skip if no mesh collider

                Vector3 left_closestPoint = meshCollider.ClosestPoint(leftWristBonePosition);
                Vector3 right_closestPoint = meshCollider.ClosestPoint(rightWristBonePosition);
                float left_distance = Vector3.Distance(leftWristBonePosition, left_closestPoint);
                float right_distance = Vector3.Distance(rightWristBonePosition, right_closestPoint);

                if (left_distance <= 0.12)
                {
                    grabbedItems += $"Left hand is grabbing {obj.name}\n";
                    

                }
                if (right_distance <= 0.12  )
                {
                    grabbedItems += $"Right Hand is grabbing {obj.name}\n";
                }
                string left_truncatedDistance = left_distance.ToString("F2");
                string right_truncatedDistance = right_distance.ToString("F2");

                //userMessage += $"Distance between left hand and {obj.name} is {left_truncatedDistance} \n";
               // userMessage += $"Distance between right hand and {obj.name} is {right_truncatedDistance} \n";

                


            }
            HandStateText.text = userMessage;
            GrabbedItemsText.text = grabbedItems;
        }
       
        

    }
    public float GetUserMovement()
    {
        if(loadedHands)
            return (leftHandState.wristVelocity + rightHandState.wristVelocity)/2f;

        else 
            return -1f;
    }
}
