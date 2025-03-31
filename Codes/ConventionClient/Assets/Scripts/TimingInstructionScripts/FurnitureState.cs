using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FurnitureState : MonoBehaviour
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

    [System.Serializable]
    public class FurnitureConfig
    {
        public string FurnitureName; // Name of the furniture
        public TextAsset IdealStateFile; // JSON file for the ideal state
        public List<GameObject> SubtaskPiecesA; // Subtask objects A
        public List<GameObject> SubtaskPiecesB; // Subtask objects B
    }

    public List<FurnitureConfig> FurnitureConfigs; // List of all furniture configurations
    public Slider progressBar;
    public TextMeshProUGUI progressText;

    private FurnitureConfig activeFurnitureConfig; // Current active furniture configuration
    private IdealStateData idealStateData;

    private float progress;
    private const float positionTolerance = 0.05f;
    private const float orientationTolerance = 2;
    private int currentSubtaskIndex = -1;
    private float currentPositionError = 0f;
    private float currentRotationError = 0f;

    /// <summary>
    /// Sets the active furniture based on the name provided by the manager.
    /// </summary>
    public void SetActiveFurniture(string furnitureName)
    {
        activeFurnitureConfig = FurnitureConfigs.Find(config => config.FurnitureName == furnitureName);

        if (activeFurnitureConfig == null)
        {
            Debug.LogError($"No configuration found for furniture: {furnitureName}");
            return;
        }

        Debug.Log($"Active furniture set to: {furnitureName}");

        LoadIdealState();
        SetSubtaskPieces(activeFurnitureConfig.SubtaskPiecesA, activeFurnitureConfig.SubtaskPiecesB);
    }

    private void LoadIdealState()
    {
        if (activeFurnitureConfig == null || activeFurnitureConfig.IdealStateFile == null)
        {
            Debug.LogError("No ideal state file assigned for the active furniture.");
            return;
        }

        string json = activeFurnitureConfig.IdealStateFile.text; // Read the content of the uploaded file
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

    private void Update()
    {
        CalculateProgress();
    }

    private void CalculateProgress()
    {
        if (idealStateData == null || idealStateData.Subtasks.Count == 0)
        {
            Debug.LogWarning("No subtasks available for progress calculation.");
            return;
        }

        float totalError = 0f;
        bool stopCalculation = false;

        for (int i = 0; i < activeFurnitureConfig.SubtaskPiecesA.Count; i++)
        {
            GameObject pieceA = activeFurnitureConfig.SubtaskPiecesA[i];
            GameObject pieceB = activeFurnitureConfig.SubtaskPiecesB[i];

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
                float diff_diff = Quaternion.Angle(idealSubtask.AngleDifference, currentDiff);

                float positionError = Mathf.Max(0, (Mathf.Abs(currentDistance - idealDistance) - positionTolerance));
                positionError = Mathf.Clamp(positionError / (1f - positionTolerance), 0f, 1f);

                float orientationError = Mathf.Max(0, diff_diff - orientationTolerance) / 180;
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
        //Debug.Log("calcu")

        progress = Mathf.Clamp(1f - (totalError / activeFurnitureConfig.SubtaskPiecesA.Count), 0f, 1f);
        //Debug.Log(progress);
        progressBar.value = progress;
        progressText.text = $"Progress: {Mathf.RoundToInt(progress * 100f)}%";
    }

    private void SetSubtaskPieces(List<GameObject> piecesA, List<GameObject> piecesB)
    {
        if (piecesA.Count != piecesB.Count)
        {
            Debug.LogError("Subtask pieces count mismatch!");
            return;
        }

        Debug.Log("Subtask pieces set for active furniture.");
    }

    public float GetTaskProgress() => progress;
    public int GetCurrentSubtaskIndex() => currentSubtaskIndex;
    public float GetCurrentPositionError() => currentPositionError;
    public float GetCurrentRotationError() => currentRotationError;
}
