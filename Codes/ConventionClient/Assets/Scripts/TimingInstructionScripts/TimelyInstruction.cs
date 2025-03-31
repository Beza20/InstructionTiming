using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class TimelyInstruction : MonoBehaviour
{
    [SerializeField] private OptiHandState userstate;
    [SerializeField] private FurnitureState furniturestate;

    private List<float> progressHistory = new List<float>();
    private List<float> effortHistory = new List<float>();
    private const int historyLimit = 200;
    private const float Idletimerlimit = 10;
    private const float negativetimerlimit = 3;
    private const float fluctimerLimit = 20;

    private float MiniTimer = 0;
    private float MixedTimer = 0;
    private float TransitionTimer = 0;

    public float filteredProgress = 0f;
    public float filteredEffort = 0f;

    private const float alpha = 0.02f; // Smoothing factor for low-pass filter

    private float timeElapsed = 0f;
    private bool startChecking = false;

    private State currentState = State.Positive;
    public delegate void InstructionTriggeredEvent(string state);
    public event InstructionTriggeredEvent OnInstructionTriggered;

    public enum State
    {
        Idle,
        EffortNoProgress,
        Negative,
        Fluctuating,
        Positive
    }

    // Toggles for enabling/disabling states (exposed in Inspector)
    [Header("State Toggles")]
    public bool checkIdle = true;
    public bool checkEffortNoProgress = true;
    public bool checkNegative = true;
    public bool checkFluctuating = true;
    public bool checkPositive = true;

    void Update()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed >= 5f) // Start delay
        {
            startChecking = true;
        }

        if (startChecking)
        {
            float effort = userstate.GetUserMovement();
            float progress = furniturestate.GetTaskProgress();

            filteredProgress = alpha * progress + (1 - alpha) * filteredProgress;
            filteredEffort = alpha * effort + (1 - alpha) * filteredEffort;

            progressHistory.Add(filteredProgress);
            effortHistory.Add(filteredEffort);

            if (progressHistory.Count > historyLimit) progressHistory.RemoveAt(0);
            if (effortHistory.Count > historyLimit) effortHistory.RemoveAt(0);

            float avgProgress = GetAverage(progressHistory);
            float avgEffort = GetAverage(effortHistory);

            UpdateState(avgProgress, filteredProgress, avgEffort, filteredEffort);
        }
    }

    private void UpdateState(float avgProgress, float progress, float avgEffort, float effort)
    {
        if (currentState != State.Positive)
        {
            MixedTimer += Time.deltaTime;
        }

        // Check each state only if its toggle is enabled
        if (checkIdle && Mathf.Abs(avgProgress - progress) < 0.01f && Mathf.Abs(avgEffort - effort) < 0.5f)
        {
            TransitionToState(State.Idle, progress);
            MiniTimer += Time.deltaTime;
            if (MiniTimer >= Idletimerlimit)
            {
                HandleStateActions();
            }
        }
        else if (checkEffortNoProgress && Mathf.Abs(avgProgress - progress) < 0.01f && Mathf.Abs(avgEffort - effort) >= 0.5f)
        {
            TransitionToState(State.EffortNoProgress, progress);
            MiniTimer += Time.deltaTime;
            if (MiniTimer >= (Idletimerlimit - 3))
            {
                HandleStateActions();
            }
        }
        else if (checkNegative && (progress - avgProgress) < -0.01f)
        {
            TransitionToState(State.Negative, progress);
            TransitionTimer += Time.deltaTime;
            if (TransitionTimer > negativetimerlimit)
            {
                HandleStateActions();
                TransitionTimer = 0;
            }
        }
        else if (checkFluctuating && MixedTimer > fluctimerLimit)
        {
            TransitionToState(State.Fluctuating, progress);
            HandleStateActions();
            MixedTimer = 0;
        }
        else if (checkPositive && (progress - avgProgress) > 0.01f)
        {
            TransitionToState(State.Positive, progress);
            MiniTimer = 0;
            MixedTimer = 0;
        }
    }

    private void TransitionToState(State newState, float progress)
    {
        if (currentState != newState)
        {
            currentState = newState;
            Debug.Log($"Transitioned to state: {newState}");
        }
    }

    private void HandleStateActions()
    {
        Debug.Log($"Handling actions for state: {currentState}");
        MiniTimer = 0;
        MixedTimer = 0;
        TransitionTimer = 0;

        // Trigger specific instructions based on the state
        ErrorBasedInstruction errorBasedInstruction = FindObjectOfType<ErrorBasedInstruction>();
        errorBasedInstruction?.HandleInstruction();
        // Find and trigger the AudioPlayer
        AudioPlayer audioPlayer = FindObjectOfType<AudioPlayer>();
        if (audioPlayer != null)
        {
            audioPlayer.PlayAudio();
        }
        else
        {
            Debug.LogWarning("No AudioPlayer found in the scene.");
        }

        // 🚀 Fire the event for `InstructionLogger` to capture
        OnInstructionTriggered?.Invoke(currentState.ToString());
    }

    private float GetAverage(List<float> values)
    {
        return values.Count > 0 ? values.Average() : 0f;
    }
}
