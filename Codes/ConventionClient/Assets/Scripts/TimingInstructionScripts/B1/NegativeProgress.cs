using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.Events;
using System.IO;
using System.Text;
public class NegativeProgress : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AdaptiveProgressFormulation progressScript;

    [Header("Monitoring Settings")]
    [SerializeField] private float checkInterval = 0.005f; // in seconds
    [SerializeField] private float durationThreshold = 0.1f; // how long without improvement before triggering

    [Header("Optional Feedback")]
    [SerializeField] private AudioSource warningBeep;
    [SerializeField] private bool logWarnings = true;
    [SerializeField] private TriggerManagerCoordinator interviewManager;
    [SerializeField] private string triggerLabel = "negative_progress";
    private Queue<float> progressHistory = new Queue<float>();
    [SerializeField] private int historyWindowSize = 180; // Number of samples (last 3 seconds)
    private float smoothedProgress = 0f;
    [SerializeField] private float alpha = 0.1f;
    public Slider progressBar7;
    public Slider progressBar8;
    private float filteredProgress = 0f;
    private bool awaitingResponse = false;
    private string responseQ1 = "";

    private float lastCheckedProgress = 0f;
    private float timeSinceImprovement = 0f;
    private float checkTimer = 0f;
    private float cooldown_neg = 8;
    private float last_trigger = 0;
    [Header("UI Buttons")]
    public Button yesButton;
    public Button noButton;

    public event Action OnNegativeProgressTriggered; // Optional hook for other scripts
    public AudioSource questionAudioSource;
    public AudioClip questionPAudio;

    void Start()
    {
        yesButton.onClick.AddListener(() => OnAnswer("Yes"));
        noButton.onClick.AddListener(() => OnAnswer("No"));

        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);
    }

    void Update()
    {
        if (progressScript == null) return;

        checkTimer += Time.deltaTime;
        last_trigger += Time.deltaTime;


        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;

            int activeGroup = progressScript.GetActiveGroup();
            if (activeGroup == -1)
            {
                Debug.Log("stuck cuz active grp is -1");
                return;
            }

            Debug.Log("passing because active grp for neg prgress is " + activeGroup);

            float currentProgress = progressScript.EvaluateGroupProgress(activeGroup);
            // Debug.Log("cureent progresss " + currentProgress);
            filteredProgress = alpha * currentProgress + (1 - alpha) * filteredProgress;

            // Add to history
            progressHistory.Enqueue(filteredProgress);
            if (progressHistory.Count > historyWindowSize)
                progressHistory.Dequeue();

            // Compute moving average
            float sum = 0f;
            foreach (var p in progressHistory)
                sum += p;

            float averageProgress = sum / progressHistory.Count;
            progressBar7.value = averageProgress;
            progressBar8.value = filteredProgress;

            // Debug.Log("VERAGE progresss " + averageProgress);
            // Debug.Log("filter progresss " + filteredProgress);



            float epsilon = 0.00005f;  // small tolerance
            float? lastVal = null;
            int downticks = 0;

            foreach (var p in progressHistory)
            {
                if (lastVal.HasValue && p < lastVal.Value - epsilon)
                {
                    downticks++;
                }
                lastVal = p;
            }

            float downtickRatio = (float)downticks / (progressHistory.Count - 1);  // -1 to skip first

            //Debug.Log($"📊 Downticks: {downticks} / {progressHistory.Count - 1} = {downtickRatio:P1}");
            float delta = progressHistory.Peek() - progressHistory.Last();


            if (!awaitingResponse &&
                downtickRatio >= 0.75f &&  // <-- you can tune this
                delta >= 0.1f &&
                last_trigger > cooldown_neg)
            {
                //Debug.Log($"📉 Graceful decline detected. Δ={delta:F3}, downtickRatio = {downtickRatio:P1}, Triggering.");
                last_trigger = 0;
                AskQuestion(questionPAudio);
                timeSinceImprovement = 0f;
            }

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
            TriggerNegativeProgressEvent();

        }
        if (answer == "Yes")
        {
            yesButton.gameObject.SetActive(false);
            noButton.gameObject.SetActive(false);

        }

    }


    private void TriggerNegativeProgressEvent()
    {

        if (logWarnings)
            Debug.Log("Negative or stagnant progress detected!");

        if (interviewManager != null)
            interviewManager.TriggerInterview(triggerLabel);
        else
            Debug.Log("InterviewManager is not assigned!");
    }
    
    public void ResetTrigger()
    {
        Debug.Log("Resetting B1");
        last_trigger = 0;
       
        
       
       
    }

               
}
