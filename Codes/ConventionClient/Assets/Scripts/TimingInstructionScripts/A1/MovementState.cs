using UnityEngine;

[System.Serializable]
public class MovementState {
    public Vector3 position;
    public Quaternion rotation;
    public float velocity;         // Positional velocity (m/s)
    public float rotationDifference;  // Total rotation change (degrees)
    public float angularVelocity;     // Angular velocity (rad/s)

    private Vector3 previousPosition;
    private Quaternion previousRotation;
    private float updateInterval = 0.2f;  // Update every 0.2s
    private float timer = 0f;

    public void Initialize(Vector3 startPosition, Quaternion startRotation) {
        position = previousPosition = startPosition;
        rotation = previousRotation = startRotation;
    }

    public void UpdateState(Vector3 newPosition, Quaternion newRotation, float deltaTime) {
        timer += deltaTime;
        if (timer >= updateInterval) {
            // Compute positional velocity (m/s)
            velocity = (newPosition - previousPosition).magnitude / updateInterval;

            // Compute rotation difference (degrees)
            rotationDifference = Quaternion.Angle(previousRotation, newRotation);

            // Compute angular velocity (rad/s) like in Python
            angularVelocity = (rotationDifference * Mathf.Deg2Rad) / updateInterval;

            // Store previous values for next update
            previousPosition = newPosition;
            previousRotation = newRotation;
            timer = 0f;
        }
    }
}
