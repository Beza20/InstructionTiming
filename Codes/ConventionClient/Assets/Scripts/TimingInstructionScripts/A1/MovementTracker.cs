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
    [SerializeField] private GameObject head;
    public class RotationHistoryHands
    {
        public Quaternion rotation;
        public Vector3 position;
        public float timestamp;
    }
    private Dictionary<GameObject, List<RotationHistoryHands>> rotationLogsHands = new();

    

    // Tracking state
    private Queue<float> headRotationDeltas = new Queue<float>();
    private float rotationSum = 0f;
    private Vector3 prevLeftHandPos, prevRightHandPos;
    private Quaternion prevHeadRotation;

    // Public properties (simplified conditions)
    public bool IsHeadMoving { get; private set; }
    public bool AreHandsStill { get; private set; }

    void Start()
    {
        rotationLogsHands[leftHand] = new List<RotationHistoryHands>();
        rotationLogsHands[rightHand] = new List<RotationHistoryHands>();

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
        
        // Track cumulative head rotation over timeWindow
        float deltaRotation = Quaternion.Angle(prevHeadRotation, head.transform.rotation);
        headRotationDeltas.Enqueue(deltaRotation);
        rotationSum += deltaRotation;

        // Remove old entries if timeWindow exceeded
        if (headRotationDeltas.Count > timeWindow / Time.deltaTime)
        {
            rotationSum -= headRotationDeltas.Dequeue();
        }

        // Update conditions
        IsHeadMoving = rotationSum >= rotationThreshold;
        AreHandsStill = AreHandsStillA1();
         
        

        // Store current state for next frame
        prevHeadRotation = head.transform.rotation;
        
    }
    public bool AreHandsStillA1(float windowSeconds = 1f)
    {
        float now = Time.time;
        var logs = GetHandRotationLogs();

        foreach (var kvp in logs)
        {
            
            GameObject obj = kvp.Key;

            List<RotationHistoryHands> history = kvp.Value;
            RotationHistoryHands past = null;
            bool foundPast = false;

            // Find the first log entry older than the time window
            for (int i = history.Count - 1; i >= 0; i--)
            {
                if (now - history[i].timestamp >= windowSeconds)
                {
                    past = history[i];
                    foundPast = true;
                    break;
                }
            }

            // Check position change if history exists
            if (foundPast)
            {
                float distance = Vector3.Distance(kvp.Key.transform.position, past.position);
                if (distance > positionThreshold)
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