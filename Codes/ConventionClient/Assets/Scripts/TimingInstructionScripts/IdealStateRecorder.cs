using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class IdealStateRecorder : MonoBehaviour
{
    [System.Serializable]
    public class FurnitureConfig
    {
        public string Name; // Name of the furniture
        public List<GameObject> SubtaskPiecesA; // First set of objects for this furniture
        public List<GameObject> SubtaskPiecesB; // Second set of objects for this furniture
    }

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

    public List<FurnitureConfig> FurnitureConfigs; // List of all furniture configurations
    private FurnitureConfig activeConfig; // Currently active furniture configuration
    private IdealStateData idealStateData = new IdealStateData();
    private string fileDirectory = "Assets";

    /// <summary>
    /// Sets the active furniture configuration by name.
    /// Called by FurnitureManager when the selected furniture changes.
    /// </summary>
    /// 
    void Start()
    {
        string defaultFurnitureName = "Rashult"; // Change to match your actual config name

        SetActiveFurniture(defaultFurnitureName);

        if (activeConfig != null)
        {
            SaveIdealState();
        }
    }
    public void SetActiveFurniture(string furnitureName)
    {
        if (FurnitureConfigs == null || FurnitureConfigs.Count == 0)
        {
            Debug.LogError("FurnitureConfigs is empty or null. Please add configurations in the Inspector.");
            activeConfig = null;
            return;
        }

        activeConfig = FurnitureConfigs.Find(config => config.Name == furnitureName);

        if (activeConfig == null)
        {
            Debug.LogError($"No configuration found for furniture: {furnitureName}. Make sure the name matches exactly.");
        }
        else
        {
            Debug.Log($"Active furniture updated to: {furnitureName}");
        }
    }

    /// <summary>
    /// Saves the ideal state for the currently active furniture configuration.
    /// </summary>
    public void SaveIdealState()
    {
        if (activeConfig == null)
        {
            Debug.LogError("No active furniture configuration. Cannot save ideal state.");
            return;
        }

        // Clear previous data
        idealStateData.Subtasks.Clear();

        // Calculate the ideal state for the active furniture
        for (int i = 0; i < activeConfig.SubtaskPiecesA.Count; i++)
        {
            GameObject pieceA = activeConfig.SubtaskPiecesA[i];
            GameObject pieceB = activeConfig.SubtaskPiecesB[i];

            if (pieceA == null || pieceB == null)
            {
                Debug.LogWarning($"Subtask {i + 1}: One or both pieces are null. Skipping.");
                continue;
            }

            float distance = Vector3.Distance(pieceA.transform.position, pieceB.transform.position);
            Quaternion angleDifference = Quaternion.Inverse(pieceA.transform.rotation) * pieceB.transform.rotation;

            SubtaskData subtaskData = new SubtaskData
            {
                SubtaskName = $"Subtask_{i + 1}",
                RelativeDistance = distance,
                AngleDifference = angleDifference
            };

            idealStateData.Subtasks.Add(subtaskData);
        }

        SaveToJson();
    }

    /// <summary>
    /// Saves the ideal state data to a JSON file.
    /// </summary>
    private void SaveToJson()
    {
        if (activeConfig == null)
        {
            Debug.LogError("No active furniture configuration to save.");
            return;
        }

        // Generate the directory name with furniture name and timestamp
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        //string directoryName = $"IdealStates_{activeConfig.Name}_{timestamp}";
        //string directoryPath = Path.Combine("Assets", directoryName);

        // Create the directory if it doesn't exist
        if (!Directory.Exists(fileDirectory))
        {
            Directory.CreateDirectory(fileDirectory);
        }

        // Generate the file name
        string fileName =  $"IdealStates_{activeConfig.Name}_{timestamp}.json";
        string filePath = Path.Combine(fileDirectory, fileName);

        // Serialize the data to JSON and write to the file
        string json = JsonUtility.ToJson(idealStateData, true);
        File.WriteAllText(filePath, json);

        Debug.Log($"Ideal state saved to {filePath}");
    }
}
