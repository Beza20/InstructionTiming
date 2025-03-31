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
    
    public float positionDeltaThreshold = 0.01f;  // Acceptable movement in meters


    public List<GameObject> trackedObjects = new List<GameObject>();
    public float recordInterval = 0.1f;

    private Dictionary<GameObject, List<RotationHistory>> rotationLogs = new();
    private float timer = 0f;

    void Start()
    {
        foreach (GameObject obj in trackedObjects)
            rotationLogs[obj] = new List<RotationHistory>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= recordInterval)
        {
            float now = Time.time;
            foreach (GameObject obj in trackedObjects)
            {
                rotationLogs[obj].Add(new RotationHistory
                {
                    rotation = obj.transform.rotation,
                    position = obj.transform.position,
                    timestamp = now
                });
            }
            timer = 0f;
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
}
