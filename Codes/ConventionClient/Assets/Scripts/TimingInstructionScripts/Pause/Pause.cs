using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO;
using System.Text;
using TMPro;

public class Pause : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform head1;
    [SerializeField] private Transform head2;
    [SerializeField] private bool useHead2 = false;
    [SerializeField] private HandGrabbingMonitor grabbingMonitor;
    [SerializeField] private ConecastHandling conecast;
    [SerializeField] private ObjectRotationTracker rotationTracker;
    [SerializeField] private TriggerManagerCoordinator interviewManager;
    public TextMeshProUGUI visiblOBj;
    public TextMeshProUGUI grabbedOBj;
    public TextMeshProUGUI grabbedandVisbil;





    [Header("Trigger Settings")]
    [SerializeField] private float freezeThreshold = 3f;

    //public UnityEvent OnHesitationDetected;

    private Dictionary<GameObject, float> freezeTimers = new();

    public void SetUseHead2(bool value)
    {
        useHead2 = value;
    }

    private Transform ActiveHead => useHead2 ? head2 : head1;

    void Update()
    {
        HashSet<GameObject> inView = conecast.GetObjectsInSight(ActiveHead);
        List<GameObject> grabbedObjects = new List<GameObject>();
        grabbedObjects.AddRange(grabbingMonitor.grabbedByLeftHand);
        grabbedObjects.AddRange(grabbingMonitor.grabbedByRightHand);
        visiblOBj.text = "";
        grabbedOBj.text = "";
        grabbedandVisbil.text = "";
        foreach (GameObject obj in grabbedObjects)
        {
            grabbedOBj.text += obj.name + "\n";

        }
        foreach (GameObject obj in inView)
        {
            visiblOBj.text += obj.name + "\n";

        }




        foreach (GameObject obj in grabbedObjects)
        {
            GameObject rootObj = obj.transform.root.gameObject;
            if (!inView.Contains(rootObj)) continue;

            if (obj == null) continue;
            

            grabbedandVisbil.text += obj.name + "\n";

            bool isMoving = rotationTracker.IsObjectMoving(obj);

            if (!isMoving)
            {
                if (!freezeTimers.ContainsKey(obj))
                    freezeTimers[obj] = 0f;

                freezeTimers[obj] += Time.deltaTime;

                if (freezeTimers[obj] >= freezeThreshold)
                {
                    Debug.LogWarning($"⏱️ Hesitation detected on: {obj.name}");
                    interviewManager?.TriggerInterview("pause");

                    freezeTimers[obj] = 0f; // Reset to prevent repeat
                }
            }
            else
            {
                freezeTimers[obj] = 0f; // reset if motion resumes
            }
        }

        // Clean up timers for ungrabbed objects
        foreach (var key in new List<GameObject>(freezeTimers.Keys))
        {
            if (!grabbedObjects.Contains(key))
                freezeTimers.Remove(key);
        }
    }

    
    
}
