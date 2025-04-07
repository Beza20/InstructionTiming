
using System.Collections.Generic;
using UnityEngine;

public class MovementTracker : MonoBehaviour
{
    [SerializeField] private GameObject leftHand;
    [SerializeField] private GameObject rightHand;
    [SerializeField] private GameObject head;

    private Vector3 prevLeftHandPos, prevRightHandPos;
    private Quaternion prevLeftRotation, prevRightRotation, prevHeadRotation;

    private float velocityUpdateInterval = 0.1f;
    private float velocityTimer = 0f;

    private Queue<float> leftVelocities = new Queue<float>();
    private Queue<float> rightVelocities = new Queue<float>();

    private Queue<float> leftRotationDiffs = new Queue<float>();
    private Queue<float> rightRotationDiffs = new Queue<float>();
    private Queue<float> headRotationDiffs = new Queue<float>();

    private int smoothingWindow = 10;

    private float smoothedLeftVelocity = 0f;
    private float smoothedRightVelocity = 0f;
    private float smoothedLeftRotationDiff = 0f;
    private float smoothedRightRotationDiff = 0f;
    private float smoothedHeadRotationDiff = 0f;

    void Start()
    {
        prevLeftHandPos = leftHand.transform.position;
        prevRightHandPos = rightHand.transform.position;

        prevLeftRotation = leftHand.transform.rotation;
        prevRightRotation = rightHand.transform.rotation;
        prevHeadRotation = head.transform.rotation;
    }

    void Update()
    {
        velocityTimer += Time.deltaTime;
        // smoothes velocities every 0.1 seconds over 10 enteries.

        if (velocityTimer >= velocityUpdateInterval)
        {
            // Velocity
            float leftVelocity = (leftHand.transform.position - prevLeftHandPos).magnitude / velocityUpdateInterval;
            float rightVelocity = (rightHand.transform.position - prevRightHandPos).magnitude / velocityUpdateInterval;

            // Rotation Difference
            float leftRotationDiff = Quaternion.Angle(prevLeftRotation, leftHand.transform.rotation);
            float rightRotationDiff = Quaternion.Angle(prevRightRotation, rightHand.transform.rotation);
            float headRotationDiff = Quaternion.Angle(prevHeadRotation, head.transform.rotation);

            // Add to queues
            AddToQueue(leftVelocities, leftVelocity);
            AddToQueue(rightVelocities, rightVelocity);
            AddToQueue(leftRotationDiffs, leftRotationDiff);
            AddToQueue(rightRotationDiffs, rightRotationDiff);
            AddToQueue(headRotationDiffs, headRotationDiff);

            // Smooth
            smoothedLeftVelocity = ComputeMovingAverage(leftVelocities);
            smoothedRightVelocity = ComputeMovingAverage(rightVelocities);
            smoothedLeftRotationDiff = ComputeMovingAverage(leftRotationDiffs);
            smoothedRightRotationDiff = ComputeMovingAverage(rightRotationDiffs);
            smoothedHeadRotationDiff = ComputeMovingAverage(headRotationDiffs);

            // Update previous states
            prevLeftHandPos = leftHand.transform.position;
            prevRightHandPos = rightHand.transform.position;
            prevLeftRotation = leftHand.transform.rotation;
            prevRightRotation = rightHand.transform.rotation;
            prevHeadRotation = head.transform.rotation;

            velocityTimer = 0f;
        }
    }

    private void AddToQueue(Queue<float> queue, float newValue)
    {
        if (queue.Count >= smoothingWindow)
            queue.Dequeue();
        queue.Enqueue(newValue);
    }

    private float ComputeMovingAverage(Queue<float> queue)
    {
        if (queue.Count == 0) return 0f;
        float sum = 0f;
        foreach (float val in queue) sum += val;
        return sum / queue.Count;
    }

    // Public getters
    public float GetLeftHandVelocity() => smoothedLeftVelocity;
    public float GetRightHandVelocity() => smoothedRightVelocity;
    public float GetLeftRotationDifference() => smoothedLeftRotationDiff;
    public float GetRightRotationDifference() => smoothedRightRotationDiff;
    public float GetHeadRotationDifference() => smoothedHeadRotationDiff;
}
