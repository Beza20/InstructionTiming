using System.Collections;
using UnityEngine;

public class MovementTrigger : MonoBehaviour
{
    [SerializeField] private MovementTracker movementTracker;
    [SerializeField] private ObjectRotationTracker objectTracker;

    public AudioSource beepAudio;

    public float velocityThreshold = 0.1f;       // Hand movement threshold (m/s)
    public float rotationThreshold = 5.0f;       // Head rotation difference threshold (degrees)
    public float requiredTime = 2f;              // Time condition must hold

    private float conditionMetTime = 0f;
    private bool canTrigger = true;

    void Update()
    {
        float leftVelocity = movementTracker.GetLeftHandVelocity();
        float rightVelocity = movementTracker.GetRightHandVelocity();
        float headRotationDifference = movementTracker.GetHeadRotationDifference();  // <-- Use rotation difference, not angular velocity

        bool isHandStill = (leftVelocity < velocityThreshold) && (rightVelocity < velocityThreshold);
        bool isHeadRotating = headRotationDifference > rotationThreshold;
        bool areObjectsStill = objectTracker.AreObjectsStill(requiredTime);


        if (isHandStill && isHeadRotating && areObjectsStill)
        {
            conditionMetTime += Time.deltaTime;

            if (conditionMetTime >= requiredTime && canTrigger)
            {
                StartCoroutine(TriggerBeep());
                conditionMetTime = 0f;
            }
        }
        else
        {
            conditionMetTime = 0f;  // Reset if condition not met
        }
    }

    IEnumerator TriggerBeep()
    {
        canTrigger = false;
        beepAudio.Play();
        yield return new WaitForSeconds(1f);  // Cooldown
        canTrigger = true;
    }
}
