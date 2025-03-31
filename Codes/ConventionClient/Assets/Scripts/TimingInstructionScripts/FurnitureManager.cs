using System.Collections.Generic;
using UnityEngine;

public class FurnitureManager : MonoBehaviour
{
    [System.Serializable]
    public class Furniture
    {
        public string name; // Name of the furniture piece
        public List<GameObject> associatedObjects; // List of GameObjects linked to this furniture
    }

    [System.Serializable]
    public enum DetectionMode
    {
        NegativeProgress,
        EffortWithoutProgress,
        CustomMode1
    }

    public List<Furniture> furniturePieces; // List of all furniture pieces
    public List<DetectionMode> activeModes = new List<DetectionMode>(); // List of active detection modes

    [Tooltip("Select the active furniture piece from the dropdown.")]
    [SerializeField]
    private int selectedFurnitureIndex = -1; // Index of the currently selected furniture

    [Header("Recorder Settings")]
    public MovementRecorderReplayer movementRecorder; // Reference to the movement recorder
    public IdealStateRecorder idealStateRecorder;

    public FurnitureState furnitureState; // Reference to FurnitureState script
    public bool isRecording = false; // Flag to indicate whether recording is active
    private List<GameObject> activeObjects = new List<GameObject>(); // List of currently active objects

    private void OnValidate()
    {
        HandleFurnitureSelection(); // Handles selection updates in the Inspector
        NotifyRecorder(); // Ensures the recorder is notified of changes
    }

    private void Update()
    {
        if (movementRecorder != null)
        {
            if (isRecording)
            {
                // During recording, update recorder's frame
                movementRecorder.RecordFrame();
            }
            else
            {
                // If recording is not active, clear the recorder's tracking list
                movementRecorder.objectsToTrack = new List<GameObject>();
            }
        }
    }

    /// <summary>
    /// Handles furniture selection changes.
    /// </summary>
    private void HandleFurnitureSelection()
    {
        if (selectedFurnitureIndex < 0 || selectedFurnitureIndex >= furniturePieces.Count)
        {
            DeselectAllFurniture();
            Debug.Log("No furniture selected.");
            NotifyRecorder(); // Notify recorder about the change
            return;
        }

        // Activate selected furniture and deactivate others
        for (int i = 0; i < furniturePieces.Count; i++)
        {
            bool isActive = i == selectedFurnitureIndex;
            SetFurnitureActive(furniturePieces[i], isActive);
        }

        NotifyRecorder(); // Notify recorder about the updated active objects
    }

    /// <summary>
    /// Deactivates all furniture pieces.
    /// </summary>
    private void DeselectAllFurniture()
    {
        foreach (var furniture in furniturePieces)
        {
            SetFurnitureActive(furniture, false);
        }

        activeObjects.Clear();
        NotifyRecorder(); // Notify recorder about the change
    }

    /// <summary>
    /// Activates or deactivates the associated objects of a furniture piece.
    /// </summary>
    private void SetFurnitureActive(Furniture furniture, bool isActive)
    {
        foreach (var obj in furniture.associatedObjects)
        {
            if (obj != null)
            {
                obj.SetActive(isActive);

                if (isActive && !activeObjects.Contains(obj))
                {
                    activeObjects.Add(obj); // Add to active list
                }
                else if (!isActive && activeObjects.Contains(obj))
                {
                    activeObjects.Remove(obj); // Remove from active list
                }
            }
        }
        string activeFurnitureName = selectedFurnitureIndex >= 0 && selectedFurnitureIndex < furniturePieces.Count
        ? furniturePieces[selectedFurnitureIndex].name
        : null;

        if (furnitureState != null)
        {
            furnitureState.SetActiveFurniture(activeFurnitureName); // Notify FurnitureState
            Debug.Log($"FurnitureManager: Active furniture set to {activeFurnitureName}");
        }
    }

    /// <summary>
    /// Notifies the movement recorder about active objects and starts/stops recording sessions.
    /// </summary>
    private void NotifyRecorder()
    {
        if (movementRecorder == null)
        {
            Debug.LogWarning("Movement recorder reference not set.");
            return;
        }
        if (idealStateRecorder == null)
        {
            Debug.LogWarning("IdealStateRecorder reference is not set in FurnitureManager.");
            return;
        }
        string activeFurnitureName = selectedFurnitureIndex >= 0 && selectedFurnitureIndex < furniturePieces.Count
        ? furniturePieces[selectedFurnitureIndex].name
        : null;

        if (!string.IsNullOrEmpty(activeFurnitureName))
        {
            idealStateRecorder.SetActiveFurniture(activeFurnitureName);
            Debug.Log($"IdealStateRecorder notified with active furniture: {activeFurnitureName}");
        }

        if (isRecording)
        {
            // Update recorder with active objects and start session with selected furniture name
            movementRecorder.objectsToTrack = new List<GameObject>(activeObjects);

            string selectedFurnitureName = selectedFurnitureIndex >= 0 && selectedFurnitureIndex < furniturePieces.Count
                ? furniturePieces[selectedFurnitureIndex].name
                : "NoFurnitureSelected";

            movementRecorder.StartRecordingSession(selectedFurnitureName);
            //Debug.Log($"Recording started for: {selectedFurnitureName}");
        }
        else
        {
            // Clear recorder's tracking list if recording is disabled
            movementRecorder.objectsToTrack = new List<GameObject>();
            movementRecorder.EndRecordingSession();
            Debug.Log("Recording stopped. Recorder tracking list cleared.");
        }
    }

    /// <summary>
    /// Toggles a specific detection mode.
    /// </summary>
    public void ToggleDetectionMode(DetectionMode mode, bool isEnabled)
    {
        if (isEnabled && !activeModes.Contains(mode))
        {
            activeModes.Add(mode);
            Debug.Log($"Enabled detection mode: {mode}");
        }
        else if (!isEnabled && activeModes.Contains(mode))
        {
            activeModes.Remove(mode);
            Debug.Log($"Disabled detection mode: {mode}");
        }
    }

    /// <summary>
    /// Checks if a detection mode is active.
    /// </summary>
    public bool IsModeActive(DetectionMode mode)
    {
        return activeModes.Contains(mode);
    }
}
