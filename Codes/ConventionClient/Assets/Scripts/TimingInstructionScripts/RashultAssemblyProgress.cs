using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class RashultAssemblyProgress : MonoBehaviour
{
    [System.Serializable]
    public class SubtaskData
    {
        public string SubtaskName;
        public float RelativeDistance;
        public Quaternion AngleDifference;
    }

    [System.Serializable]
    public class IdealStateData
    {
        public List<SubtaskData> Subtasks = new List<SubtaskData>();
    }

    [System.Serializable]
    public class ConnectionRule
    {
        public int subtaskID1;
        public int subtaskID2;
        public bool mustBeSameType; // A-A or B-B connection
        public bool mustBeDifferentType; // A-B connection
        public List<int> requiredCompletedSubtasks = new List<int>();
        [TextArea] public string description;
    }

    [System.Serializable]
    public class SubtaskPieces
    {
        public GameObject pieceA;
        public GameObject pieceB;
    }

    public TextAsset idealStateFile;
    public List<SubtaskPieces> subtaskPieces;
    public List<ConnectionRule> connectionRules;
    public Slider progressSlider;
    public TextMeshProUGUI progressText;
    public ObjectRotationTracker movementTracker;

    [Header("Progress Parameters")]
    public float positionWeight = 0.5f;
    public float rotationWeight = 0.5f;
    public float positionThreshold = 0.1f;
    public float rotationThreshold = 5f; // Degrees

    private IdealStateData idealState;
    private Dictionary<int, bool> completedSubtasks = new Dictionary<int, bool>();
    private Dictionary<int, float> subtaskProgress = new Dictionary<int, float>();
    private Dictionary<string, bool> validConnections = new Dictionary<string, bool>();

    void Start()
    {
        LoadIdealState();
        InitializeSubtasks();
        SetupDefaultRules();
    }

    void LoadIdealState()
    {
        idealState = JsonUtility.FromJson<IdealStateData>(idealStateFile.text);
        
        // Verify data matches
        if (idealState.Subtasks.Count != subtaskPieces.Count)
        {
            Debug.LogError($"Mismatch between ideal state ({idealState.Subtasks.Count} subtasks) " +
                         $"and piece references ({subtaskPieces.Count})");
        }
    }

    void InitializeSubtasks()
    {
        for (int i = 0; i < idealState.Subtasks.Count; i++)
        {
            completedSubtasks[i] = false;
            subtaskProgress[i] = 0f;
        }
    }

    void SetupDefaultRules()
    {
        connectionRules.Clear();

        // Subtasks 1-2 (0-1 in zero-index): Interchangeable A-B only
        connectionRules.Add(new ConnectionRule
        {
            subtaskID1 = 0,
            subtaskID2 = 1,
            mustBeDifferentType = true,
            description = "Subtask 1 and 2 must connect A-B (not A-A or B-B)"
        });

        // Subtasks 3-4 (2-3): Can connect to 1-2 in specific pattern
        connectionRules.Add(new ConnectionRule
        {
            subtaskID1 = 2,
            subtaskID2 = 0,
            requiredCompletedSubtasks = new List<int> { 0, 1 },
            mustBeDifferentType = true,
            description = "Subtask 3 must connect to Subtask 1 (A-B) after 1&2 complete"
        });

        connectionRules.Add(new ConnectionRule
        {
            subtaskID1 = 3,
            subtaskID2 = 1,
            requiredCompletedSubtasks = new List<int> { 0, 1, 4, 5 },
            mustBeDifferentType = true,
            description = "Subtask 4 must connect to Subtask 2 (A-B) after 1,2,5,6 complete"
        });

        // Subtasks 5-6 (4-5): Must complete 1-2 first
        connectionRules.Add(new ConnectionRule
        {
            subtaskID1 = 4,
            subtaskID2 = 5,
            requiredCompletedSubtasks = new List<int> { 0, 1 },
            description = "Subtasks 5-6 require 1-2 completed first"
        });

        // Subtasks 7-9 (6-8): Sequential
        for (int i = 6; i <= 8; i++)
        {
            connectionRules.Add(new ConnectionRule
            {
                subtaskID1 = i,
                requiredCompletedSubtasks = Enumerable.Range(0, i).ToList(),
                description = $"Subtask {i+1} requires all previous subtasks completed"
            });
        }
    }

    void Update()
    {
        UpdateValidConnections();
        CalculateAllProgress();
        UpdateUI();
    }

    void UpdateValidConnections()
    {
        validConnections.Clear();

        for (int i = 0; i < idealState.Subtasks.Count; i++)
        {
            for (int j = i + 1; j < idealState.Subtasks.Count; j++)
            {
                if (ArePiecesNearby(i, j))
                {
                    // Check all possible connection types
                    CheckConnectionValidity(i, j);
                }
            }
        }
    }

    void CheckConnectionValidity(int subtask1, int subtask2)
    {
        string key = $"{subtask1}-{subtask2}";
        
        // Check A-A connection
        if (IsConnectionValid(subtask1, subtask2, true))
        {
            validConnections[key+"_AA"] = true;
        }
        
        // Check A-B connection
        if (IsConnectionValid(subtask1, subtask2, false))
        {
            validConnections[key+"_AB"] = true;
        }
        
        // Check B-B connection (rare case)
        if (IsConnectionValid(subtask1, subtask2, true, true))
        {
            validConnections[key+"_BB"] = true;
        }
    }

    bool IsConnectionValid(int subtask1, int subtask2, bool isSameType, bool isBB = false)
    {
        foreach (var rule in connectionRules)
        {
            if ((rule.subtaskID1 == subtask1 && rule.subtaskID2 == subtask2) ||
                (rule.subtaskID1 == subtask2 && rule.subtaskID2 == subtask1))
            {
                // Check type requirements
                if (rule.mustBeSameType && !isSameType) return false;
                if (rule.mustBeDifferentType && isSameType) return false;
                
                // Special case for B-B connections
                if (isBB && rule.mustBeDifferentType) return false;

                // Check dependencies
                foreach (var req in rule.requiredCompletedSubtasks)
                {
                    if (!completedSubtasks.ContainsKey(req)) return false;
                    if (!completedSubtasks[req]) return false;
                }
            }
        }
        return true;
    }

    bool ArePiecesNearby(int subtask1, int subtask2)
    {
        if (!subtaskPieces[subtask1].pieceA || !subtaskPieces[subtask1].pieceB ||
            !subtaskPieces[subtask2].pieceA || !subtaskPieces[subtask2].pieceB)
            return false;

        float minDist = Mathf.Min(
            idealState.Subtasks[subtask1].RelativeDistance,
            idealState.Subtasks[subtask2].RelativeDistance) * 1.5f;

        return 
            Vector3.Distance(subtaskPieces[subtask1].pieceA.transform.position, 
                           subtaskPieces[subtask2].pieceA.transform.position) < minDist ||
            Vector3.Distance(subtaskPieces[subtask1].pieceA.transform.position, 
                           subtaskPieces[subtask2].pieceB.transform.position) < minDist ||
            Vector3.Distance(subtaskPieces[subtask1].pieceB.transform.position, 
                           subtaskPieces[subtask2].pieceA.transform.position) < minDist ||
            Vector3.Distance(subtaskPieces[subtask1].pieceB.transform.position, 
                           subtaskPieces[subtask2].pieceB.transform.position) < minDist;
    }

    void CalculateAllProgress()
    {
        for (int i = 0; i < idealState.Subtasks.Count; i++)
        {
            if (!completedSubtasks[i])
            {
                subtaskProgress[i] = CalculateSubtaskProgress(i);
                if (subtaskProgress[i] >= 0.99f)
                {
                    completedSubtasks[i] = true;
                    Debug.Log($"{idealState.Subtasks[i].SubtaskName} completed!");
                }
            }
        }
    }

    float CalculateSubtaskProgress(int subtaskIndex)
    {
        float maxProgress = 0f;
        var ideal = idealState.Subtasks[subtaskIndex];

        // Check all valid connections involving this subtask
        foreach (var connection in validConnections)
        {
            if (connection.Value) // If connection is valid
            {
                string[] parts = connection.Key.Split('_');
                string[] ids = parts[0].Split('-');
                int id1 = int.Parse(ids[0]);
                int id2 = int.Parse(ids[1]);

                if (id1 == subtaskIndex || id2 == subtaskIndex)
                {
                    string connectionType = parts[1];
                    Transform piece1 = null, piece2 = null;

                    // Get the appropriate pieces based on connection type
                    if (connectionType == "AA")
                    {
                        piece1 = subtaskPieces[id1].pieceA.transform;
                        piece2 = subtaskPieces[id2].pieceA.transform;
                    }
                    else if (connectionType == "AB")
                    {
                        piece1 = subtaskPieces[id1].pieceA.transform;
                        piece2 = subtaskPieces[id2].pieceB.transform;
                    }
                    else if (connectionType == "BB")
                    {
                        piece1 = subtaskPieces[id1].pieceB.transform;
                        piece2 = subtaskPieces[id2].pieceB.transform;
                    }

                    if (piece1 && piece2)
                    {
                        float score = EvaluateConnection(
                            piece1, piece2, 
                            ideal.RelativeDistance, 
                            ideal.AngleDifference);
                        
                        maxProgress = Mathf.Max(maxProgress, score);
                    }
                }
            }
        }

        // Also check individual piece alignment to ideal positions
        maxProgress = Mathf.Max(maxProgress, 
            EvaluateIndividualAlignment(subtaskIndex));

        return maxProgress;
    }

    float EvaluateIndividualAlignment(int subtaskIndex)
    {
        // This checks if pieces are approaching their ideal positions independently
        // Useful for sequential tasks where connection happens later
        return 0f; // Implement based on your needs
    }

    float EvaluateConnection(Transform piece1, Transform piece2, float idealDist, Quaternion idealRot)
    {
        // Position evaluation
        float currentDist = Vector3.Distance(piece1.position, piece2.position);
        float distError = Mathf.Abs(currentDist - idealDist);
        float posScore = Mathf.Clamp01(1 - (distError / positionThreshold));

        // Rotation evaluation
        Quaternion relativeRot = Quaternion.Inverse(piece1.rotation) * piece2.rotation;
        float angleDiff = Quaternion.Angle(relativeRot, idealRot);
        float rotScore = Mathf.Clamp01(1 - (angleDiff / rotationThreshold));

        return (positionWeight * posScore) + (rotationWeight * rotScore);
    }

    void UpdateUI()
    {
        float totalProgress = CalculateTotalProgress();
        progressSlider.value = totalProgress;
        progressText.text = $"{totalProgress * 100f:F1}% Complete";
    }

    float CalculateTotalProgress()
    {
        if (idealState.Subtasks.Count == 0) return 0f;

        float sum = 0f;
        foreach (var progress in subtaskProgress.Values)
        {
            sum += progress;
        }
        return sum / idealState.Subtasks.Count;
    }

    // Debug visualization
    void OnDrawGizmosSelected()
    {
        if (idealState == null || subtaskPieces == null) return;

        for (int i = 0; i < Mathf.Min(idealState.Subtasks.Count, subtaskPieces.Count); i++)
        {
            if (!subtaskPieces[i].pieceA || !subtaskPieces[i].pieceB) continue;

            // Draw ideal connection lines
            Vector3 idealPos = subtaskPieces[i].pieceA.transform.position + 
                              (subtaskPieces[i].pieceA.transform.forward * idealState.Subtasks[i].RelativeDistance);
            
            Gizmos.color = completedSubtasks.ContainsKey(i) && completedSubtasks[i] ? 
                Color.green : Color.yellow;
            Gizmos.DrawLine(subtaskPieces[i].pieceA.transform.position, idealPos);
            Gizmos.DrawWireSphere(idealPos, 0.02f);
        }
    }
}