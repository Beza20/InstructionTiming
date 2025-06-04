using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AdaptiveIdealStateRecorder : MonoBehaviour
{
    [System.Serializable]
    public class SubtaskData
    {
        public string SubtaskLabel;
        public float RelativeDistance;
        public Quaternion AngleDifference;
    }

    [System.Serializable]
    public class IdealStateData
    {
        public List<SubtaskData> Subtasks = new List<SubtaskData>();
    }

    [System.Serializable]
    public class SubtaskDefinition
    {
        public string Label;            // e.g., Subtask_1a
        public GameObject PieceA;       // Custom object A
        public GameObject PieceB;       // Custom object B
    }
    [Tooltip("Select which subtask to record from the CustomSubtasks list.")]
    public int currentSubtaskIndex = 0;


    public List<SubtaskDefinition> CustomSubtasks;   // Defined in the Inspector
    public string activeFurnitureName = "DefaultFurniture"; // Used in filename
    public string fileDirectory = "Assets";           // Save location for JSON
    private IdealStateData idealStateData = new IdealStateData();

    /// <summary>
    /// Record a single subtask using specified GameObjects.
    /// </summary>
    public void RecordSubtask(string label, GameObject pieceA, GameObject pieceB)
    {
        if (pieceA == null || pieceB == null)
        {
            Debug.LogWarning($"{label}: One or both pieces are null. Skipping.");
            return;
        }

        float distance = Vector3.Distance(pieceA.transform.position, pieceB.transform.position);
        Quaternion angleDiff = Quaternion.Inverse(pieceA.transform.rotation) * pieceB.transform.rotation;

        SubtaskData data = new SubtaskData
        {
            SubtaskLabel = label,
            RelativeDistance = distance,
            AngleDifference = angleDiff
        };

        idealStateData.Subtasks.Add(data);
        Debug.Log($"✅ Recorded {label} → Distance: {distance:F3}, Angle: {angleDiff}");
    }

    /// <summary>
    /// Records all subtasks defined in the inspector list.
    /// </summary>
    [ContextMenu("Record All Custom Subtasks")]
    public void RecordAllCustomSubtasks()
    {
        foreach (var subtask in CustomSubtasks)
        {
            RecordSubtask(subtask.Label, subtask.PieceA, subtask.PieceB);
        }
    }

    [ContextMenu("Record Current Subtask")]
    public void RecordCurrentSubtask()
    {
        if (CustomSubtasks == null || CustomSubtasks.Count == 0)
        {
            Debug.LogWarning("No subtasks defined.");
            return;
        }

        if (currentSubtaskIndex < 0 || currentSubtaskIndex >= CustomSubtasks.Count)
        {
            Debug.LogWarning("Invalid subtask index.");
            return;
        }

        var sub = CustomSubtasks[currentSubtaskIndex];
        RecordSubtask(sub.Label, sub.PieceA, sub.PieceB);
    }


    /// <summary>
    /// Clears all recorded subtasks from memory.
    /// </summary>
    [ContextMenu("Clear All Recorded Subtasks")]
    public void ClearSubtasks()
    {
        idealStateData.Subtasks.Clear();
        Debug.Log("🧹 Cleared all recorded subtasks.");
    }

    /// <summary>
    /// Saves current recorded subtasks to JSON file.
    /// </summary>
    [ContextMenu("Save to JSON")]
    public void SaveToJson()
    {
        if (idealStateData.Subtasks.Count == 0)
        {
            Debug.LogWarning("No subtasks recorded to save.");
            return;
        }

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"IdealStates_{activeFurnitureName}_{timestamp}.json";
        string filePath = Path.Combine(fileDirectory, fileName);

        if (!Directory.Exists(fileDirectory))
        {
            Directory.CreateDirectory(fileDirectory);
        }

        string json = JsonUtility.ToJson(idealStateData, true);
        File.WriteAllText(filePath, json);

        Debug.Log($"📁 Ideal state saved to: {filePath}");
    }
}
