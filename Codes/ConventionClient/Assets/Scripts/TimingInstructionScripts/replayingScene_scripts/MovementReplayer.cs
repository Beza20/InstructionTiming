using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class MovementReplayer : MonoBehaviour
{
    [System.Serializable]
    public class FrameData
    {
        public int frameIndex;
        public List<ObjectData> objects;
    }

    [System.Serializable]
    public class ObjectData
    {
        public string name;
        public Vector3 position;
        public Quaternion rotation;
    }

    [System.Serializable]
    public class RootData
    {
        public List<FrameData> frames;
    }

    public string filePath = "Assets/movements_Rashult_20250115_155918.json";
    public List<GameObject> prefabsToReplay;
    public float replaySpeed = 10f;
    
    [SerializeField] private int _targetFrame = 0; // Serialized field for target frame
    [SerializeField] private Slider replaySlider;

    private List<FrameData> replayFrames = new List<FrameData>();
    private List<GameObject> replayObjects = new List<GameObject>();
    private float replayTime = 0f;
    private int currentFrame = 0;
    private bool isInitialized = false;

    private void Start()
    {
        LoadFromJson(filePath);
        SetupReplayObjects();
        
        if (replayFrames.Count > 0)
        {
            // Calculate start time (2 seconds before target frame)
            float targetTime = _targetFrame / FRAME_RATE;
            float startTime = Mathf.Max(0, targetTime - 10f);
            
            // Convert back to frames
            currentFrame = Mathf.FloorToInt(startTime * FRAME_RATE);
            replayTime = startTime;
            
            UpdateObjectsToFrame(currentFrame);
            isInitialized = true;
        }
        
        if (replaySlider != null)
        {
            replaySlider.onValueChanged.AddListener(OnSliderValueChanged);
        }
    }

    private void Update()
    {
        if (!isInitialized || replayFrames.Count == 0) return;

        replayTime += Time.deltaTime * replaySpeed;
        int newFrame = Mathf.FloorToInt(replayTime * FRAME_RATE);

        if (newFrame != currentFrame && newFrame < replayFrames.Count)
        {
            currentFrame = newFrame;
            UpdateObjectsToFrame(currentFrame);
            
            // Update slider if available
            if (replaySlider != null)
            {
                replaySlider.SetValueWithoutNotify((float)currentFrame / (replayFrames.Count - 1));
            }
        }
    }

    private void OnSliderValueChanged(float value)
    {
        if (replayFrames.Count == 0) return;
        
        currentFrame = Mathf.FloorToInt(value * (replayFrames.Count - 1));
        replayTime = currentFrame / FRAME_RATE;
        UpdateObjectsToFrame(currentFrame);
    }

    private void UpdateObjectsToFrame(int frameIndex)
    {
        foreach (var objData in replayFrames[frameIndex].objects)
        {
            GameObject obj = replayObjects.Find(o => o.name == objData.name);
            if (obj != null)
            {
                obj.transform.position = objData.position;
                obj.transform.rotation = objData.rotation;
            }
        }
    }

    private void LoadFromJson(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError($"File not found: {filePath}");
            return;
        }

        try
        {
            string json = File.ReadAllText(filePath);
            var rootDataArray = JsonUtility.FromJson<RootDataArrayWrapper>("{\"items\":" + json + "}");

            replayFrames = new List<FrameData>();
            foreach (var root in rootDataArray.items)
            {
                replayFrames.AddRange(root.frames);
            }

            Debug.Log($"Loaded {replayFrames.Count} frames.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to parse JSON: {e.Message}");
        }
    }

    private void SetupReplayObjects()
    {
        if (replayFrames.Count == 0) return;

        foreach (var objData in replayFrames[0].objects)
        {
            GameObject prefab = prefabsToReplay.Find(p => p.name == objData.name);
            if (prefab != null)
            {
                GameObject obj = Instantiate(prefab);
                obj.name = objData.name;
                replayObjects.Add(obj);
            }
            else
            {
                Debug.LogWarning($"No matching prefab found for object: {objData.name}");
            }
        }
    }

    [System.Serializable]
    private class RootDataArrayWrapper
    {
        public List<RootData> items;
    }

    public bool HasFinishedReplay()
    {
        if (replayFrames.Count == 0) return true;
        return currentFrame >= replayFrames.Count - 1;
    }

    private const float FRAME_RATE = 13.5f; // Match this with your recording frame rate
}