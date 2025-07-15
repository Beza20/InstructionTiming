using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using TMPro;

public class GrabIncorrectPiece : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AdaptiveProgressFormulation progressScript;
    [SerializeField] private HandGrabbingMonitor grabbingMonitor;
    [SerializeField] private TriggerManagerCoordinator interviewManager;
    public ObjectRotationTracker movementTracker;

    [Header("Trigger Timing")]
    [SerializeField] private float sustainThreshold = 3f;    // how long an incorrect piece must be held
    [SerializeField] private float triggerCooldown = 7f;     // delay between triggers

    private float lastTriggerTime = -999f;
    private Dictionary<GameObject, float> grabTimers = new(); // tracks how long each incorrect object is held
    public TextMeshProUGUI grabbedOBj;
    public TextMeshProUGUI currentlyheld;
    private float startDelay = 15f;
    private float elapsedTime = 0f;
    private bool started = false;



    void Update()
    {
        if (!started)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= startDelay)
            {
                started = true;
            }
            else
            {
                return; // wait until 6 seconds have passed
            }
        }
        if (interviewManager == null || progressScript == null || grabbingMonitor == null) return;
        grabbedOBj.text = "";
        currentlyheld.text = "";
        bool triggered = false;
        float now = Time.time;

        int activeGroup = progressScript.GetActiveGroup();
        if (activeGroup == -1) return;
        Debug.Log(activeGroup + " is the active group");

        

        List<int> activeSubtasks = new List<int>();

        if (activeGroup == 3)
        {
            int currentGrp3task = progressScript.currentGrp3Subtask();
            if (currentGrp3task != -1)
            {
                activeSubtasks.Add(currentGrp3task);
                activeSubtasks.AddRange(progressScript.GetGroupedSubtasks()[0]);
                activeSubtasks.AddRange(progressScript.GetGroupedSubtasks()[1]);
                activeSubtasks.AddRange(progressScript.GetGroupedSubtasks()[2]);

            }
        }
        else
        {
            activeSubtasks = progressScript.GetGroupedSubtasks()[activeGroup];
        }

        var piecesA = progressScript.GetSubtaskPiecesA();
        var piecesB = progressScript.GetSubtaskPiecesB();

        HashSet<GameObject> expectedObjects = new HashSet<GameObject>();
        foreach (int i in activeSubtasks)
        {
            expectedObjects.Add(piecesA[i]);
            expectedObjects.Add(piecesB[i]);
        }
        foreach (GameObject obj in expectedObjects)
        {
            if (obj == null)
            {
                Debug.Log("no expected objs");
                grabbedOBj.text += "nothing";
            }
            else
            {
                grabbedOBj.text += activeGroup + obj.name + "\n";
            }
        }

        var leftGrabbed = grabbingMonitor.grabbedByLeftHand;
        var rightGrabbed = grabbingMonitor.grabbedByRightHand;

       

        // Check incorrect grabs
        List<GameObject> currentlyHeld = new List<GameObject>();
        currentlyHeld.AddRange(leftGrabbed);
        currentlyHeld.AddRange(rightGrabbed);

        foreach (GameObject obj in currentlyHeld)
        {
            if (obj == null)
            {
                Debug.Log("no expected objs");
                currentlyheld.text += "nothing";
            }
            else
            {
                currentlyheld.text += obj.name + "\n";
            }
        }

        foreach (var obj in currentlyHeld)
        {
            if (obj == null || expectedObjects.Contains(obj.transform.root.gameObject)) continue;
            currentlyheld.text += obj.name + "is not expected \n";


            bool isLeft = leftGrabbed.Contains(obj);
            bool isRight = rightGrabbed.Contains(obj);

            GameObject hand = isLeft ? grabbingMonitor.leftHandRigidbody.gameObject :
                            isRight ? grabbingMonitor.rightHandRigidbody.gameObject : null;
            if (IsMovingTogether(grabbingMonitor.leftHandRigidbody.gameObject, obj))
            {
                currentlyheld.text += "moving: " + obj.name + "\n";
            }
            if (IsMovingTogether(grabbingMonitor.rightHandRigidbody.gameObject, obj))
            {
                currentlyheld.text += "moving: " + obj.name + "\n";
            }

            if (hand == null || !IsMovingTogether(hand, obj)) continue;

            if (!grabTimers.ContainsKey(obj))
                grabTimers[obj] = now;

            float heldDuration = now - grabTimers[obj];
            if (heldDuration >= sustainThreshold)
            {
                Debug.Log($"Sustained incorrect grab: {obj.name} for {heldDuration:F1}s with moving hand and object");
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
            GameObject leftObj = leftGrabbed.Count > 0 ? leftGrabbed[0] : null;
            GameObject rightObj = rightGrabbed.Count > 0 ? rightGrabbed[0] : null;

            if (IsMovingTogether(grabbingMonitor.leftHandRigidbody.gameObject, leftObj) &&
                IsMovingTogether(grabbingMonitor.rightHandRigidbody.gameObject, rightObj))
            {
                Debug.Log("Both hands are grabbing different objects from SubtaskPiecesA and are moving with them — coordination issue." );
                triggered = true;
            }
        }

        if (leftIsB && rightIsB && !IsGrabbingSameObject(leftGrabbed, rightGrabbed))
        {
            GameObject leftObj = leftGrabbed.Count > 0 ? leftGrabbed[0] : null;
            GameObject rightObj = rightGrabbed.Count > 0 ? rightGrabbed[0] : null;

            if (IsMovingTogether(grabbingMonitor.leftHandRigidbody.gameObject, leftObj) &&
                IsMovingTogether(grabbingMonitor.rightHandRigidbody.gameObject, rightObj))
            {
                Debug.Log("Both hands are grabbing different objects from SubtaskPiecesB and are moving with them — coordination issue.");
                triggered = true;
            }
        }

        // Final trigger
        if (triggered && Time.time - lastTriggerTime > triggerCooldown)
        {
            interviewManager?.TriggerInterview("B2-grab-incorrectpiece");
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

    private bool IsMovingTogether(GameObject hand, GameObject obj)
    {
        if (hand == null || obj == null || movementTracker == null)
        {
            Debug.LogWarning("IsMovingTogether: Missing reference");
            return false;
        }

        // Get the root objects (like AdaptiveProgressFormulation does)
        GameObject rootHand = hand.transform.root.gameObject;
        GameObject rootObj = obj.transform.root.gameObject;

        bool handMoving = movementTracker.IsObjectMoving(rootHand);
        bool objMoving = movementTracker.IsObjectMoving(rootObj);

        Debug.Log($"IsMovingTogether - " +
                $"Hand '{rootHand.name}' moving: {handMoving}, " +
                $"Object '{rootObj.name}' moving: {objMoving}");

        return objMoving;
    }
    public void ResetTrigger()
    {
       Debug.Log("Resetting B2");
       lastTriggerTime = Time.time; 
    }
}
