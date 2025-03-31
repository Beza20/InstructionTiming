using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class InstructionLogger : MonoBehaviour
{
    public MovementReplayer movementReplayer; // Assignable in Inspector
    public RecordedFurnitureState recordedFurnitureState; // Updated to use recorded furniture state
    public TimelyInstructionRecorded timelyInstruction;

    private float timeElapsed = 0f;
    private List<InstructionLogEntry> instructionLogEntries = new List<InstructionLogEntry>();
    private bool replayComplete = false;

    [System.Serializable]
    private class InstructionLogEntry
    {
        public float timestamp;
        public string triggeredState;
        public float taskProgress;
    }

    void Start()
    {
        if (timelyInstruction != null)
        {
            timelyInstruction.OnInstructionTriggered += LogInstruction;
            Debug.Log("✅ InstructionLogger successfully subscribed to OnInstructionTriggered.");
        }
        else
        {
            Debug.LogError("TimelyInstructionRecorded is not assigned!");
        }
    }

    void Update()
    {
        if (movementReplayer == null || replayComplete)
            return;

        timeElapsed += Time.deltaTime;
        
        if (movementReplayer.HasFinishedReplay())
        {
            SaveInstructionLog();
            replayComplete = true;
        }
    }

    private void LogInstruction(string state)
    {
        float progress = recordedFurnitureState != null ? recordedFurnitureState.GetTaskProgress() : 0f;
        InstructionLogEntry newEntry = new InstructionLogEntry { timestamp = timeElapsed, triggeredState = state, taskProgress = progress };
        AppendInstructionToFile(newEntry);
        Debug.Log("Instruction triggered at time: " + timeElapsed + " seconds, State: " + state + ", Progress: " + progress);
    }

    private void AppendInstructionToFile(InstructionLogEntry entry)
    {
        string savePath = Path.Combine(Application.persistentDataPath, "instruction_log.json");
        List<InstructionLogEntry> existingEntries = new List<InstructionLogEntry>();

        // Read existing file if it exists
        if (File.Exists(savePath))
        {
            string existingJson = File.ReadAllText(savePath);
            if (!string.IsNullOrEmpty(existingJson))
            {
                existingEntries = JsonUtility.FromJson<InstructionLogWrapper>(existingJson)?.entries ?? new List<InstructionLogEntry>();
            }
        }

        // Append the new entry
        existingEntries.Add(entry);

        // Save updated log
        InstructionLogWrapper wrapper = new InstructionLogWrapper { entries = existingEntries };
        File.WriteAllText(savePath, JsonUtility.ToJson(wrapper, true));
        Debug.Log("Instruction log updated and appended to: " + savePath);
    }

    private void SaveInstructionLog()
    {
        Debug.Log("Final instruction log save triggered.");
    }

    [System.Serializable]
    private class InstructionLogWrapper
    {
        public List<InstructionLogEntry> entries;
    }

    void OnDestroy()
    {
        if (timelyInstruction != null)
        {
            timelyInstruction.OnInstructionTriggered -= LogInstruction;
        }
    }
}
