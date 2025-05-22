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

    private void Start()
    {
        if (interviewManager == null)
        {
            Debug.LogWarning("InterviewManager reference is missing.");
            return;
        }

        StartCoroutine(RandomTriggerLoop());
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
}
