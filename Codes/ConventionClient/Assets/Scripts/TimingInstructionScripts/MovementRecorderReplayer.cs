#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MovementRecorderReplayer : MonoBehaviour
{
    [System.Serializable]
    public class FrameDataContainer
    {
        public List<FrameData> frames;
    }

    [System.Serializable]
    public class FrameData
    {
        public int frameIndex;
        public List<ObjectData> objects = new List<ObjectData>();
        
    }

    [System.Serializable]
    public class ObjectData
    {
        public string name;
        public Vector3 position;
        public Quaternion rotation;
        
    }

    public List<GameObject> objectsToTrack; // Objects to record
    private List<FrameData> recordedFrames = new List<FrameData>();
    private string fileDirectory = "Assets";
    private string fileName;
    private int frameCounter = 0; // Tracks the current frame index
    private const int saveInterval = 10; // Save every 10 frames
    private int saving = 0;
    
    private void OnEnable()
    {
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif
        EndRecordingSession(); // Ensure recording ends properly
    }

    public void StartRecordingSession(string sessionName)
    {
        if (objectsToTrack == null || objectsToTrack.Count == 0)
        {
            Debug.LogWarning("No objects to track. Recording session not started.");
            return;
        }

        foreach (var obj in objectsToTrack)
        {
            Debug.Log($"Tracking object: {obj.name}");
        }

        recordedFrames.Clear();
        frameCounter = 0;
        fileName = GenerateTimestampedFileName(sessionName);
        Debug.Log($"Recording session started: {sessionName}");
    }


    public void RecordFrame()
    {
        FrameData frameData = new FrameData
        {
            frameIndex = frameCounter,
            objects = new List<ObjectData>()
        };

        foreach (var obj in objectsToTrack)
        {
            if (obj != null)
            {
                frameData.objects.Add(new ObjectData
                {
                    name = obj.name,
                    position = obj.transform.position,
                    rotation = obj.transform.rotation
                });
                //Debug.Log($"Recorded: {obj.name}, Position: {obj.transform.position}, Rotation: {obj.transform.rotation}");
            }
        }

        if (frameData.objects.Count > 0)
        {
            recordedFrames.Add(frameData);
            //Debug.Log($"Frame {recordedFrames.Count} recorded with {frameData.objects.Count} objects.");
            //Debug.Log($"Current recorded frame: {recordedFrames[recordedFrames.Count - 1]}");
        }
        else
        {
            Debug.LogWarning($"Frame {frameCounter} has no objects.");
        }

        frameCounter++;
        if (frameCounter % saveInterval == 0)
        {
           // Debug.Log($"Saving: {saving}");
            saving++;
            SaveToJson();
        }
    }

    public void EndRecordingSession()
    {
        // Save remaining frames and ensure the JSON structure is valid
        SaveToJson(isFinalSave: true);
        Debug.Log("Recording session ended.");
    }

    private void SaveToJson(bool isFinalSave = false)
    {
        if (!Directory.Exists(fileDirectory))
        {
            Directory.CreateDirectory(fileDirectory);
        }

        string filePath = Path.Combine(fileDirectory, fileName);

        //Debug.Log($"Saving frames. Current recordedFrames count: {recordedFrames.Count}");

        if (recordedFrames.Count == 0)
        {
            Debug.LogWarning("No frames to save.");
            return;
        }

        try
        {
            // foreach (var frame in recordedFrames)
            // {
            //     Debug.Log($"Frame: {frame}");
            // }
            FrameDataContainer container = new FrameDataContainer { frames = recordedFrames };
            string jsonToAppend = JsonUtility.ToJson(container, true);
            //Debug.Log($"Serialized JSON: {jsonToAppend}");
            recordedFrames.Clear();
    
            if (File.Exists(filePath))
            {
                string currentContent = File.ReadAllText(filePath).TrimEnd(']', '\n');
                currentContent += ",\n" + jsonToAppend.TrimStart('[') + (isFinalSave ? "\n]" : "");
                File.WriteAllText(filePath, currentContent);
            }
            else
            {
                File.WriteAllText(filePath, "[\n" + jsonToAppend.TrimStart('[') + (isFinalSave ? "\n]" : ""));
            }

            Debug.Log($"Saved {recordedFrames.Count} frames to {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save data to JSON: {e.Message}");
        }

        if (isFinalSave)
        {
            Debug.Log("Clearing recordedFrames after final save.");
            recordedFrames.Clear();
        }
    }




    private string GenerateTimestampedFileName(string sessionName)
    {
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string sanitizedSessionName = string.IsNullOrWhiteSpace(sessionName) ? "UnknownSession" : sessionName.Replace(" ", "_");
        return $"movements_{sanitizedSessionName}_{timestamp}.json";
    }

#if UNITY_EDITOR
    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            Debug.Log("Play mode is stopping, saving data...");
            EndRecordingSession();
        }
    }
#endif
}
