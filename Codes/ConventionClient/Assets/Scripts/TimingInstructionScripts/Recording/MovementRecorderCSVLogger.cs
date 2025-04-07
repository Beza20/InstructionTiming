#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MovementRecorderCSVLogger : MonoBehaviour
{
    [System.Serializable]
    public class ObjectData
    {
        public string name;
        public Vector3 position;
        public Quaternion rotation;
    }

    [Tooltip("List of GameObjects to track each frame.")]
    public List<GameObject> objectsToTrack;

    private int frameCounter = 0;
    private string filePath;
    public StreamWriter csvWriter;

    void Start()
    {
        AutoStartRecording();
    }

    public void OnEnable()
    {
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
    }

    public void OnDisable()
    {
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif
        EndRecordingSession();
    }

    public void AutoStartRecording()
    {
        if (objectsToTrack == null || objectsToTrack.Count == 0)
        {
            Debug.LogWarning("No objects to track. Recording session not started.");
            return;
        }

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"movements_AutoSession_{timestamp}.csv";

        string folderPath = Path.Combine(Application.persistentDataPath, "csv_logs");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        filePath = Path.Combine(folderPath, fileName);
        csvWriter = new StreamWriter(filePath, false);
        csvWriter.WriteLine("TimeSinceStart,ObjectName,PosX,PosY,PosZ,RotX,RotY,RotZ,RotW");

        Debug.Log($"📁 Auto-logging started. Saving to: {filePath}");
    }

    void Update()
    {
        RecordFrame();
    }

    public void RecordFrame()
    {
        if (csvWriter == null) return;

        float timestamp = Time.time;

        foreach (var obj in objectsToTrack)
        {
            if (obj == null) continue;

            Vector3 pos = obj.transform.position;
            Quaternion rot = obj.transform.rotation;

            string line = $"{timestamp:F3},{obj.name},{pos.x:F5},{pos.y:F5},{pos.z:F5},{rot.x:F5},{rot.y:F5},{rot.z:F5},{rot.w:F5}";
            csvWriter.WriteLine(line);
        }

        csvWriter.Flush();
        frameCounter++;
    }

    public void EndRecordingSession()
    {
        if (csvWriter != null)
        {
            csvWriter.Flush();
            csvWriter.Close();
            csvWriter = null;
            Debug.Log($"✅ Auto-recording session ended. File saved at: {filePath}");
        }
    }

#if UNITY_EDITOR
    public void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            Debug.Log(" Exiting play mode. Finalizing CSV log...");
            EndRecordingSession();
        }
    }
#endif
}
