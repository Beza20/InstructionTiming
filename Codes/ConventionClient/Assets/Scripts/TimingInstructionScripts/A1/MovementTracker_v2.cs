using System.Collections.Generic;
using UnityEngine;

public class MovementTracker_v2 : MonoBehaviour
{
    [Header("Tracking Objects")]
    [SerializeField] private GameObject leftHand;
    [SerializeField] private GameObject rightHand;
    [SerializeField] private GameObject head;

    [Header("Smoothing Settings")]
    public float smoothingWindow = 0.2f; // Seconds (matches Python's 0.2s window)

    // Data buffers with timestamps
    private List<TimestampedData> headRotations = new List<TimestampedData>();
    private List<TimestampedData> leftHandPositions = new List<TimestampedData>();
    private List<TimestampedData> rightHandPositions = new List<TimestampedData>();

    private struct TimestampedData
    {
        public float time;
        public Quaternion rotation;
        public Vector3 position;
    }

    void Update()
    {
        float currentTime = Time.time;
        
        // Store current data with timestamp
        headRotations.Add(new TimestampedData {
            time = currentTime,
            rotation = head.transform.rotation
        });

        leftHandPositions.Add(new TimestampedData {
            time = currentTime,
            position = leftHand.transform.position
        });

        rightHandPositions.Add(new TimestampedData {
            time = currentTime,
            position = rightHand.transform.position
        });

        // Remove data older than our smoothing window
        PruneOldData(currentTime);
    }

    private void PruneOldData(float currentTime)
    {
        RemoveOldSamples(headRotations, currentTime);
        RemoveOldSamples(leftHandPositions, currentTime);
        RemoveOldSamples(rightHandPositions, currentTime);
    }

    private void RemoveOldSamples(List<TimestampedData> samples, float currentTime)
    {
        // Remove samples older than our smoothing window
        for (int i = samples.Count - 1; i >= 0; i--)
        {
            if (currentTime - samples[i].time > smoothingWindow)
            {
                samples.RemoveAt(i);
            }
        }
    }

    // slerp is used for ARIA as well
    public Quaternion GetSmoothedHeadRotation()
    {
        if (headRotations.Count == 0) return Quaternion.identity;
    
        Quaternion avg = headRotations[0].rotation;
        for (int i = 1; i < headRotations.Count; i++)
        {
            avg = Quaternion.Slerp(avg, headRotations[i].rotation, 1f / (i + 1));
        }
        return avg;
    }

    public float GetSmoothedLeftHandVelocity()
    {
        return CalculateSmoothedVelocity(leftHandPositions);
    }

    public float GetSmoothedRightHandVelocity()
    {
        return CalculateSmoothedVelocity(rightHandPositions);
    }

    private float CalculateSmoothedVelocity(List<TimestampedData> positions)
    {
        if (positions.Count < 2) return 0f;
        
        // Calculate total distance traveled in the smoothing window
        float totalDistance = 0f;
        for (int i = 1; i < positions.Count; i++)
        {
            totalDistance += Vector3.Distance(
                positions[i-1].position,
                positions[i].position
            );
        }
        
        // Calculate velocity over the actual time window
        float windowDuration = positions[positions.Count-1].time - positions[0].time;
        return windowDuration > 0 ? totalDistance / windowDuration : 0f;
    }

    // Returns total angular change (degrees) over the last 1 second
    public float GetSmoothedHeadAngularChangePerSecond()
    {
        if (headRotations.Count < 2) return 0f;

        // Get timestamps of newest and oldest data
        float newestTime = headRotations[headRotations.Count - 1].time;
        float oldestTime = newestTime - 0.1f; // 0.1-second window

        // Find the oldest rotation within the 0.1-second window
        Quaternion oldestSmoothedRot = GetSmoothedHeadRotation(); // Default to full buffer if no match
        for (int i = 0; i < headRotations.Count; i++)
        {
            if (headRotations[i].time >= oldestTime)
            {
                // Temporarily prune the buffer to calculate the smoothed rotation at this point
                var tempBuffer = headRotations.GetRange(i, headRotations.Count - i);
                oldestSmoothedRot = GetSmoothedRotationFromBuffer(tempBuffer);
                break;
            }
        }

        // Compare with the newest smoothed rotation
        Quaternion newestSmoothedRot = GetSmoothedHeadRotation();
        return Quaternion.Angle(oldestSmoothedRot, newestSmoothedRot);
    }

    // Helper to apply your smoothing to a subset of the buffer
    private Quaternion GetSmoothedRotationFromBuffer(List<TimestampedData> buffer)
    {
        if (buffer.Count == 0) return Quaternion.identity;
    
        Quaternion avg = buffer[0].rotation;
        for (int i = 1; i < buffer.Count; i++)
        {
            avg = Quaternion.Slerp(avg, buffer[i].rotation, 1f / (i + 1));
        }
        return avg;
    }

    public Quaternion GetCurrentHeadRotation() => head.transform.rotation;
}