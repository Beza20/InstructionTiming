using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.Linq; 
public class NegativeProgress : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AdaptiveProgressFormulation progressScript;

    [Header("Monitoring Settings")]
    [SerializeField] private float checkInterval = 0.05f; // in seconds
    [SerializeField] private float durationThreshold = 5f; // how long without improvement before triggering

    [Header("Optional Feedback")]
    [SerializeField] private AudioSource warningBeep;
    [SerializeField] private bool logWarnings = true;
    [SerializeField] private TriggerManagerCoordinator interviewManager;
    [SerializeField] private string triggerLabel = "negative_progress";
    private Queue<float> progressHistory = new Queue<float>();
    [SerializeField] private int historyWindowSize = 5; // Number of samples (e.g., last 5 seconds)
    private float smoothedProgress = 0f;
    [SerializeField] private float alpha = 0.1f;
    public Slider progressBar7;
    public Slider progressBar8;
    private float filteredProgress = 0f;

    private float lastCheckedProgress = 0f;
    private float timeSinceImprovement = 0f;
    private float checkTimer = 0f;

    public event Action OnNegativeProgressTriggered; // Optional hook for other scripts

    void Update()
    {
        if (progressScript == null) return;

        checkTimer += Time.deltaTime;

        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;

            int activeGroup = progressScript.GetActiveGroup();
            if (activeGroup == -1) return;

            float currentProgress = progressScript.EvaluateGroupProgress(activeGroup);
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

            // Compare smoothed progress to previous smoothed progress
            if (averageProgress >= filteredProgress + 0.1f)
            {
                Debug.Log("negative detected");
                timeSinceImprovement += checkInterval;
            }
            else
            {
                timeSinceImprovement = 0f;
            }

           
            if (timeSinceImprovement >= durationThreshold)
            {
                TriggerNegativeProgressEvent();
                timeSinceImprovement = 0f;
            }
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

               
}
