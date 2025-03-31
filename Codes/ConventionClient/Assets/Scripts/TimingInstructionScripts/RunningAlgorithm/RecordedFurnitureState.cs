using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RecordedFurnitureState : MonoBehaviour
{
    [System.Serializable]
    public class SubtaskData
    {
        public string SubtaskName;
        public float RelativeDistance; // Ideal relative distance
        public Quaternion AngleDifference; // Ideal angular difference
    }

    [System.Serializable]
    public class IdealStateData
    {
        public List<SubtaskData> Subtasks = new List<SubtaskData>();
    }

    public TextAsset IdealStateFile; // JSON file for the ideal state
    public List<GameObject> SubtaskPiecesA; // Subtask objects A from the replay
    public List<GameObject> SubtaskPiecesB; // Subtask objects B from the replay

    private IdealStateData idealStateData;
    private float progress;
    private const float positionTolerance = 0.05f;
    private const float orientationTolerance = 2;
    private int currentSubtaskIndex = -1;
    private float currentPositionError = 0f;
    private float currentRotationError = 0f;

    void Start()
    {
        LoadIdealState();
    }

    private void LoadIdealState()
    {
        if (IdealStateFile == null)
        {
            Debug.LogError("No ideal state file assigned.");
            return;
        }

        string json = IdealStateFile.text;
        idealStateData = JsonUtility.FromJson<IdealStateData>(json);

        if (idealStateData != null && idealStateData.Subtasks.Count > 0)
        {
            Debug.Log("Ideal state data loaded successfully.");
        }
        else
        {
            Debug.LogError("Failed to load ideal state data or no subtasks available.");
        }
    }

    public void UpdateWithRecordedObjects(List<GameObject> recordedObjectsA, List<GameObject> recordedObjectsB)
    {
        SubtaskPiecesA = recordedObjectsA;
        SubtaskPiecesB = recordedObjectsB;
        Debug.Log("Subtask pieces updated from replay.");
    }

    private void Update()
    {
        CalculateProgress();
    }

    private void CalculateProgress()
    {
        if (idealStateData == null || idealStateData.Subtasks.Count == 0 || SubtaskPiecesA.Count != SubtaskPiecesB.Count)
        {
            Debug.LogWarning("Invalid data for progress calculation.");
            return;
        }

        float totalError = 0f;
        bool stopCalculation = false;

        for (int i = 0; i < SubtaskPiecesA.Count; i++)
        {
            GameObject pieceA = SubtaskPiecesA[i];
            GameObject pieceB = SubtaskPiecesB[i];

            if (pieceA == null || pieceB == null)
            {
                Debug.LogError($"Invalid objects for subtask {i + 1}");
                continue;
            }

            float subtaskError = 0f;

            if (!stopCalculation)
            {
                float currentDistance = Vector3.Distance(pieceA.transform.position, pieceB.transform.position);
                SubtaskData idealSubtask = idealStateData.Subtasks[i];
                float idealDistance = idealSubtask.RelativeDistance;
                Quaternion currentDiff = Quaternion.Inverse(pieceA.transform.rotation) * pieceB.transform.rotation;
                float diffDiff = Quaternion.Angle(idealSubtask.AngleDifference, currentDiff);

                float positionError = Mathf.Max(0, (Mathf.Abs(currentDistance - idealDistance) - positionTolerance));
                positionError = Mathf.Clamp(positionError / (1f - positionTolerance), 0f, 1f);

                float orientationError = Mathf.Max(0, diffDiff - orientationTolerance) / 180;
                orientationError = Mathf.Clamp(orientationError / (1f - (orientationTolerance / 180)), 0f, 1f);

                subtaskError = Mathf.Clamp((0.4f * positionError) + (0.6f * orientationError), 0f, 1f);

                if (subtaskError > 0.05f)
                {
                    currentSubtaskIndex = i;
                    currentPositionError = positionError;
                    currentRotationError = orientationError;
                    stopCalculation = true; // Skip subsequent subtasks
                    Debug.Log($"Subtask {i + 1} is incomplete. Skipping subsequent subtasks.");
                }
            }
            else
            {
                subtaskError = 1f;
            }

            totalError += subtaskError;
        }

        progress = Mathf.Clamp(1f - (totalError / SubtaskPiecesA.Count), 0f, 1f);
        Debug.Log($"Current progress: {progress * 100f}%");
    }

    public float GetTaskProgress() => progress;
    public int GetCurrentSubtaskIndex() => currentSubtaskIndex;
    public float GetCurrentPositionError() => currentPositionError;
    public float GetCurrentRotationError() => currentRotationError;
}
