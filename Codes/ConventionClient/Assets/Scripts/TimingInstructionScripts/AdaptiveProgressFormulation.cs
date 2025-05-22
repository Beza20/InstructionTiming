using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

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
        groupSequentialRules[1] = true; // this group must be done only if 0 is completed
        groupSequentialRules[2] = false; // this group can be done first
        groupSequentialRules[3] = true; // This group is gated by completion of groups 0–2

        for (int i = 0; i < idealStateData.Subtasks.Count; i++)
        {
            var subtask = idealStateData.Subtasks[i];

            // Hardcoded group assignment logic
            if (i < 2) { subtask.GroupID = 0; subtask.MustBeSequential = false; }
            else if (i < 4) { subtask.GroupID = 1; subtask.MustBeSequential = false; }
            else if (i < 6) { subtask.GroupID = 2; subtask.MustBeSequential = false; }
            else { subtask.GroupID = 3; subtask.MustBeSequential = true; }

            if (!groupedSubtasks.ContainsKey(subtask.GroupID))
                groupedSubtasks[subtask.GroupID] = new List<int>();

            groupedSubtasks[subtask.GroupID].Add(i);
            subtaskProgress[i] = 0f;

            
        }
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
        progressBar.value = subtaskProgress[0];
        progressBar2.value = subtaskProgress[1];
        progressBar3.value = subtaskProgress[2];
        progressBar4.value = subtaskProgress[3];
        progressBar5.value = subtaskProgress[4];
        progressBar6.value = subtaskProgress[5];
        progressText.text = $"Progress: {(totalProgress * 100f):F1}%";
    }

    float CalculateProgress()
    {
        int activeGroup = GetActiveGroup();
        Debug.Log($"Active group: {activeGroup}");
        // First, check if any groups are now complete (regardless of active group)
        for (int groupID = 0; groupID < groupedSubtasks.Count; groupID++)
        {
            if (IsGroupComplete(groupID) && !completedGroups.ContainsKey(groupID))
            {
                float groupScore = EvaluateGroupProgress(groupID);
                completedGroups[groupID] = groupScore;
                Debug.Log($"Group {groupID} completed with progress {groupScore:F2}!");
            }

        }
        // Sum progress of all completed groups (0 if none)
        float completedGroupsProgress = completedGroups.Values.Sum() / groupedSubtasks.Count;
        
        if (activeGroup == -1) 
        {
            if (lastKnownProgress == 0)
            {
                
                groupProgress = completedGroupsProgress + EvaluateGroupProgress(0);
                lastKnownProgress = groupProgress;
                lastActiveGroup = activeGroup;
                //Debug.Log("progress in group 0");
                
                return groupProgress;

            }   
            
            else
            {
                return lastKnownProgress;
            }
        } 
        if (activeGroup == 1 && !IsGroupComplete(0))
        {
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
        if (!completedGroups.ContainsKey(activeGroup)){
            groupProgress = EvaluateGroupProgress(activeGroup);
            lastKnownProgress = groupProgress;
            lastActiveGroup = activeGroup;

        }

        groupProgress += completedGroupsProgress;
        //Debug.Log("progress in calculate: " + groupProgress);
        return groupProgress;
    }
    

    public float EvaluateGroupProgress(int groupID)
    {
        float groupProgress = 0f;
        var subtaskIndices = groupedSubtasks[groupID];
        bool mustBeSequential = groupSequentialRules.ContainsKey(groupID) && groupSequentialRules[groupID];

        for (int i = 0; i < subtaskIndices.Count; i++)
        {
            int index = subtaskIndices[i];

            if (subtaskProgress[index] >= idealProgress)
            {
                groupProgress += subtaskProgress[index];
                continue;
            }



            //if (idealStateData.Subtasks[index].MustBeSequential && i > 0)
            if (i > 0)
            {
                //Debug.Log("checking if mustbe sequential works");
                int prevIndex = subtaskIndices[i - 1];
                if (subtaskProgress[prevIndex] < idealProgress)
                {
                    Debug.Log("subtask not complete because " + subtaskProgress[prevIndex] + " and ideal progress is " + idealProgress);
                    break; // stop if previous one in sequence is not complete
                }
            }
            
            groupProgress += EvaluateSubtask(index);
        }
        //Debug.Log("when group progress computes " + groupProgress);

        return groupProgress / subtaskIndices.Count;
    }

    (bool found, Transform a, Transform b) GetBestTransformPair(int index)
    {
        GameObject candidateA = activeFurnitureConfig.SubtaskPiecesA[index];
        GameObject candidateB = activeFurnitureConfig.SubtaskPiecesB[index];
        GameObject candidateA1 = activeFurnitureConfig.SubtaskPiecesA[index + 1];
        GameObject candidateB1 = activeFurnitureConfig.SubtaskPiecesB[index + 1];


        bool aMoving = IsMoving(candidateA);
        bool bMoving = IsMoving(candidateB);
        bool a1Moving = IsMoving(candidateA1);
        bool b1Moving = IsMoving(candidateB1);


        if (aMoving && bMoving)
        {
            Debug.Log("a and b moving");
            idealProgress = 0.9f;
            return (true, candidateA.transform, candidateB.transform);
        }
        if (a1Moving && b1Moving)
        {
            idealProgress = 0.9f;
            Debug.Log("a1 and b1 moving");
            return (true, candidateA1.transform, candidateB1.transform);
        }

        if (aMoving && !bMoving && b1Moving)
        {
            Debug.Log("a and b1 moving");
            idealProgress = 0.48f;
            return (true, candidateA.transform, candidateB1.transform);
        }
        if (a1Moving && !b1Moving && bMoving)
        {
            Debug.Log("a1 and b moving");
            idealProgress = 0.68f;
            return (true, candidateA1.transform, candidateB.transform);
        }
        // Check nearby options to accommodate interchangeability
        float bestScore = float.MaxValue;
        Transform bestA = candidateA.transform;
        Transform bestB = candidateB.transform;
        float distErrorBest = Mathf.Abs((bestB.position - bestA.position).magnitude - idealStateData.Subtasks[index].RelativeDistance);
        float rotErrorBest = Mathf.Abs(1f - Mathf.Abs(Quaternion.Dot(Quaternion.Inverse(bestA.rotation) * bestB.rotation, idealStateData.Subtasks[index].AngleDifference)));

        bestScore = distErrorBest + rotErrorBest;
        bool found = false;
        int aIndex = index;
        int bIndex = index;

        int offset = 1; // allow checking adjacent indices
        if (index == 0)
        {
            aIndex = index;
            bIndex = index + offset;

        }
        if (index == 1)
        {
            aIndex = index - offset;
            bIndex = index;
        }
        if (index == 2)
        {
            aIndex = index;
            bIndex = index + offset;

        }
        if (index == 3)
        {
            aIndex = index - offset;
            bIndex = index;
        }
        if (index == 4)
        {
            aIndex = index;
            bIndex = index + offset;

        }
        if (index == 5)
        {
            aIndex = index - offset;
            bIndex = index;
        }
        

        

        if (aIndex >= 0 && aIndex < activeFurnitureConfig.SubtaskPiecesA.Count &&
            bIndex >= 0 && bIndex < activeFurnitureConfig.SubtaskPiecesB.Count)
        {
            Transform aTry = activeFurnitureConfig.SubtaskPiecesA[aIndex].transform;
            Transform bTry = activeFurnitureConfig.SubtaskPiecesB[bIndex].transform;

            float distError = Mathf.Abs((bTry.position - aTry.position).magnitude - idealStateData.Subtasks[index].RelativeDistance);
            float rotError = Mathf.Abs(1f - Mathf.Abs(Quaternion.Dot(Quaternion.Inverse(aTry.rotation) * bTry.rotation, idealStateData.Subtasks[index].AngleDifference)));

            float totalScore = distError + rotError;
            //Debug.Log("best score " + bestScore);
            // Debug.Log("total score " + totalScore);

            if (totalScore < bestScore)
            {
                // Debug.Log("doing the alternative");
                bestScore = totalScore;
                bestA = aTry;
                bestB = bTry;
                found = true;
            }
        }
        found = true;
        return (found, bestA, bestB);
    }

    float EvaluateSubtask(int index)
    {
        var (found, aTransform, bTransform) = GetBestTransformPair(index);
        if (!found) return subtaskProgress[index]; // fallback to cached if nothing suitable
        //Debug.Log("found is true");

        Vector3 r_ij = bTransform.position - aTransform.position;
        float distError = Mathf.Abs(r_ij.magnitude - idealStateData.Subtasks[index].RelativeDistance);
        float posPenalty = Mathf.Max(0f, distError - epsilonP) / (deltaPMax - epsilonP);

        Quaternion q_ij = Quaternion.Inverse(aTransform.rotation) * bTransform.rotation;
        Quaternion q_goal = idealStateData.Subtasks[index].AngleDifference;
        float rotError = Mathf.Abs(1f - Mathf.Abs(Quaternion.Dot(q_ij, q_goal)));
        float rotPenalty = Mathf.Max(0f, rotError - epsilonQ) / (1f - epsilonQ);

        float progress = 1f - (wP * posPenalty + wQ * rotPenalty);
        progress = Mathf.Clamp01(progress);

        subtaskProgress[index] = progress;
        Debug.Log("checking subtaskProgress of " + index + " " + subtaskProgress[index]);
        return progress;
    }


    bool IsGroupComplete(int groupID)
    {
        var subtaskIndices = groupedSubtasks[groupID];
        foreach (int i in subtaskIndices)
        {
            //Debug.Log("subtask progress of checking completion " + i + " is " + subtaskProgress[i]);
            if (subtaskProgress[i] < idealProgress)
            {
                //Debug.Log("subtask progress of checking completion " + i + " is " + subtaskProgress[i]);
                return false;
            }
           
        }
        Debug.Log("group is complete: " + groupID  );
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
        foreach (var kvp in groupedSubtasks)
        {
            int groupID = kvp.Key;
            foreach (int subtaskIndex in kvp.Value)
            {

                var pieceA = activeFurnitureConfig.SubtaskPiecesA[subtaskIndex];
                var pieceB = activeFurnitureConfig.SubtaskPiecesB[subtaskIndex];


                if (IsMoving(pieceA) || IsMoving(pieceB))
                {
                    if(IsMoving(pieceA) )
                    {
                        Debug.Log(pieceA.ToString() + "is moving");
                    }
                    if(IsMoving(pieceB) )
                    {
                        Debug.Log(pieceB.ToString() + "is moving");
                    }
                    if (subtaskIndex == 4 || subtaskIndex == 5)
                    {
                        if (IsMoving(pieceA) && IsGroupComplete(0))
                        {
                            return groupID - 1;
                        }
                        else
                        {
                            return groupID;
                        }

                        
                    }

                    if (subtaskIndex == 2 || subtaskIndex == 3)
                    {
                        if (IsMoving(pieceA) && IsGroupComplete(0))
                        {
                            return groupID;
                        }
                        else
                        {
                            //Debug.Log("group is not complete");
                            return groupID + 1;
                        }

                    } 
                    
                    return groupID;
                }
            }
        }
        return -1;
    }

    bool IsMoving(GameObject obj)
    {
        return movementTracker != null && movementTracker.IsObjectMoving(obj);
    }

    public void MarkSubtaskComplete(int index)
    {
        subtaskProgress[index] = 1f;
    }
    public List<GameObject> GetSubtaskPiecesA()
    {
        return activeFurnitureConfig.SubtaskPiecesA;
    }

    public List<GameObject> GetSubtaskPiecesB()
    {
        return activeFurnitureConfig.SubtaskPiecesB;
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
