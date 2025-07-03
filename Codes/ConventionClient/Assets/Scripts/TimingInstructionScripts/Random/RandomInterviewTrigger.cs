using System.Collections;
using UnityEngine;

public class RandomInterviewTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TriggerManagerCoordinator interviewManager;

    [Header("Timing (seconds)")]
    [SerializeField] private float minInterval = 120f; // 2 minutes
    [SerializeField] private float maxInterval = 300f; // 5 minutes

    [Header("Trigger Label")]
    [SerializeField] private string triggerLabel = "random_prompt";
    private Coroutine triggerLoopCoroutine;
    private float remainingTime = -1f;
    private float targetTime;

    private void Start()
    {
        if (interviewManager == null)
        {
            Debug.LogWarning("InterviewManager reference is missing.");
            return;
        }

        triggerLoopCoroutine = StartCoroutine(RandomTriggerLoop());
    }

    private IEnumerator RandomTriggerLoop()
    {
        float waitTime = Random.Range(minInterval, maxInterval);
        targetTime = Time.realtimeSinceStartup + waitTime;

        while (Time.realtimeSinceStartup < targetTime)
        {
            yield return null;  // wait until resumed
        }

        if (interviewManager != null)
        {
            Debug.Log($"🔔 Random interview triggered at {Time.time}");
            interviewManager.TriggerInterview(triggerLabel);
        }

        // Restart the loop
        triggerLoopCoroutine = StartCoroutine(RandomTriggerLoop());
    }

    public void PauseTrigger()
    {
        if (triggerLoopCoroutine != null)
        {
            remainingTime = targetTime - Time.realtimeSinceStartup;
            StopCoroutine(triggerLoopCoroutine);
            triggerLoopCoroutine = null;
            Debug.Log($"⏸️ RandomInterviewTrigger paused with {remainingTime:F1}s remaining");
        }
    }

    public void ResumeTrigger()
    {
        
        if (triggerLoopCoroutine == null)
        {
            triggerLoopCoroutine = StartCoroutine(ResumeWithRemainingTime());
            Debug.Log("▶️ RandomInterviewTrigger resumed");
        }
        
    }

    private IEnumerator ResumeWithRemainingTime()
    {
        targetTime = Time.realtimeSinceStartup + Mathf.Max(remainingTime, 0f);

        while (Time.realtimeSinceStartup < targetTime)
        {
            yield return null;
        }

        if (interviewManager != null)
        {
            Debug.Log($"🔔 Random interview triggered at {Time.time}");
            interviewManager.TriggerInterview(triggerLabel);
        }

        triggerLoopCoroutine = StartCoroutine(RandomTriggerLoop());
    }

    public void ResetTrigger()
    {
        Debug.Log("Random reset doing nothing");
    }

}
