using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class MovementTrigger : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private MovementTracker movementTracker;
    [SerializeField] private ObjectRotationTracker objectTracker;
   // [SerializeField] private AudioSource beepAudio;
    [SerializeField] private TriggerManagerCoordinator interviewManager;
   

    [Header("Timing Settings")]
    [SerializeField] private float requiredDuration = 2.0f;   // How long conditions must stay true
    [SerializeField] private float gracePeriod = 0.3f;      // Brief interruptions allowed
    [SerializeField] private float cooldownDuration = 3.0f; // Prevent rapid retriggering

    [SerializeField] private Transform _glassesTransform;
    
    // State tracking
    private bool _canTrigger = true;
    private float _conditionMetTime = 0f;
    private float _gracePeriodTimer = 0f;

    public TextMeshProUGUI head;
    public TextMeshProUGUI hands;

    void Update()
    {
        if (!_canTrigger) return;

        // Check conditions (simplified cumulative checks)
        bool isTriggerReady = 
            movementTracker.IsHeadMoving &&          // Cumulative head rotation > threshold
            movementTracker.AreHandsStill &&         // Hands barely moved
            objectTracker.AreObjectsStillA1(5);         // External objects static
        if(!objectTracker.AreObjectsStillA1(5))
        {
            Debug.Log("objectTracker is blocking");
        }
        if (movementTracker.IsHeadMoving)
        {
            head.text = "head is moving";
            head.color = Color.green;
        }
        if (!movementTracker.IsHeadMoving)
        {
            head.text = "head is not moving";
            head.color = Color.red;
        }
        if (!movementTracker.AreHandsStill)
        {
            hands.text = "hand is moving";
            hands.color = Color.red;
        }
        if (movementTracker.AreHandsStill)
        {
            hands.text = "hand is not moving";
            hands.color = Color.green;
        }

        if (isTriggerReady)
        {
            Debug.Log("trigger stays ready");
            HandleSuccessfulCondition();
        }
        else
        {
            HandleFailedCondition();
        }
    }

    // Called when ALL conditions are met
    private void HandleSuccessfulCondition()
    {
        _gracePeriodTimer = 0f; // Reset grace period
        _conditionMetTime += Time.deltaTime;

        if (_conditionMetTime >= requiredDuration)
        {
            Debug.Log("trigger is going off trust your beep is just tweaking");
            TriggerAction();
        }
    }

    // Called when ANY condition fails
    private void HandleFailedCondition()
    {
        // Only penalize if we were making progress
        if (_conditionMetTime > 0)
        {
            _gracePeriodTimer += Time.deltaTime;

            // If grace period expires, reset progress
            if (_gracePeriodTimer >= gracePeriod)
            {
                _conditionMetTime = 0f;
                _gracePeriodTimer = 0f;
            }
        }
    }

    // Execute the trigger (e.g., play sound)
    private void TriggerAction()
    {
        _canTrigger = false;
        //beepAudio.Play();
        interviewManager?.TriggerInterview("A1");
        Invoke(nameof(ResetTrigger), cooldownDuration);
    }

    // Re-enable triggering after cooldown
    private void ResetTrigger()
    {
        _canTrigger = true;
        _conditionMetTime = 0f;
        _gracePeriodTimer = 0f;
    }
}