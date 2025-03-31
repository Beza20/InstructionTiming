using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[System.Serializable]
class ObjectState {
    public Vector3 LastObserved;
    public string TimeStamp;
    public float Offset;
    public string name;
}

[System.Serializable]
class ObjectStateList {
    public List<ObjectState> States = new List<ObjectState>();
}

public class FilteringTracking : MonoBehaviour
{
    [SerializeField] string object_name;
    ObjectState CurrentState = new ObjectState();
    Vector3 prev;

    // Start is called before the first frame update
    void Start()
    {
        CurrentState.LastObserved = this.transform.position;
        CurrentState.name = object_name;
    }

    // Update is called once per frame
    void Update()
    {
        CurrentState.Offset = Vector3.Distance(CurrentState.LastObserved, this.transform.position);
        if (CurrentState.Offset > 0.75f) {
            Int32 unixTimestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            CurrentState.TimeStamp = unixTimestamp.ToString();

            // Load existing states from file
            string fileName = object_name + ".json";
            string filePath = Application.persistentDataPath + "/" + fileName;
            ObjectStateList stateList = LoadStatesFromFile(filePath);

            // Add the current state to the list
            stateList.States.Add(CurrentState);

            // Save the updated list back to the file
            SaveStatesToFile(filePath, stateList);

            // Reset position to last observed
            this.transform.position = CurrentState.LastObserved;
        }
    }

    ObjectStateList LoadStatesFromFile(string filePath) {
        ObjectStateList stateList = new ObjectStateList();

        if (File.Exists(filePath)) {
            string json = File.ReadAllText(filePath);
            stateList = JsonUtility.FromJson<ObjectStateList>(json);
        }

        return stateList;
    }

    void SaveStatesToFile(string filePath, ObjectStateList stateList) {
        string json = JsonUtility.ToJson(stateList, true);
        File.WriteAllText(filePath, json);
    }
}
