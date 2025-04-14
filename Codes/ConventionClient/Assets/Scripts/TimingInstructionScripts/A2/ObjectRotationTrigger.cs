using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class ObjectRotationTrigger : MonoBehaviour
{
    [SerializeField] private ObjectRotationTracker tracker;
    [SerializeField] private AudioSource beepAudio;

    public float checkInterval = 0.1f;
    public float returnThreshold = 20f; // Degrees
    public float minReturnTime = 0.3f;
    public float maxReturnTime = 10f;
    public float triggerCooldown = 1.5f;

    private float checkTimer = 0f;
    public float deviationThreshold = 10f;

    private Dictionary<GameObject, List<float>> returnTimestamps = new Dictionary<GameObject, List<float>>();
    private Dictionary<GameObject, float> lastTriggerTime = new Dictionary<GameObject, float>();

    void Update()
    {
        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            float now = Time.time;
            var logs = tracker.GetRotationLogs();

            foreach (var kvp in logs)
            {
                GameObject obj = kvp.Key;
                List<ObjectRotationTracker.RotationHistory> history = kvp.Value;
                //Debug.Log("angular velocity "+ kvp.Value + "at " + checkTimer);

                // Cooldown check
                if (lastTriggerTime.ContainsKey(obj) && now - lastTriggerTime[obj] < triggerCooldown)
                    continue;

                foreach (var past in history)
                {
                    float age = now - past.timestamp;
                    if (age >= minReturnTime && age <= maxReturnTime)
                    {
                        float currentAngleToPast = Quaternion.Angle(obj.transform.rotation, past.rotation);

                        // Look for deviation between 'past' and now
                        bool hasDeviated = false;
                        foreach (var middle in history)
                        {
                            if (middle.timestamp <= past.timestamp || middle.timestamp >= now) continue;
                            float deviation = Quaternion.Angle(middle.rotation, past.rotation);
                            if (deviation > deviationThreshold)
                            {
                                hasDeviated = true;
                                break;
                            }
                        }

                        // Final trigger condition: Returned + Deviation happened
                        if (currentAngleToPast < returnThreshold && hasDeviated)
                        {
                            // Log timestamp
                            if (!returnTimestamps.ContainsKey(obj))
                                returnTimestamps[obj] = new List<float>();
                            returnTimestamps[obj].Add(now);

                            // Play beep
                            if (beepAudio != null) beepAudio.Play();

                            // Set cooldown
                            lastTriggerTime[obj] = now;

                            break; // Triggered once per object per check
                        }
                    }
                }
            }

            checkTimer = 0f;
        }
    }

    public void SaveSummaryToCSV(string filePath = "RotationReturnSummary.csv")
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Object,Timestamp (s),Triggered");

        foreach (var kvp in returnTimestamps)
        {
            string objName = kvp.Key.name;
            foreach (float timestamp in kvp.Value)
            {
                sb.AppendLine($"{objName},{timestamp:F2},TRUE");
            }
        }

        File.WriteAllText(Path.Combine(Application.dataPath, filePath), sb.ToString());
        Debug.Log($"Rotation return summary saved to: {Path.Combine(Application.dataPath, filePath)}");
    }

    void OnApplicationQuit()
    {
        SaveSummaryToCSV();
    }
}
