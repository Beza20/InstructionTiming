using System.Collections.Generic;
using UnityEngine;

public class MovementTracker : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float positionThreshold = 0.05f; // 5cm = minimal hand movement
    [SerializeField] private float rotationThreshold = 20f;   // 20° cumulative = head moved
    [SerializeField] private float timeWindow = 2f;        // Check over last 0.5 seconds

    [Header("References")]
    [SerializeField] private GameObject leftHand;
    [SerializeField] private GameObject rightHand;
    
    public class RotationHistoryHands
    {
        public Quaternion rotation;
        public Vector3 position;
        public float timestamp;
    }
    private Dictionary<GameObject, List<RotationHistoryHands>> rotationLogsHands = new();
    [SerializeField] private TriggerManagerCoordinator coordinator;

    private Transform ActiveHead => coordinator != null ? coordinator.ActiveHead : null;


    // Tracking state
    private Queue<float> headRotationDeltas = new Queue<float>();
    private Queue<float> head2RotationDeltas = new Queue<float>();
    private float rotationSum = 0f;
    private Vector3 prevLeftHandPos, prevRightHandPos;
    private Quaternion prevHeadRotation;
    private Transform lastActiveHead;

    // Public properties (simplified conditions)
    public bool IsHeadMoving { get; private set; }
    public bool AreHandsStill { get; private set; }

    void Start()
    {
        rotationLogsHands[leftHand] = new List<RotationHistoryHands>();
        rotationLogsHands[rightHand] = new List<RotationHistoryHands>();
        lastActiveHead = ActiveHead;
        prevHeadRotation = ActiveHead.transform.rotation;
        

    }

    void Update()
    {
        float now = Time.time;
        rotationLogsHands[leftHand].Add(new RotationHistoryHands
        {
            rotation = leftHand.transform.rotation,
            position = leftHand.transform.position,
            timestamp = now
        });
        rotationLogsHands[rightHand].Add(new RotationHistoryHands
        {
            rotation = rightHand.transform.rotation,
            position = rightHand.transform.position,
            timestamp = now
        });
        float cutoff = now - 2f;
        rotationLogsHands[leftHand].RemoveAll(e => e.timestamp < cutoff);
        rotationLogsHands[rightHand].RemoveAll(e => e.timestamp < cutoff);

        Transform currentHead = ActiveHead;

        if (currentHead != lastActiveHead)
        {
            // Reset tracking when the active head changes
            prevHeadRotation = currentHead.rotation;
            rotationSum = 0f;
            headRotationDeltas.Clear();
            lastActiveHead = currentHead;
        }

        float deltaRotation = Quaternion.Angle(prevHeadRotation, currentHead.rotation);
        headRotationDeltas.Enqueue(deltaRotation);
        rotationSum += deltaRotation;

        if (headRotationDeltas.Count > timeWindow / Time.deltaTime)
        {
            rotationSum -= headRotationDeltas.Dequeue();
        }

        IsHeadMoving = rotationSum >= rotationThreshold;
        prevHeadRotation = currentHead.rotation;

        AreHandsStill = AreHandsStillA1();

    }
    public bool AreHandsStillA1(float windowSeconds = 1f)
    {
        float now = Time.time;
        var logs = GetHandRotationLogs();

        foreach (var kvp in logs)
        {
            List<RotationHistoryHands> history = kvp.Value;
            float totalDistance = 0f;

            // Step through consecutive points in the time window
            for (int i = 1; i < history.Count; i++)
            {
                if (now - history[i].timestamp <= windowSeconds)
                {
                    float segmentDist = Vector3.Distance(history[i - 1].position, history[i].position);
                    totalDistance += segmentDist;
                }
            }

            if (totalDistance > positionThreshold)
            {
                // Exceeded movement threshold
                return false;
            }
        }

        return true;
    }

    public Dictionary<GameObject, List<RotationHistoryHands>> GetHandRotationLogs()
    {
        return rotationLogsHands;
    }
    
}