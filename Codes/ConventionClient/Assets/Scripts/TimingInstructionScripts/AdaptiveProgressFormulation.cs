using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;

public class AdaptiveProgressFormulation : MonoBehaviour
{
    [System.Serializable]
    public class SubtaskData
    {
        public string SubtaskLabel;        // e.g., "Subtask1A"
        public float RelativeDistance;
        public Quaternion AngleDifference;

        public int GroupID;                // e.g., 0 = interchangeable 1–4, 1 = 5–6, 2 = sequential 7–9
        public bool MustBeSequential;      // Used only if sequential order matters


        [NonSerialized] public string BaseLabel;   // e.g., "Subtask1" (computed at load time)
    }

    [System.Serializable]
    public class IdealStateData
    {
        public List<SubtaskData> Subtasks = new List<SubtaskData>();
    }
    [System.Serializable]
    public class SubtaskDefinition
    {
        public string BaseLabel;           // e.g., "Subtask1"
        public GameObject PieceA;
        public GameObject PieceB;
    }

    [System.Serializable]
    public class FurnitureConfig
    {
        public string FurnitureName;
        public TextAsset IdealStateFile;
        public List<SubtaskDefinition> CurrentSubtasks; // Runtime state of objects
    }

    public List<FurnitureConfig> FurnitureConfigs;
    public Slider progressBar;
    public Slider progressBar2;
    public Slider progressBar3;
    public Slider progressBar4;
    public Slider progressBar5;

    public Slider progressBar6;
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
    private HashSet<int> sequentialGroups = new HashSet<int> {1, 3 }; // Only group 3 subtasks must be  sequential
    private int lastActiveGroup = -1;
    private float lastKnownProgress = 0f;

    private float idealProgress = 0.9f;
    private Dictionary<int, bool> groupSequentialRules = new Dictionary<int, bool>();

    private Transform a;
    private Transform b;
    private float startDelay = 6f;
    private float elapsedTime = 0f;
    private bool started = false;
    private float groupProgress = 0;
    private Dictionary<int, float> completedGroups = new Dictionary<int, float>();
    private Dictionary<int, int> variantDependencies = new Dictionary<int, int>();
    private Dictionary<int, List<List<int>>> variantChains = new Dictionary<int, List<List<int>>>();
    private Dictionary<int, float> groupProgressDict = new Dictionary<int, float>();




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
        variantDependencies.Clear(); // Make sure to declare this as a Dictionary<int, int> elsewhere

        // Define group sequentiality: true = sequential, false = parallel
        groupSequentialRules[0] = false; // Group 0: 1A/B, 2A/B, 3*
        groupSequentialRules[1] = false;  // Group 1: 3A/B, 4A/B
        groupSequentialRules[2] = false; // Group 2: 5A/B, 6A/B
        groupSequentialRules[3] = true;  // Group 3: 7, 8*, 9, 10

        for (int i = 0; i < idealStateData.Subtasks.Count; i++)
        {
            var subtask = idealStateData.Subtasks[i];

            // Assign GroupID and sequentiality based on index
            if (i >= 0 && i <= 4) // Subtasks 1A, 1B, 2A, 2B, 3*
            {
                subtask.GroupID = 0;
                subtask.MustBeSequential = false;
            }
            else if (i >= 5 && i <= 8) // Subtasks 3A, 3B, 4A, 4B
            {
                subtask.GroupID = 1;
                subtask.MustBeSequential = true;
            }
            else if (i >= 9 && i <= 12) // Subtasks 5A, 6A, 5B, 6B
            {
                subtask.GroupID = 2;
                subtask.MustBeSequential = false;
            }
            else if (i >= 13 && i <= 16) // Subtasks 7, 8*, 9, 10
            {
                subtask.GroupID = 3;
                subtask.MustBeSequential = true;
            }

            if (!groupedSubtasks.ContainsKey(subtask.GroupID))
                groupedSubtasks[subtask.GroupID] = new List<int>();

            groupedSubtasks[subtask.GroupID].Add(i);
            subtaskProgress[i] = 0f;
        }

        // Define variant dependencies
        // Group 0 logic (1A/B → 2A/B and vice versa)
        variantDependencies[0] = 1; // 1B → 2B
        variantDependencies[1] = 0; // 2B → 1B
        variantDependencies[3] = 4; // 1A → 2A
        variantDependencies[4] = 3; // 2A → 1A

        // Group 2 logic (5A/B → 6A/B and vice versa)
        variantDependencies[9] = 10;  // 5A → 6A
        variantDependencies[10] = 9;  // 6A → 5A
        variantDependencies[11] = 12; // 5B → 6B
        variantDependencies[12] = 11; // 6B → 5B

        variantChains.Clear();

        // Add chains for Group 0 (1A+2A+3*, etc.)
        variantChains[0] = new List<List<int>>
        {
            new List<int> {
                idealStateData.Subtasks.FindIndex(s => s.SubtaskLabel == "Subtask1A"),
                idealStateData.Subtasks.FindIndex(s => s.SubtaskLabel == "Subtask2A"),
                idealStateData.Subtasks.FindIndex(s => s.SubtaskLabel == "Subtask3*")
            },
            new List<int> {
                idealStateData.Subtasks.FindIndex(s => s.SubtaskLabel == "Subtask2A"),
                idealStateData.Subtasks.FindIndex(s => s.SubtaskLabel == "Subtask1A"),
                idealStateData.Subtasks.FindIndex(s => s.SubtaskLabel == "Subtask3*")
            },
            new List<int> {
                idealStateData.Subtasks.FindIndex(s => s.SubtaskLabel == "Subtask1B"),
                idealStateData.Subtasks.FindIndex(s => s.SubtaskLabel == "Subtask2B"),
                idealStateData.Subtasks.FindIndex(s => s.SubtaskLabel == "Subtask3*")
            },
            new List<int> {
                idealStateData.Subtasks.FindIndex(s => s.SubtaskLabel == "Subtask2B"),
                idealStateData.Subtasks.FindIndex(s => s.SubtaskLabel == "Subtask1B"),
                idealStateData.Subtasks.FindIndex(s => s.SubtaskLabel == "Subtask3*")
            }
        };

        // Add chains for Group 1 (3A+4A, 3B+4B)
        variantChains[1] = new List<List<int>>
        {
            new List<int> {
                idealStateData.Subtasks.FindIndex(s => s.SubtaskLabel == "Subtask3A"),
                idealStateData.Subtasks.FindIndex(s => s.SubtaskLabel == "Subtask4A")
            },
            new List<int> {
                idealStateData.Subtasks.FindIndex(s => s.SubtaskLabel == "Subtask3B"),
                idealStateData.Subtasks.FindIndex(s => s.SubtaskLabel == "Subtask4B")
            }
        };

        // Add chains for Group 2 (5A+6A, 5B+6B)
        variantChains[2] = new List<List<int>>
        {
            new List<int> {
                idealStateData.Subtasks.FindIndex(s => s.SubtaskLabel == "Subtask5A"),
                idealStateData.Subtasks.FindIndex(s => s.SubtaskLabel == "Subtask6A")
            },
            new List<int> {
                idealStateData.Subtasks.FindIndex(s => s.SubtaskLabel == "Subtask5B"),
                idealStateData.Subtasks.FindIndex(s => s.SubtaskLabel == "Subtask6B")
            }
        };
    }

    

    void Update()
    {
        if (!started)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= startDelay)
            {
                started = true;
            }
            else
            {
                return; // wait until 6 seconds have passed
            }
        }
        float totalProgress = CalculateProgress();
        progressBar.value = totalProgress;
        // progressBar2.value = subtaskProgress[1];
        // progressBar3.value = subtaskProgress[2];
        // progressBar4.value = subtaskProgress[3];
        // progressBar5.value = subtaskProgress[4];
        // progressBar6.value = subtaskProgress[5];
        progressText.text = $"Progress: {(totalProgress * 100f):F1}%";
    }

   float CalculateProgress()
    {
        int activeGroup = GetActiveGroup();
        Debug.Log($"Active group: {activeGroup}");

        // Step 1: Update completed group cache
        for (int groupID = 0; groupID < groupedSubtasks.Count; groupID++)
        {
            if (IsGroupComplete(groupID) && !completedGroups.ContainsKey(groupID))
            {
                float groupScore = EvaluateGroupProgress(groupID);
                completedGroups[groupID] = groupScore;
                Debug.Log($"Group {groupID} completed with progress {groupScore:F2}!");
            }
        }

        float completedGroupsProgress = completedGroups.Values.Sum() / groupedSubtasks.Count;

        // Step 2: If no active group, return known or group 0
        if (activeGroup == -1)
        {
            if (lastKnownProgress == 0f)
            {
                float group0Score = EvaluateGroupProgress(0);
                if (IsGroupComplete(0))
                {
                    lastKnownProgress = completedGroupsProgress;
                }
                else
                {
                    lastKnownProgress = completedGroupsProgress + group0Score;
                    lastActiveGroup = 0;
                }
                
                return lastKnownProgress;
            }
            return lastKnownProgress;
        }

        // Step 3: Block if active group depends on incomplete earlier groups
        if (sequentialGroups.Contains(activeGroup))
        {
            for (int g = 0; g < activeGroup; g++)
            {
                if (!IsGroupComplete(g))
                    return lastKnownProgress;
            }
        }
        
        // Step 4: Evaluate active group if not cached
        float currentGroupProgress = completedGroups.ContainsKey(activeGroup)
            ? completedGroups[activeGroup]
            : EvaluateGroupProgress(activeGroup);

        // Step 5: Final aggregation
        float totalProgress = completedGroupsProgress;
        if (!completedGroups.ContainsKey(activeGroup))
        {
            totalProgress += currentGroupProgress;
        }

        lastKnownProgress = totalProgress;
        lastActiveGroup = activeGroup;
        return totalProgress;
    }

    

    public float EvaluateGroupProgress(int groupID)
    {
        
        float groupProgress = 0f;
        var subtaskIndices = groupedSubtasks[groupID];

        /// GROUP 0: (1A+2A+3*) or (1B+2B+3*) or (2A+1A+3*) or (2B+1B+3*)
        if (groupID == 0)
        {
            float bestChainScore = 0f;
    
            // Iterate through all chains defined for group 0
            foreach (var chain in variantChains[0])
            {
                float chainScore = 0f;

                foreach (int subtaskIndex in chain)
                {
                    float score = EvaluateSubtask(subtaskIndex);
                    chainScore += score;
                    //Debug.Log("evaluated subtask " + score);
                }

                chainScore /= chain.Count;
                bestChainScore = Mathf.Max(bestChainScore, chainScore);
            }
            //Debug.Log($"Evaluating group {groupID} and best chain score: {bestChainScore}");
            return bestChainScore;

        }

        if (groupID == 1 && !IsGroupComplete(0))
        {
            Debug.Log("Skipping evaluation of Group 1 because Group 0 is not complete.");
            return 0f;
        }

        if (groupID == 1)
        {
            float bestChainScore = 0f;
    
            /// Iterate through all chains defined for group 0
            foreach (var chain in variantChains[1])
            {
                float chainScore = 0f;

                foreach (int subtaskIndex in chain)
                {
                    float score = EvaluateSubtask(subtaskIndex);
                    chainScore += score;
                    //Debug.Log("evaluated subtask " + score);
                }

                chainScore /= chain.Count;
                bestChainScore = Mathf.Max(bestChainScore, chainScore);
            }
            //Debug.Log($"Evaluating group {groupID} and best chain score: {bestChainScore}");
            return bestChainScore;
        }

        // GROUP 2: (5A+6A) or (5B+6B) — direction doesn't matter
        if (groupID == 2)
        {
            float bestChainScore = 0f;
    
            
            // Iterate through all chains defined for group 2
            foreach (var chain in variantChains[2])
            {
                float chainScore = 0f;

                foreach (int subtaskIndex in chain)
                {
                    float score = EvaluateSubtask(subtaskIndex);
                    chainScore += score;
                    //Debug.Log("evaluated subtask " + score);
                }

                chainScore /= chain.Count;
                bestChainScore = Mathf.Max(bestChainScore, chainScore);
            }
            //Debug.Log($"Evaluating group {groupID} and best chain score: {bestChainScore}");
            return bestChainScore;
            
            return bestChainScore;
        }
        if (groupID == 3 && (!IsGroupComplete(0) || !IsGroupComplete(1) || !IsGroupComplete(2)))
        {
            Debug.Log("Skipping evaluation of Group 3 because Groups 0-2 are not complete.");
            return 0f;
        }

        // GROUP 3: Standard evaluation (sequential optional)
        if (groupID == 3)
        {
            Debug.Log($"variantChains count: {variantChains.Count}");

            float groupScore = 0f;
            int count = 0;

            int idx8 = idealStateData.Subtasks.FindIndex(s => s.SubtaskLabel == "Subtask8*");
            int idx9 = idealStateData.Subtasks.FindIndex(s => s.SubtaskLabel == "Subtask9");

            bool skip8 = idx8 != -1 && idx9 != -1 &&
                        subtaskProgress.ContainsKey(idx9) &&
                        subtaskProgress[idx9] >= idealProgress;

            for (int i = 0; i < subtaskIndices.Count; i++)
            {
                int index = subtaskIndices[i];
                if (index == idx8 && skip8)
                {
                    Debug.Log("Skipping Subtask8* due to Subtask9 completion.");
                    continue;
                }

                // Sequential gate
                if (i > 0)
                {
                    int prevIndex = subtaskIndices[i - 1];
                    if (subtaskProgress[prevIndex] < idealProgress)
                        break;
                }

                groupScore += EvaluateSubtask(index);
                count++;
            }

            return count > 0 ? groupScore / count : 0f;
        }

        return groupProgress / subtaskIndices.Count;
    }


    

    float EvaluateSubtask(int index)
    {
        var subtask = idealStateData.Subtasks[index];
       // Debug.Log($"Entering EvaluateSubtask for {index} – {idealStateData.Subtasks[index].SubtaskLabel}");

        // Find corresponding runtime GameObjects by BaseLabel
        var definition = activeFurnitureConfig.CurrentSubtasks
            .FirstOrDefault(def => def.BaseLabel == subtask.SubtaskLabel);

        if (definition == null || definition.PieceA == null || definition.PieceB == null)
        {
            Debug.LogWarning($"Missing GameObjects for subtask {subtask.SubtaskLabel}");
            return subtaskProgress.ContainsKey(index) ? subtaskProgress[index] : 0f;
        }

        
        if (idealStateData.Subtasks[index].SubtaskLabel == "Subtask8*")
        {
            int idx9 = idealStateData.Subtasks.FindIndex(s => s.SubtaskLabel == "Subtask9");
            if (idx9 != -1 && subtaskProgress.ContainsKey(idx9) && subtaskProgress[idx9] >= idealProgress)
            {
                // Just return the stored (frozen) value — do not re-evaluate
                return subtaskProgress.ContainsKey(index) ? subtaskProgress[index] : 0f;
            }
        }

        Transform aTransform = definition.PieceA.transform;
        Transform bTransform = definition.PieceB.transform;

        Vector3 r_ij = bTransform.position - aTransform.position;
        float distError = Mathf.Abs(r_ij.magnitude - subtask.RelativeDistance);
        float posPenalty = Mathf.Max(0f, distError - epsilonP) / (deltaPMax - epsilonP);

        Quaternion q_ij = Quaternion.Inverse(aTransform.rotation) * bTransform.rotation;
        Quaternion q_goal = subtask.AngleDifference;
        float rotError = Mathf.Abs(1f - Mathf.Abs(Quaternion.Dot(q_ij, q_goal)));
        float rotPenalty = Mathf.Max(0f, rotError - epsilonQ) / (1f - epsilonQ);

        float progress = 1f - (wP * posPenalty + wQ * rotPenalty);
        progress = Mathf.Clamp01(progress);

        subtaskProgress[index] = progress;
        //Debug.Log($"Target distance: {subtask.RelativeDistance}, Actual: {r_ij.magnitude}, PosError: {posPenalty}");
       // Debug.Log($"Angle goal: {subtask.AngleDifference}, Actual: {q_ij}, RotError: {rotPenalty}");

       // Debug.Log($"Progress for subtask {index} ({subtask.SubtaskLabel}) = {progress:F2}");

        return progress;
    }

    public bool IsGroupComplete(int groupID)
    {
        int win_counter = 0;
        int subtask_pos = 0;
        var subtaskIndices = groupedSubtasks[groupID];
        foreach (int i in subtaskIndices)
        {
            //Debug.Log("subtask progress of checking completion " + i + " is " + subtaskProgress[i]);
            if (subtaskProgress[i] < idealProgress)
            {
                if (groupID == 0 && win_counter < 3 && subtask_pos == groupedSubtasks[groupID].Count - 1)
                {
                    return false;

                }

                if ((groupID == 1 || groupID == 2) && win_counter < 2 && subtask_pos == groupedSubtasks[groupID].Count - 1)
                {
                    return false;

                }
                if ((groupID == 3))
                {
                    return false;
                }
                //Debug.Log("subtask progress of checking completion " + i + " is " + subtaskProgress[i]);    
            }
            if (subtaskProgress[i] > idealProgress)
            {
                win_counter++;
            }
            subtask_pos++;
           
           
        }
        Debug.Log("group " + groupID + " is complete"  );
        return true;
    }
    public bool IsSubtaskComplete(int groupID)
    {
        var subtaskIndices = groupedSubtasks[groupID];
        foreach (int i in subtaskIndices)
        {
            Debug.Log("subtaskprogress is " + subtaskProgress[i]);
            if (subtaskProgress[i] > idealProgress){
                
                return true;
            }
           
        }
        Debug.Log("group is not complete: " + groupID);
        return false;

    }
    

    public int GetActiveGroup()
    {
        //Debug.Log($"Checking groups: {string.Join(", ", groupedSubtasks.Keys)}");
        foreach (var kvp in groupedSubtasks)
        {
            int groupID = kvp.Key;

            foreach (int subtaskIndex in kvp.Value)
            {
                string label = idealStateData.Subtasks[subtaskIndex].SubtaskLabel;

                var definition = activeFurnitureConfig.CurrentSubtasks
                    .FirstOrDefault(def => def.BaseLabel == label);  // Match exact label
                if (definition == null)
                    Debug.LogWarning($"No subtask definition found for base label: {label}");

                if (definition == null || definition.PieceA == null || definition.PieceB == null)
                    continue;

                bool aMoving = IsMoving(definition.PieceA);
                bool bMoving = IsMoving(definition.PieceB);

                if (aMoving || bMoving)
                {
                    if (aMoving) Debug.Log($"{definition.PieceA.name} is moving");
                    if (bMoving) Debug.Log($"{definition.PieceB.name} is moving");
                    if (groupID == 0  && IsGroupComplete(0) )
                    {
                        continue;

                    }
                    if (groupID == 1  && IsGroupComplete(1) )
                    {
                        continue;

                    }
                    if (groupID == 2  && IsGroupComplete(2) )
                    {
                        continue;

                    }

                    // Gate Group 1 on Group 0 completion
                    if (groupID == 1 && !IsGroupComplete(0))
                    {
                        Debug.Log("Group 1 blocked: Group 0 not complete.");
                        return 2;
                    }

                    if (groupID == 3 && !IsGroupComplete(0))
                    {
                        Debug.Log("Group 3 blocked: Group 0 not complete.");
                        return 0;
                    }
                    if (groupID == 3 && !IsGroupComplete(1))
                    {
                        Debug.Log("Group 3 blocked: Group 1 not complete.");
                        return 1;
                    }

                    if (groupID == 3 && !IsGroupComplete(2))
                    {
                        Debug.Log("Group 3 blocked: Group 2 not complete.");
                        return 2;
                    }

                    // Gate Group 3 on Groups 0–2 completion
                    if (groupID == 3)
                    {
                        for (int i = 0; i <= 2; i++)
                        {
                            if (!IsGroupComplete(i))
                            {
                                Debug.Log($"Group 3 blocked: Group {i} not complete.");
                                return -1;
                            }
                        }
                    }

                    return groupID;
                }
            }
        }

        return -1; // No active group found
    }

    public bool IsMoving(GameObject obj)
    {
        return movementTracker != null && movementTracker.IsObjectMoving(obj);
    }

    public void MarkSubtaskComplete(int index)
    {
        subtaskProgress[index] = 1f;
    }
    
    public List<GameObject> GetSubtaskPiecesA()
    {
        return activeFurnitureConfig.CurrentSubtasks.Select(s => s.PieceA).ToList();
    }

    public List<GameObject> GetSubtaskPiecesB()
    {
        return activeFurnitureConfig.CurrentSubtasks.Select(s => s.PieceB).ToList();
    }

    public Dictionary<int, List<int>> GetGroupedSubtasks()
    {
        return groupedSubtasks;
    }
    public float GetTotalProgress()
    {
        return progressBar.value;
    }
}
