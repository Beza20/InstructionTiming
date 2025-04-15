using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AdaptiveProgressFormulation : MonoBehaviour
{
    [System.Serializable]
    public class SubtaskData
    {
        public string SubtaskName;
        public float RelativeDistance;
        public Quaternion AngleDifference;

        public int GroupID; // 0: interchangeable 1-4, 1: interchangeable 5-6, 2: sequential 7-9
        public bool MustBeSequential;
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
    }

    public List<FurnitureConfig> FurnitureConfigs;
    public Slider progressBar;
    public TextMeshProUGUI progressText;
    public ObjectRotationTracker movementTracker;

    [Header("Progress Evaluation Parameters")]
    [Range(0f, 1f)] public float epsilonP = 0.01f;
    [Range(0f, 1f)] public float epsilonQ = 0.05f;
    [Range(0.01f, 1f)] public float deltaPMax = 0.5f;
    [Range(0f, 1f)] public float wP = 0.5f;
    [Range(0f, 1f)] public float wQ = 0.5f;

    private FurnitureConfig activeFurnitureConfig;
    private IdealStateData idealStateData;

    private Dictionary<int, float> subtaskProgress = new Dictionary<int, float>();
    private Dictionary<int, List<int>> groupedSubtasks = new Dictionary<int, List<int>>();
    private HashSet<int> sequentialGroups = new HashSet<int> { 3 }; // Only group 2 is sequential
    private int lastActiveGroup = -1;
    private float lastKnownProgress = 0f;
    private Dictionary<int, bool> groupSequentialRules = new Dictionary<int, bool>();

    private Transform a;
    private Transform b;

    void Start()
    {
        LoadIdealState(FurnitureConfigs[0]);
    }

    void LoadIdealState(FurnitureConfig config)
    {
        activeFurnitureConfig = config;
        idealStateData = JsonUtility.FromJson<IdealStateData>(config.IdealStateFile.text);

        groupedSubtasks.Clear();
        subtaskProgress.Clear();
        groupSequentialRules.Clear();

        // Define group sequentiality: true = sequential, false = parallel
        groupSequentialRules[0] = false;
        groupSequentialRules[1] = true;
        groupSequentialRules[2] = false;
        groupSequentialRules[3] = true; // This group is gated by completion of groups 0–2

        for (int i = 0; i < idealStateData.Subtasks.Count; i++)
        {
            var subtask = idealStateData.Subtasks[i];

            // Hardcoded group assignment logic
            if (i <= 2) { subtask.GroupID = 0; subtask.MustBeSequential = false; }
            else if (i <= 4) { subtask.GroupID = 1; subtask.MustBeSequential = false; }
            else if (i <= 6) { subtask.GroupID = 2; subtask.MustBeSequential = false; }
            else { subtask.GroupID = 3; subtask.MustBeSequential = true; }

            if (!groupedSubtasks.ContainsKey(subtask.GroupID))
                groupedSubtasks[subtask.GroupID] = new List<int>();

            groupedSubtasks[subtask.GroupID].Add(i);
            subtaskProgress[i] = 0f;

            
        }
    }

    void Update()
    {
        float totalProgress = CalculateProgress();
        progressBar.value = totalProgress;
        progressText.text = $"Progress: {(totalProgress * 100f):F1}%";
    }

    float CalculateProgress()
    {
        int activeGroup = GetActiveGroup();
        if (activeGroup == -1) return 0f;
        Debug.Log($"Active group: {activeGroup}");

        if (activeGroup == -1)
        {
            // Return cached value if user isn't actively moving anything
            return lastKnownProgress;
        }

        // Enforce sequential dependency
        if (sequentialGroups.Contains(activeGroup))
        {
            for (int g = 0; g < activeGroup; g++)
            {
                if (!IsGroupComplete(g))
                    return lastKnownProgress;
            }
        }

        float groupProgress = EvaluateGroupProgress(activeGroup);
        lastKnownProgress = groupProgress;
        lastActiveGroup = activeGroup;
        //Debug.Log("progress in calculate: " + groupProgress);
        return groupProgress;
    }

    float EvaluateGroupProgress(int groupID)
    {
        float groupProgress = 0f;
        var subtaskIndices = groupedSubtasks[groupID];

        foreach (int i in subtaskIndices)
        {
            groupProgress += EvaluateSubtask(i);
        }
        //Debug.Log("group progreess: " + groupProgress/subtaskIndices.Count);

        return groupProgress / subtaskIndices.Count;
    }

    float EvaluateSubtask(int index)
    {
        // Transform a;
        // Transform b;
        if (index == 0){
            if(movementTracker.IsObjectMoving(activeFurnitureConfig.SubtaskPiecesA[index]) && movementTracker.IsObjectMoving(activeFurnitureConfig.SubtaskPiecesB[index]))
            {
                a = activeFurnitureConfig.SubtaskPiecesA[index].transform;
                b = activeFurnitureConfig.SubtaskPiecesB[index].transform;
            }
            else if (!movementTracker.IsObjectMoving(activeFurnitureConfig.SubtaskPiecesA[index]) && movementTracker.IsObjectMoving(activeFurnitureConfig.SubtaskPiecesB[index])){
                a = activeFurnitureConfig.SubtaskPiecesA[index + 1].transform;
                b = activeFurnitureConfig.SubtaskPiecesB[index].transform;
            }
            else {
                a = activeFurnitureConfig.SubtaskPiecesA[index].transform;
                b = activeFurnitureConfig.SubtaskPiecesB[index + 1].transform;

            }
        }
        if (index == 1){
            if(movementTracker.IsObjectMoving(activeFurnitureConfig.SubtaskPiecesA[index]) && movementTracker.IsObjectMoving(activeFurnitureConfig.SubtaskPiecesB[index]))
            {
                a = activeFurnitureConfig.SubtaskPiecesA[index].transform;
                b = activeFurnitureConfig.SubtaskPiecesB[index].transform;
            }
            else if (!movementTracker.IsObjectMoving(activeFurnitureConfig.SubtaskPiecesA[index]) && movementTracker.IsObjectMoving(activeFurnitureConfig.SubtaskPiecesB[index])){
                a = activeFurnitureConfig.SubtaskPiecesA[index - 1].transform;
                b = activeFurnitureConfig.SubtaskPiecesB[index].transform;
            }
            else {
                a = activeFurnitureConfig.SubtaskPiecesA[index].transform;
                b = activeFurnitureConfig.SubtaskPiecesB[index - 1].transform;

            }
        }

        Vector3 r_ij = b.position - a.position;
        float distError = Mathf.Abs(r_ij.magnitude - idealStateData.Subtasks[index].RelativeDistance);
        float posPenalty = Mathf.Max(0f, distError - epsilonP) / (deltaPMax - epsilonP);

        Quaternion q_ij = Quaternion.Inverse(a.rotation) * b.rotation;
        Quaternion q_goal = idealStateData.Subtasks[index].AngleDifference;
        float rotError = Mathf.Abs(1f - Mathf.Abs(Quaternion.Dot(q_ij, q_goal)));
        float rotPenalty = Mathf.Max(0f, rotError - epsilonQ) / (1f - epsilonQ);

        float progress = 1f - (wP * posPenalty + wQ * rotPenalty);
        progress = Mathf.Clamp01(progress);

        // 🔍 DEBUG LOGGING
        //Debug.Log($"Subtask {index} | distError: {distError:F4}, posPenalty: {posPenalty:F4}, rotError: {rotError:F4}, rotPenalty: {rotPenalty:F4}, progress: {progress:F4}");


        subtaskProgress[index] = progress;
        return progress;
    }

    bool IsGroupComplete(int groupID)
    {
        var subtaskIndices = groupedSubtasks[groupID];
        foreach (int i in subtaskIndices)
        {
            if (subtaskProgress[i] < 1f)
                return false;
        }
        return true;
    }

    int GetActiveGroup()
    {
        for (int groupID = 0; groupID <= 2; groupID++)
        {
            foreach (int subtaskIndex in groupedSubtasks[groupID])
            {
                var pieceA = activeFurnitureConfig.SubtaskPiecesA[subtaskIndex];
                var pieceB = activeFurnitureConfig.SubtaskPiecesB[subtaskIndex];

                if (IsMoving(pieceA) || IsMoving(pieceB))
                    return groupID;
            }
        }
        return 0; // Nothing currently moving
    }

    bool IsMoving(GameObject obj)
    {
        return movementTracker != null && movementTracker.IsObjectMoving(obj);
    }

    public void MarkSubtaskComplete(int index)
    {
        subtaskProgress[index] = 1f;
    }
}
