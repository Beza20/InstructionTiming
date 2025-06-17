using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine.UI;

public class Pause : MonoBehaviour
{
    [Header("References")]

    [SerializeField] private HandGrabbingMonitor grabbingMonitor;
    [SerializeField] private ConecastHandling conecast;
    [SerializeField] private ObjectRotationTracker rotationTracker;
    [SerializeField] private TriggerManagerCoordinator interviewManager;
    public TextMeshProUGUI visiblOBj;
    public TextMeshProUGUI grabbedOBj;
    public TextMeshProUGUI grabbedandVisbil;
    public AudioSource questionAudioSource;
    public AudioClip questionPAudio;

    [Header("UI Buttons")]
    public Button yesButton;
    public Button noButton;
    private bool sth_moving = false;





    [Header("Trigger Settings")]
    [SerializeField] private float freezeThreshold = 3f;

    private float cooldown_neg = 8;
    private float last_trigger = 0;

    //public UnityEvent OnHesitationDetected;

    private Dictionary<GameObject, float> freezeTimers = new();

    private bool awaitingResponse = false;
    private string responseQ1 = "";
    private Transform ActiveHead => interviewManager != null ? interviewManager.ActiveHead : null;

    void Start()
    {
        yesButton.onClick.AddListener(() => OnAnswer("Yes"));
        noButton.onClick.AddListener(() => OnAnswer("No"));

        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);
    }

   

    void Update()
    {
        last_trigger += Time.deltaTime;
        HashSet<GameObject> inView = conecast.GetObjectsInSight(ActiveHead);
        List<GameObject> grabbedObjects = new List<GameObject>();
        grabbedObjects.AddRange(grabbingMonitor.grabbedByLeftHand);
        grabbedObjects.AddRange(grabbingMonitor.grabbedByRightHand);
        visiblOBj.text = "";
        grabbedOBj.text = "";
        grabbedandVisbil.text = "";
        List<GameObject> tracked_objects = rotationTracker.trackedObjects;
        sth_moving = false;
        foreach (GameObject obj in tracked_objects)
        {
            if (rotationTracker.IsObjectMoving(obj) && (obj.name != "glass1" || obj.name != "glass2" ))
            {
                Debug.Log("sth is moving so no pause");
                sth_moving = true;
            }
        }
        if (sth_moving)
        {
            return;
        }
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

                if (!awaitingResponse && freezeTimers[obj] >= freezeThreshold && (last_trigger > cooldown_neg))
                {
                    Debug.LogWarning($"⏱️ Hesitation detected on: {obj.name}");
                    awaitingResponse = true;
                    AskQuestion(questionPAudio);
                    freezeTimers[obj] = 0f;
                    last_trigger = 0;
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

    private void AskQuestion(AudioClip clip)
    {
        questionAudioSource.clip = clip;
        questionAudioSource.Play();

        yesButton.gameObject.SetActive(true);
        noButton.gameObject.SetActive(true);
    }

    private void OnAnswer(string answer)
    {
        responseQ1 = answer;
        awaitingResponse = false;

        questionAudioSource.Stop();
        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);

        if (answer == "No")
        {
            yesButton.gameObject.SetActive(false);
            noButton.gameObject.SetActive(false);
            interviewManager?.TriggerInterview("pause");

        }
        if (answer == "Yes")
        {
            yesButton.gameObject.SetActive(false);
            noButton.gameObject.SetActive(false);

        }

    }
    public void ResetTrigger()
    {
        Debug.Log("Resetting pause");
        last_trigger = 0;
    }

    
    
}
