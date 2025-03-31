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
    public List<GameObject> prefabsToReplay; // Prefabs for replay
    public float replaySpeed = 10f;

    private List<FrameData> replayFrames = new List<FrameData>();
    private List<GameObject> replayObjects = new List<GameObject>();
    private float replayTime = 0f;
    private int currentFrame = 0;

    [SerializeField] Slider replay_slider;

    private void Start()
    {
        LoadFromJson(filePath);
        SetupReplayObjects();
        StartReplaying();
        replay_slider.onValueChanged.AddListener (delegate {ValueChangeCheck ();});
    }

    private void Update()
    {
        if (replayFrames.Count > 0)
        {
            ReplayMovement();
        }
    }

    public void ValueChangeCheck()
	{
		Debug.Log (replay_slider.value);
        currentFrame = (int)(replayFrames.Count * replay_slider.value);
        replayTime = (float)currentFrame;


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
            // Parse the JSON as an array of RootData
            var rootDataArray = JsonUtility.FromJson<RootDataArrayWrapper>("{\"items\":" + json + "}");

            // Flatten all frames from all roots
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
        if (replayFrames.Count == 0)
        {
            Debug.LogError("No frames loaded. Cannot set up replay objects.");
            return;
        }

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

    private void StartReplaying()
    {
        replayTime = 0f;
        currentFrame = 0;
        Debug.Log("Replaying started.");
    }

    private void ReplayMovement()
    {
        replayTime += Time.deltaTime * replaySpeed;
        int newFrame = Mathf.FloorToInt(replayTime);

        if (newFrame != currentFrame && newFrame < replayFrames.Count)
        {
            currentFrame = newFrame;

            foreach (var objData in replayFrames[currentFrame].objects)
            {
                GameObject obj = replayObjects.Find(o => o.name == objData.name);

                if (obj != null)
                {
                    obj.transform.position = objData.position;
                    obj.transform.rotation = objData.rotation;
                    //Debug.Log("changing pos");
                }
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
        return currentFrame >= replayFrames.Count - 1;
    }
}
