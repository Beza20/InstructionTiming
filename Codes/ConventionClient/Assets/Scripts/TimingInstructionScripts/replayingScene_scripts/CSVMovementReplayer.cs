using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class CSVMovementReplayer : MonoBehaviour
{
    public string csvFilePath = "Assets/csv_logs/Objectmovement_recording_20250115_155918.csv";
    public List<GameObject> prefabsToReplay;
    public float replaySpeed = 1f;
    public Slider replaySlider;

    private Dictionary<string, GameObject> replayObjects = new();
    private List<FrameEntry> allFrames = new();
    private double startTime;
    private double currentTime;
    private int currentIndex = 0;
    private bool isReady = false;

    private class FrameEntry
    {
        public double timestamp;
        public string objectName;
        public Vector3 position;
        public Quaternion rotation;
    }

    void Start()
    {
        LoadCSV();
        if (allFrames.Count > 0)
        {
            startTime = allFrames[0].timestamp;
            currentTime = startTime;
            isReady = true;
        }

        if (replaySlider != null)
            replaySlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    void Update()
    {
        if (!isReady || allFrames.Count == 0) return;

        currentTime += Time.deltaTime * replaySpeed;

        while (currentIndex < allFrames.Count && allFrames[currentIndex].timestamp <= currentTime)
        {
            var entry = allFrames[currentIndex];
            if (replayObjects.TryGetValue(entry.objectName, out var obj))
            {
                obj.transform.position = entry.position;
                obj.transform.rotation = entry.rotation;
            }
            currentIndex++;
        }

        if (replaySlider != null && allFrames.Count > 0)
        {
            float progress = (float)((currentTime - startTime) / (allFrames[^1].timestamp - startTime));
            replaySlider.SetValueWithoutNotify(progress);
        }
    }

    private void OnSliderValueChanged(float value)
    {
        if (allFrames.Count == 0) return;

        double targetTimestamp = startTime + value * (allFrames[^1].timestamp - startTime);
        currentTime = targetTimestamp;

        // Reset replay objects
        foreach (var obj in replayObjects.Values)
        {
            obj.transform.position = Vector3.zero;
            obj.transform.rotation = Quaternion.identity;
        }

        // Rewind to correct state
        currentIndex = 0;
        while (currentIndex < allFrames.Count && allFrames[currentIndex].timestamp <= currentTime)
        {
            var entry = allFrames[currentIndex];
            if (replayObjects.TryGetValue(entry.objectName, out var obj))
            {
                obj.transform.position = entry.position;
                obj.transform.rotation = entry.rotation;
            }
            currentIndex++;
        }
    }

    private void LoadCSV()
    {
        if (!File.Exists(csvFilePath))
        {
            Debug.LogError("CSV file not found: " + csvFilePath);
            return;
        }

        string[] lines = File.ReadAllLines(csvFilePath);
        for (int i = 1; i < lines.Length; i++) // skip header
        {
            var tokens = lines[i].Split(',');
            if (tokens.Length < 9) continue;

            var entry = new FrameEntry
            {
                timestamp = double.Parse(tokens[0], CultureInfo.InvariantCulture),
                objectName = tokens[1],
                position = new Vector3(
                    float.Parse(tokens[2], CultureInfo.InvariantCulture),
                    float.Parse(tokens[3], CultureInfo.InvariantCulture),
                    float.Parse(tokens[4], CultureInfo.InvariantCulture)
                ),
                rotation = new Quaternion(
                    float.Parse(tokens[5], CultureInfo.InvariantCulture),
                    float.Parse(tokens[6], CultureInfo.InvariantCulture),
                    float.Parse(tokens[7], CultureInfo.InvariantCulture),
                    float.Parse(tokens[8], CultureInfo.InvariantCulture)
                )
            };
            allFrames.Add(entry);
        }

        // Instantiate objects only once
        HashSet<string> instantiated = new();
        foreach (var entry in allFrames)
        {
            if (!instantiated.Contains(entry.objectName))
            {
                GameObject prefab = prefabsToReplay.Find(p => p.name == entry.objectName);
                if (prefab != null)
                {
                    var obj = Instantiate(prefab);
                    obj.name = entry.objectName;
                    replayObjects[entry.objectName] = obj;
                    instantiated.Add(entry.objectName);
                }
                else
                {
                    Debug.LogWarning($"Missing prefab for {entry.objectName}");
                }
            }
        }
    }
}
