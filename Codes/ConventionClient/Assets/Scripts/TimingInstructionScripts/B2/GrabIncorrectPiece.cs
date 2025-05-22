using System.Collections.Generic;
using UnityEngine;

public class GrabIncorrectPiece : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AdaptiveProgressFormulation progressScript;
    [SerializeField] private HandGrabbingMonitor grabbingMonitor;
    [SerializeField] private TriggerManagerCoordinator interviewManager;

    [Header("Trigger Timing")]
    [SerializeField] private float sustainThreshold = 3f;    // how long an incorrect piece must be held
    [SerializeField] private float triggerCooldown = 7f;     // delay between triggers

    private float lastTriggerTime = -999f;
    private Dictionary<GameObject, float> grabTimers = new(); // tracks how long each incorrect object is held

    void Update()
    {
        if (interviewManager == null || progressScript == null || grabbingMonitor == null) return;

        int activeGroup = progressScript.GetActiveGroup();
        if (activeGroup == -1) return;

        List<int> activeSubtasks = progressScript.GetGroupedSubtasks()[activeGroup];
        var piecesA = progressScript.GetSubtaskPiecesA();
        var piecesB = progressScript.GetSubtaskPiecesB();

        HashSet<GameObject> expectedObjects = new HashSet<GameObject>();
        foreach (int i in activeSubtasks)
        {
            expectedObjects.Add(piecesA[i]);
            expectedObjects.Add(piecesB[i]);
        }

        var leftGrabbed = grabbingMonitor.grabbedByLeftHand;
        var rightGrabbed = grabbingMonitor.grabbedByRightHand;

        bool triggered = false;
        float now = Time.time;

        // Check incorrect grabs
        List<GameObject> currentlyHeld = new List<GameObject>();
        currentlyHeld.AddRange(leftGrabbed);
        currentlyHeld.AddRange(rightGrabbed);

        foreach (var obj in currentlyHeld)
        {
            if (obj == null || expectedObjects.Contains(obj.transform.root.gameObject)) continue;

            // Sustain tracking
            if (!grabTimers.ContainsKey(obj))
                grabTimers[obj] = now;

            float heldDuration = now - grabTimers[obj];
            if (heldDuration >= sustainThreshold)
            {
                Debug.Log($"Sustained incorrect grab: {obj.name} for {heldDuration:F1}s");
                triggered = true;
            }
        }

        // Reset timers for released objects
        List<GameObject> keys = new List<GameObject>(grabTimers.Keys);
        foreach (var key in keys)
        {
            if (!currentlyHeld.Contains(key))
                grabTimers.Remove(key);
        }

        HashSet<GameObject> activeGroupPiecesA = new HashSet<GameObject>();
        HashSet<GameObject> activeGroupPiecesB = new HashSet<GameObject>();
        foreach (int i in activeSubtasks)
        {
            activeGroupPiecesA.Add(piecesA[i]);
            activeGroupPiecesB.Add(piecesB[i]);
        }

        bool leftIsA = leftGrabbed.Exists(obj => activeGroupPiecesA.Contains(obj.transform.root.gameObject));
        bool rightIsA = rightGrabbed.Exists(obj => activeGroupPiecesA.Contains(obj.transform.root.gameObject));
        bool leftIsB = leftGrabbed.Exists(obj => activeGroupPiecesB.Contains(obj.transform.root.gameObject));
        bool rightIsB = rightGrabbed.Exists(obj => activeGroupPiecesB.Contains(obj.transform.root.gameObject));

        if (leftIsA && rightIsA && !IsGrabbingSameObject(leftGrabbed, rightGrabbed))
        {
            Debug.Log("Both hands are grabbing different objects from SubtaskPiecesA of the active group — possible coordination issue.");
            triggered = true;
        }

        if (leftIsB && rightIsB && !IsGrabbingSameObject(leftGrabbed, rightGrabbed))
        {
            Debug.Log("Both hands are grabbing different objects from SubtaskPiecesB — possible incorrect coordination.");
            triggered = true;
        }

        // Final trigger
        if (triggered && Time.time - lastTriggerTime > triggerCooldown)
        {
            interviewManager?.TriggerInterview("B2");
            lastTriggerTime = now;
        }
    }
    
    private bool IsGrabbingSameObject(List<GameObject> left, List<GameObject> right)
    {
        foreach (var l in left)
        {
            foreach (var r in right)
            {
                if (l.transform.root.gameObject == r.transform.root.gameObject)
                    return true;
            }
        }
        return false;
    }
}
