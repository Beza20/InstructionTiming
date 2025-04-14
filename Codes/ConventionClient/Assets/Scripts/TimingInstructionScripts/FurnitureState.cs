using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FurnitureState : MonoBehaviour
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
    public class FurnitureConfig
    {
        public string FurnitureName;
        public TextAsset IdealStateFile;
        public List<GameObject> SubtaskPiecesA;
        public List<GameObject> SubtaskPiecesB;
        public List<int[]> InterchangeableGroups; // Groups of indices for interchangeable pieces
    }

    public List<FurnitureConfig> FurnitureConfigs;
    public Slider progressBar;
    public TextMeshProUGUI progressText;

    private FurnitureConfig activeFurnitureConfig;
    private IdealStateData idealStateData;
    private Dictionary<int, int> pieceAssignments = new Dictionary<int, int>();
    private Dictionary<int, List<int>> groupLookup = new Dictionary<int, List<int>>();

    private float progress;
    private const float positionTolerance = 0.05f;
    private const float orientationTolerance = 2f;
    private int currentIssueIndex = -1;
    private float currentPositionError = 0f;
    private float currentRotationError = 0f;

    public void SetActiveFurniture(string furnitureName)
    {
        activeFurnitureConfig = FurnitureConfigs.Find(config => config.FurnitureName == furnitureName);
        if (activeFurnitureConfig == null)
        {
            Debug.LogError($"No configuration found for furniture: {furnitureName}");
            return;
        }

        LoadIdealState();
        InitializeInterchangeableGroups();
    }

    private void LoadIdealState()
    {
        if (activeFurnitureConfig?.IdealStateFile == null)
        {
            Debug.LogError("No ideal state file assigned");
            return;
        }

        idealStateData = JsonUtility.FromJson<IdealStateData>(activeFurnitureConfig.IdealStateFile.text);
        if (idealStateData?.Subtasks == null)
        {
            Debug.LogError("Failed to load ideal state data");
        }
    }

    private void InitializeInterchangeableGroups()
    {
        pieceAssignments.Clear();
        groupLookup.Clear();

        // Initialize all pieces as unassigned (-1 means no assignment)
        for (int i = 0; i < activeFurnitureConfig.SubtaskPiecesA.Count; i++)
        {
            pieceAssignments[i] = -1;
        }

        // Build group lookup
        if (activeFurnitureConfig.InterchangeableGroups != null)
        {
            foreach (var group in activeFurnitureConfig.InterchangeableGroups)
            {
                int groupId = groupLookup.Count;
                groupLookup[groupId] = new List<int>(group);
            }
        }
    }

    private void Update()
    {
        CalculateProgress();
    }

    private void CalculateProgress()
    {
        if (idealStateData == null || activeFurnitureConfig == null) return;

        // First detect all current connections
        List<DetectedConnection> currentConnections = DetectCurrentConnections();

        // Resolve piece assignments based on current connections
        ResolvePieceAssignments(currentConnections);

        // Evaluate progress based on resolved assignments
        EvaluateProgressWithAssignments(currentConnections);
    }

    private List<DetectedConnection> DetectCurrentConnections()
    {
        List<DetectedConnection> connections = new List<DetectedConnection>();

        for (int i = 0; i < activeFurnitureConfig.SubtaskPiecesA.Count; i++)
        {
            GameObject pieceA = activeFurnitureConfig.SubtaskPiecesA[i];
            if (pieceA == null) continue;

            // Check against all potential B pieces (including interchangeable ones)
            for (int j = 0; j < activeFurnitureConfig.SubtaskPiecesB.Count; j++)
            {
                GameObject pieceB = activeFurnitureConfig.SubtaskPiecesB[j];
                if (pieceB == null) continue;

                float distance = Vector3.Distance(pieceA.transform.position, pieceB.transform.position);
                Quaternion rotationDiff = Quaternion.Inverse(pieceA.transform.rotation) * pieceB.transform.rotation;

                // Check against the ideal state for this subtask
                SubtaskData ideal = idealStateData.Subtasks[i];
                if (Mathf.Abs(distance - ideal.RelativeDistance) <= positionTolerance &&
                    Quaternion.Angle(rotationDiff, ideal.AngleDifference) <= orientationTolerance)
                {
                    connections.Add(new DetectedConnection {
                        PieceAIndex = i,
                        PieceBIndex = j,
                        Distance = distance,
                        RotationDiff = rotationDiff,
                        MatchedSubtaskIndex = i
                    });
                }
            }
        }
        return connections;
    }

    private void ResolvePieceAssignments(List<DetectedConnection> connections)
    {
        // Reset assignments
        for (int i = 0; i < pieceAssignments.Count; i++)
        {
            pieceAssignments[i] = -1;
        }

        // Process connections to assign pieces
        foreach (var conn in connections)
        {
            // If this pieceA isn't assigned yet
            if (pieceAssignments[conn.PieceAIndex] == -1)
            {
                // Check if pieceB is in any interchangeable group
                bool assigned = false;
                foreach (var group in groupLookup.Values)
                {
                    if (group.Contains(conn.PieceBIndex))
                    {
                        // Assign to the first piece in the group (canonical assignment)
                        pieceAssignments[conn.PieceAIndex] = group[0];
                        assigned = true;
                        break;
                    }
                }

                if (!assigned)
                {
                    // Not in any group, assign directly
                    pieceAssignments[conn.PieceAIndex] = conn.PieceBIndex;
                }
            }
        }
    }

    private void EvaluateProgressWithAssignments(List<DetectedConnection> connections)
    {
        float totalError = 0f;
        int evaluatedSubtasks = 0;
        currentIssueIndex = -1;
        currentPositionError = 0f;
        currentRotationError = 0f;

        for (int i = 0; i < idealStateData.Subtasks.Count; i++)
        {
            float subtaskError = 1f; // Default to full error
            float positionError = 0f;
            float rotationError = 0f;

            // Find if this subtask is satisfied by any connection
            foreach (var conn in connections)
            {
                // Check if this connection satisfies the subtask (considering assignments)
                if (conn.PieceAIndex == i && 
                   (conn.PieceBIndex == pieceAssignments[i] || 
                    IsInSameGroup(conn.PieceBIndex, pieceAssignments[i])))
                {
                    // Calculate exact errors
                    positionError = Mathf.Abs(conn.Distance - idealStateData.Subtasks[i].RelativeDistance);
                    rotationError = Quaternion.Angle(conn.RotationDiff, idealStateData.Subtasks[i].AngleDifference);

                    // Normalize errors
                    float normPosError = Mathf.Clamp01(positionError / positionTolerance);
                    float normRotError = Mathf.Clamp01(rotationError / orientationTolerance);
                    subtaskError = 0.4f * normPosError + 0.6f * normRotError;
                    break;
                }
            }

            // Track the worst error for user feedback
            if (subtaskError > 0.05f && 
               (currentIssueIndex == -1 || positionError + rotationError > currentPositionError + currentRotationError))
            {
                currentIssueIndex = i;
                currentPositionError = positionError;
                currentRotationError = rotationError;
            }

            totalError += subtaskError;
            evaluatedSubtasks++;
        }

        progress = evaluatedSubtasks > 0 ? 1f - (totalError / evaluatedSubtasks) : 0f;
        progressBar.value = progress;
        progressText.text = $"Progress: {Mathf.RoundToInt(progress * 100f)}%";
    }

    private bool IsInSameGroup(int index1, int index2)
    {
        if (index1 == index2) return true;
        foreach (var group in groupLookup.Values)
        {
            if (group.Contains(index1) && group.Contains(index2))
                return true;
        }
        return false;
    }

    private struct DetectedConnection
    {
        public int PieceAIndex;
        public int PieceBIndex;
        public int MatchedSubtaskIndex;
        public float Distance;
        public Quaternion RotationDiff;
    }

    public float GetTaskProgress() => progress;
    public int GetCurrentIssueIndex() => currentIssueIndex;
    public float GetCurrentPositionError() => currentPositionError;
    public float GetCurrentRotationError() => currentRotationError;
}