using System.Collections.Generic;
using UnityEngine;

public class ObjectRotationTracker : MonoBehaviour
{
    [System.Serializable]
    public class RotationHistory
    {
        public Quaternion rotation;
        public Vector3 position;
        public float timestamp;
    }
    [SerializeField] private Transform _glassesTransform;

    
    public float positionDeltaThreshold = 0.01f;  // Acceptable movement in meters
    public float moving = 1f;


    public List<GameObject> trackedObjects = new List<GameObject>();
    public float recordInterval = 0.1f;

    private Dictionary<GameObject, List<RotationHistory>> rotationLogs = new();
    private float timer = 0f;
    private Dictionary<GameObject, Queue<float>> rotationDeltas = new();
    private Dictionary<GameObject, float> rotationSums = new();

    

    void Start()
    {
        foreach (GameObject obj in trackedObjects)
            rotationLogs[obj] = new List<RotationHistory>();
    }

    void Update()
    {
       

        float now = Time.time;
        foreach (GameObject obj in trackedObjects)
        {
            //Debug.Log($"[Tracker] Logged rotation for {obj.name} at {now}");
            rotationLogs[obj].Add(new RotationHistory
            {
                rotation = obj.transform.rotation,
                position = obj.transform.position,
                timestamp = now
            });
        }    
    }

    public Dictionary<GameObject, List<RotationHistory>> GetRotationLogs()
    {
        return rotationLogs;
    }

    public bool AreObjectsStill(float windowSeconds)
    {
        float now = Time.time;
        var logs = GetRotationLogs();

        foreach (var kvp in logs)
        {
            List<ObjectRotationTracker.RotationHistory> history = kvp.Value;

            ObjectRotationTracker.RotationHistory past = null;
            bool foundPast = false;

            for (int i = history.Count - 1; i >= 0; i--)
            {
                if (now - history[i].timestamp >= windowSeconds)
                {
                    past = history[i];
                    foundPast = true;
                    break;
                }
            }

            if (foundPast)
            {
                Vector3 currentPos = kvp.Key.transform.position;
                float distance = Vector3.Distance(currentPos, past.position);
                if (distance > positionDeltaThreshold)
                    return false;
            }

        }

        return true;
    }

    public bool AreObjectsStillA1(float windowSeconds)
    {
        float now = Time.time;
        var logs = GetRotationLogs();

        foreach (var kvp in logs)
        {
            // Skip objects with the "Glasses" tag
            GameObject obj = kvp.Key;
            if (obj == _glassesTransform.gameObject) continue;

            List<ObjectRotationTracker.RotationHistory> history = kvp.Value;
            ObjectRotationTracker.RotationHistory past = null;
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
                if (distance > positionDeltaThreshold)
                    return false;
            }
        }

        return true;
    }


    public bool IsObjectMoving(GameObject obj, float windowSeconds = 1.5f)
    {
        float now = Time.time;
        if (!rotationLogs.ContainsKey(obj)) return false;

        var history = rotationLogs[obj];
        ObjectRotationTracker.RotationHistory past = null;

        for (int i = history.Count - 1; i >= 0; i--)
        {
            if (now - history[i].timestamp >= windowSeconds)
            {
                past = history[i];
                break;
            }
        }

        if (past != null)
        {
            float distance = Vector3.Distance(obj.transform.position, past.position);
            return distance > moving; // Use your existing threshold
        }

        return false;
    }
    public bool IsObjectMovingA2(GameObject obj, float windowSeconds = 1f, float thresholdDegrees = 5f)
    {
        float now = Time.time;
        if (!rotationLogs.ContainsKey(obj)) return false;

        // Initialize if needed
        if (!rotationDeltas.ContainsKey(obj))
        {
            rotationDeltas[obj] = new Queue<float>();
            rotationSums[obj] = 0f;
        }

        var history = rotationLogs[obj];
        if (history.Count < 2) return false;

        // Remove old history
        history.RemoveAll(h => now - h.timestamp > windowSeconds);

        // Get last recorded sample
        var latest = history[^1];
        var previous = history[0];

        // Compute total cumulative rotation
        float totalRotation = 0f;
        for (int i = 1; i < history.Count; i++)
        {
            totalRotation += Quaternion.Angle(history[i - 1].rotation, history[i].rotation);
        }

        // Update cache (optional)
        rotationSums[obj] = totalRotation;

        return totalRotation > thresholdDegrees;
    }


    public RotationHistory GetClosestSnapshot(GameObject obj, float targetTimestamp)
    {
        if (!rotationLogs.ContainsKey(obj) || rotationLogs[obj].Count == 0)
            return null;

        List<RotationHistory> history = rotationLogs[obj];

        RotationHistory closest = history[0];
        float closestDiff = Mathf.Abs(targetTimestamp - closest.timestamp);

        for (int i = 1; i < history.Count; i++)
        {
            float diff = Mathf.Abs(targetTimestamp - history[i].timestamp);
            if (diff < closestDiff)
            {
                closest = history[i];
                closestDiff = diff;
            }
        }

        return closest;
    }
}
