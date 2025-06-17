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
        while (true)
        {
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSecondsRealtime(waitTime);

            if (interviewManager != null)
            {
                Debug.Log($"🔔 Random interview triggered at {Time.time}");
                interviewManager.TriggerInterview(triggerLabel);
            }
        }
    }

    public void PauseTrigger()
    {
        if (triggerLoopCoroutine != null)
        {
            StopCoroutine(triggerLoopCoroutine);
            Debug.Log("RandomInterviewTrigger paused");
            triggerLoopCoroutine = null;
        }
    }

    public void ResumeTrigger()
    {
        if (triggerLoopCoroutine == null)
        {
            triggerLoopCoroutine = StartCoroutine(RandomTriggerLoop());
            Debug.Log("RandomInterviewTrigger resumed");
        }
    }

    public void ResetTrigger()
    {
        Debug.Log("Random reset doing nothing");
    }

}
