using UnityEngine;

public class TriggerInstructionPlayer : MonoBehaviour
{
    public AdaptiveProgressFormulation progressScript;
    public AudioSource audioSource;

    public AudioClip[] baseInstructions;      // 4 clips for each group
    public AudioClip[] detailedInstructions;  // 4 clips for each group

    private const int NumGroups = 4;
    private int[] triggerCounts = new int[NumGroups]; // Tracks how many times an instruction was triggered per group

    public void OnTriggerEvent()
    {
        int group = progressScript.GetActiveGroup();
        if (group == -1)
        {
            group = 0;
        }
        if (group < 0 || group >= NumGroups) return;

        // Determine which clip to play
        AudioClip clipToPlay;

        if (triggerCounts[group] == 0)
        {
            // First time — play base
            clipToPlay = baseInstructions[group];
            triggerCounts[group]++;
        }
        else
        {

            // Already gave base instruction
            bool anySubtaskComplete = progressScript.IsSubtaskComplete(group);
            if (anySubtaskComplete && triggerCounts[group] == 1)
            {
                Debug.Log("could it be");
                clipToPlay = baseInstructions[group]; // replay base
                triggerCounts[group]++;
            }
            else
            {
                Debug.Log("doing this");
                clipToPlay = detailedInstructions[group]; // escalate
            }
        }

        if (!audioSource.isPlaying && clipToPlay != null)
        {
            Debug.Log($"Playing {(triggerCounts[group] > 1 ? "detailed" : "base")} instruction for group {group}");
            audioSource.clip = clipToPlay;
            audioSource.Play();
        }
    }
}
