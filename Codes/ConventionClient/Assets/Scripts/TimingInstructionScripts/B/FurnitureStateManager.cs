using System.Collections.Generic; 
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FurnitureStateManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider progressBar;
    public TextMeshProUGUI progressText;
    
    [Header("Configuration")]
    public List<FurnitureConfig> FurnitureConfigs;

    private FurnitureConfig activeConfig;
    private IdealStateData idealState;
    private ConnectionDetector connectionDetector;
    private PieceAssignmentManager assignmentManager;
    private ProgressEvaluator progressEvaluator;

    private void Awake()
    {
        connectionDetector = new ConnectionDetector();
        assignmentManager = new PieceAssignmentManager();
        progressEvaluator = new ProgressEvaluator();
    }

    public void SetActiveFurniture(string furnitureName)
    {
        activeConfig = FurnitureConfigs.Find(c => c.FurnitureName == furnitureName);
        if (activeConfig == null) return;

        idealState = JsonUtility.FromJson<IdealStateData>(activeConfig.IdealStateFile.text);
        assignmentManager.Initialize(activeConfig.InterchangeableGroups, activeConfig.SubtaskPiecesA.Count);
    }

    private void Update()
    {
        if (activeConfig == null || idealState == null) return;

        var connections = connectionDetector.DetectBestConnections(
            activeConfig.SubtaskPiecesA,
            activeConfig.SubtaskPiecesB,
            idealState.Subtasks,
            assignmentManager
        );

        foreach (var conn in connections)
        {
            assignmentManager.CommitConnection(conn);
        }

        var result = progressEvaluator.Evaluate(connections, idealState.Subtasks.Count);
        UpdateUI(result);
    }

    private void UpdateUI(ProgressEvaluator.EvaluationResult result)
    {
        progressBar.value = result.Progress;
        progressText.text = $"Progress: {Mathf.RoundToInt(result.Progress * 100f)}%";
        
        if (result.IssueIndex != -1)
        {
            Debug.Log($"Issue with {activeConfig.SubtaskPiecesA[result.IssueIndex].name} - " +
                     $"Position Error: {result.PositionError:F2}, " +
                     $"Rotation Error: {result.RotationError:F2}");
        }
    }
}