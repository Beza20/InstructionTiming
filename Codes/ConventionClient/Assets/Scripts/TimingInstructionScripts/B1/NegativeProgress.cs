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
    [SerializeField] private float checkInterval = 10; // in seconds
    [SerializeField] private float durationThreshold = 0.1f; // how long without improvement before triggering

    private class TimedProgress
    {
        public float progress;
        public float timestamp;

        public TimedProgress(float progress, float timestamp)
        {
            this.progress = progress;
            this.timestamp = timestamp;
        }
    }


    [Header("Optional Feedback")]
    [SerializeField] private AudioSource warningBeep;
    [SerializeField] private bool logWarnings = true;
    [SerializeField] private TriggerManagerCoordinator interviewManager;
    [SerializeField] private string triggerLabel = "negative_progress";
    private Queue<TimedProgress> progressHistory = new Queue<TimedProgress>();
    [SerializeField] private float historyDurationSeconds = 3f;  // sliding window duration
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
    private float cooldown_neg = 5;
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
            float now = Time.time;

            int activeGroup = progressScript.GetActiveGroup();
            if (activeGroup == -1)
            {
                //Debug.Log("stuck cuz active grp is -1");
                return;
            }

            Debug.Log("passing because active grp for neg progress is " + activeGroup);

            float currentProgress = progressScript.EvaluateGroupProgress(activeGroup);
            filteredProgress = alpha * currentProgress + (1 - alpha) * filteredProgress;

            // Add current progress with timestamp to the queue
            progressHistory.Enqueue(new TimedProgress(filteredProgress, now));

            // Trim old entries
            while (progressHistory.Count > 0 && now - progressHistory.Peek().timestamp > historyDurationSeconds)
            {
                progressHistory.Dequeue();
            }

            // Analyze progress history
            const float epsilon = 0.00005f;
            float sum = 0f;
            TimedProgress lastVal = null;
            int downticks = 0;

            foreach (var entry in progressHistory)
            {
                sum += entry.progress;

                if (lastVal != null && entry.progress < lastVal.progress - epsilon)
                {
                    downticks++;
                }

                lastVal = entry;
            }

            float averageProgress = progressHistory.Count > 0 ? sum / progressHistory.Count : 0f;
            float downtickRatio = (progressHistory.Count > 1) ? (float)downticks / (progressHistory.Count - 1) : 0f;
            float delta = progressHistory.Peek().progress - progressHistory.Last().progress;

            // Update UI
            progressBar7.value = averageProgress;
            progressBar8.value = filteredProgress;

            // Trigger condition
            if (!awaitingResponse &&
                downtickRatio >= 0.85f &&
                delta >= 0.1f &&
                last_trigger > cooldown_neg)
            {
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
