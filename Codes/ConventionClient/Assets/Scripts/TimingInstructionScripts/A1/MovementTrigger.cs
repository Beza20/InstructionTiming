using UnityEngine;

public class MovementTrigger : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private MovementTracker movementTracker;
    [SerializeField] private ObjectRotationTracker objectTracker;
    [SerializeField] private AudioSource beepAudio;

    [Header("Thresholds")]
    public float headRotationThreshold = 5f;    // Degrees in 0.2s window
    public float handVelocityThreshold = 0.05f; // m/s threshold
    public float requiredDuration = 2f;        // Seconds of continuous movement

    [Header("Tolerance")]
    public float gracePeriod = 0.3f;          // Allow brief interruptions
    public float cooldownDuration = 1f;       // Prevent rapid retriggering

    private bool canTrigger = true;
    private float conditionMetTime = 0f;
    private float gracePeriodTimer = 0f;
    private bool wasConditionMet = false;

    void Update()
    {
        if (!canTrigger) return;

        // Get smoothed values
        float headRotation = movementTracker.GetHeadRotationDifference();
        float leftHandVel = movementTracker.GetLeftHandVelocity();
        float rightHandVel = movementTracker.GetRightHandVelocity();
        
        // Check current conditions
        bool headMoving = headRotation > headRotationThreshold;
        bool handsStill = (leftHandVel < handVelocityThreshold) && 
                         (rightHandVel < handVelocityThreshold);
        bool objectsStill = objectTracker.AreObjectsStill(requiredDuration);
        bool conditionsMet = headMoving && handsStill && objectsStill;

        // if(headMoving)
        // {
        //     Debug.Log("head is moving because rotation is at " + headRotation);
            
        // }
        // if (!headMoving)
        // {
        //     Debug.Log("head is not moving because rotation is at " + headRotation);
        // }

       

        // head is moving but hands and objects are not.
        if (conditionsMet)
        {
            HandleSuccessfulCondition();
        }
        else
        {
            HandleFailedCondition();
        }
    }

    

    private void HandleSuccessfulCondition()
    {
        gracePeriodTimer = 0f; // Reset grace period when conditions are good
        conditionMetTime += Time.deltaTime;
        Debug.Log("conditions met but not long enough");
        if (conditionMetTime >= requiredDuration)
        {
            StartCoroutine(TriggerBeep());
            conditionMetTime = 0f;
        }
    }

    // added this because it was too constrictive when it kept having to restart the time when for an instance a person moved their hands
    private void HandleFailedCondition()
    {
        // Only start counting grace period if we had met conditions before
        if (conditionMetTime > 0)
        {
            gracePeriodTimer += Time.deltaTime;
            
            // If we exceed grace period, reset everything
            if (gracePeriodTimer >= gracePeriod)
            {
                Debug.Log("conditions are not met so restarting at "  + conditionMetTime + "and" + gracePeriodTimer);
                conditionMetTime = 0f;
                gracePeriodTimer = 0f;
            }
        }
        else
        {
            gracePeriodTimer = 0f; // Reset if we weren't making progress
        }
    }

    private System.Collections.IEnumerator TriggerBeep()
    {
        canTrigger = false;
        beepAudio.Play();
        
        yield return new WaitForSeconds(cooldownDuration);
        
        canTrigger = true;
        conditionMetTime = 0f; // Full reset after successful trigger
        gracePeriodTimer = 0f;
    }
}