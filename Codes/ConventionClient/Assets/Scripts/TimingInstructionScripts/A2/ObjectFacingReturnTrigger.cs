using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

// using ray cast handler to make the spheres interacting with the glasses
public class RaycastHandler
{
    [SerializeField] private float _distance = 10f;
    [SerializeField] private float _width = 0.5f;
    [SerializeField] private LayerMask _layerMask;

    // drawing for debugging
    public static class DebugExtension
    {
        public static void DrawWireSphere(Vector3 center, Color color, float radius, int segments = 20)
        {
            float angle = 0f;
            float increment = 360f / segments;

            Vector3 lastPointX = center + new Vector3(0, radius, 0);
            Vector3 lastPointY = center + new Vector3(radius, 0, 0);
            Vector3 lastPointZ = center + new Vector3(0, 0, radius);

            for (int i = 1; i <= segments; i++)
            {
                angle += increment;
                float rad = angle * Mathf.Deg2Rad;

                Vector3 nextPointX = center + new Vector3(0, Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius);
                Vector3 nextPointY = center + new Vector3(Mathf.Cos(rad) * radius, 0, Mathf.Sin(rad) * radius);
                Vector3 nextPointZ = center + new Vector3(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius, 0);

                Debug.DrawLine(lastPointX, nextPointX, color);
                Debug.DrawLine(lastPointY, nextPointY, color);
                Debug.DrawLine(lastPointZ, nextPointZ, color);

                lastPointX = nextPointX;
                lastPointY = nextPointY;
                lastPointZ = nextPointZ;
            }
        }
    }
    
    public HashSet<GameObject> GetObjectsInSight(Transform origin)
    {
        HashSet<GameObject> hitObjects = new HashSet<GameObject>();

        Vector3 start = origin.position;
        Vector3 direction = origin.forward;  // played around with making this ngative because the motive rigid body was having issues
        Vector3 end = start + direction * _distance;

        // Draw main cast line
        Debug.DrawLine(start, end, Color.green);

        // Draw radius circles at start and end
        DebugExtension.DrawWireSphere(start, Color.cyan, _width); 
        DebugExtension.DrawWireSphere(end, Color.cyan, _width);

        // Do the cast
        RaycastHit[] hits = Physics.SphereCastAll(
            start,
            _width,
            direction,
            _distance,
            _layerMask
        );

        foreach (var hit in hits)
        {
            hitObjects.Add(hit.collider.transform.root.gameObject);
            if (hit.distance == 0) continue; // likely overlapping at cast start


            // Debugging to see which objects the sphere is hitting
            Debug.DrawLine(hit.point, hit.point + Vector3.up * 0.2f, Color.red);

            // Label hit object
            //Debug.Log($"[SphereCast HIT] {hit.collider.gameObject.name}");
        }

        return hitObjects;
    }

    public void DrawGizmos(Transform origin)
    {
        Vector3 direction = origin.forward;
        Gizmos.color = Color.cyan;
        Vector3 endPoint = origin.position + direction * _distance;
        Gizmos.DrawLine(origin.position, endPoint);
        Gizmos.DrawWireSphere(endPoint, _width);
    }
}
// this is the main class for A2
public class ObjectFacingReturnTrigger : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private ObjectRotationTracker _tracker;
    [SerializeField] private Transform _glassesTransform;
    [SerializeField] private AudioSource _beepAudio;
    [SerializeField] private RaycastHandler _raycastHandler = new RaycastHandler();

    [Header("Detection Settings")]
    [SerializeField] private float _movementThreshold = 0.1f;
    [SerializeField] private float _returnThreshold = 0.05f;
    [SerializeField] private float _maxReturnTime = 5f;
    [SerializeField] private float _minReturnTime = 0.7f;
    [SerializeField] private float _triggerCooldown = 10f;
    [SerializeField] private float _cumulativeThreshold = 0.5f;

    private Dictionary<GameObject, float> _lastTriggerTimes = new Dictionary<GameObject, float>();
    private Dictionary<GameObject, (Vector3 dir, float timestamp)> _initialDirections = new Dictionary<GameObject, (Vector3, float)>();
    private Dictionary<GameObject, bool> _isMoving = new Dictionary<GameObject, bool>();
    private Dictionary<GameObject, float> _movementStartTimes = new Dictionary<GameObject, float>();
    private Dictionary<GameObject, float> _maxDeviationDots = new Dictionary<GameObject, float>();

    private class DotHistorySample
    {
        public float timestamp;
        public float dot;
    }

private Dictionary<GameObject, List<DotHistorySample>> _dotHistory = new();

    void Update()
    {
        float now = Time.time;
        var logs = _tracker.GetRotationLogs();
        if (!logs.ContainsKey(_glassesTransform.gameObject)) return;

        // Get objects currently in view of the sphere
        HashSet<GameObject> visibleObjects = _raycastHandler.GetObjectsInSight(_glassesTransform);

        foreach (var kvp in logs)
        {
            GameObject obj = kvp.Key;
            if (obj == _glassesTransform.gameObject) continue;
            if (!visibleObjects.Contains(obj))continue;

            // if(visibleObjects.Contains(obj))
            // {
            //     //Debug.Log(obj.name + " intercepted");
            // }

            List<ObjectRotationTracker.RotationHistory> objectHistory = kvp.Value;
            if (objectHistory.Count < 2) continue;

            // Initialize tracking if needed
            if (!_lastTriggerTimes.ContainsKey(obj))
            {
                _lastTriggerTimes[obj] = -_triggerCooldown;
            }

            // Check cooldown
            if (now - _lastTriggerTimes[obj] < _triggerCooldown)
            {
                continue;
            }

            // Get current orientation data

            // Get latest orientation samples
            ObjectRotationTracker.RotationHistory latestObjSample = objectHistory[^1];
            ObjectRotationTracker.RotationHistory latestGlassesSample = _tracker.GetClosestSnapshot(_glassesTransform.gameObject, latestObjSample.timestamp);
            if (latestGlassesSample == null) continue;

            // Compute dot product between forward directions
            Vector3 currentObjForward = latestObjSample.rotation * Vector3.forward;
            Vector3 currentGlassesForward = latestGlassesSample.rotation * Vector3.forward;
            float currentDot = Vector3.Dot(currentObjForward, currentGlassesForward);

            if (!_dotHistory.ContainsKey(obj))
                _dotHistory[obj] = new List<DotHistorySample>();

            //we only care about the dot products of the previous 5 seconds

            _dotHistory[obj].Add(new DotHistorySample { timestamp = now, dot = currentDot });
            _dotHistory[obj].RemoveAll(sample => now - sample.timestamp > _maxReturnTime);

            // if objects are not moving there's no reason to trigger
            if (!_tracker.IsObjectMoving(obj)) continue;

            float totalDeviation = 0f;
            float firstSignificantDeviationTime = -1f;


            //checks entire dictionary if significant movement was made over the past 5 seconds
            foreach (var sample in _dotHistory[obj])
            {
                float delta = Mathf.Abs(sample.dot - currentDot);
                totalDeviation += delta;

                if (delta > _movementThreshold && firstSignificantDeviationTime < 0f)
                {
                    firstSignificantDeviationTime = sample.timestamp;
                }
            }

            bool substantialDeviation = totalDeviation >= _cumulativeThreshold; 
            bool closeToPastAlignment = Mathf.Abs(currentDot - _dotHistory[obj][0].dot) < _returnThreshold;
            bool timeOK = firstSignificantDeviationTime > 0 &&
                        now - firstSignificantDeviationTime >= _minReturnTime &&
                        now - firstSignificantDeviationTime <= _maxReturnTime;

            //if so trigger

            if (substantialDeviation && closeToPastAlignment && timeOK)
            {
                Debug.Log($"{obj.name} 
                _beepAudio?.Play();
                _dotHistory[obj].Clear();
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (_glassesTransform != null)
        {
            _raycastHandler.DrawGizmos(_glassesTransform);
        }
    }
}

 