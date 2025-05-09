using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SubtaskData
{
    public string SubtaskName;
    public float RelativeDistance;
    public Quaternion AngleDifference;
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
    [Tooltip("List of interchangeable piece groups (indices from SubtaskPiecesB)")]
    public List<int[]> InterchangeableGroups = new List<int[]>();
}