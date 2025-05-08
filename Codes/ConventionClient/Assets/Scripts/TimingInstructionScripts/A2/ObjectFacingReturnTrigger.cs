using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ConecastHandler
{
    [SerializeField] private float _distance = 10f;
    [SerializeField] private float _angle = 30f; // Cone angle in degrees
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private int _rayCount = 10; // Number of rays to use for cone approximation

    public HashSet<GameObject> GetObjectsInSight(Transform origin)
    {
        HashSet<GameObject> hitObjects = new HashSet<GameObject>();

        Vector3 originPos = origin.position;
        Vector3 mainDirection = origin.forward;

        // Draw main cast line
        Debug.DrawLine(originPos, originPos + mainDirection * _distance, Color.green);

        // Draw cone visualization
        DrawCone(originPos, mainDirection, _angle, _distance);

        // Get all potential objects in a sphere that encompasses the cone
        float sphereRadius = _distance * Mathf.Sin(_angle * 0.5f * Mathf.Deg2Rad);
        Collider[] colliders = Physics.OverlapSphere(originPos + mainDirection * _distance * 0.5f, 
                                               _distance * 0.5f, _layerMask);

        foreach (var collider in colliders)
        {
            Vector3 toObject = collider.transform.position - originPos;
            float angle = Vector3.Angle(mainDirection, toObject);
            float distance = toObject.magnitude;

            if (angle <= _angle * 0.5f && distance <= _distance)
            {
                hitObjects.Add(collider.transform.root.gameObject);
                Debug.DrawLine(originPos, collider.transform.position, Color.red);
            }
        }

        return hitObjects;
    }

    private void ProcessHits(RaycastHit[] hits, HashSet<GameObject> hitObjects)
    {
        foreach (var hit in hits)
        {
            if (hit.collider != null)
            {
                hitObjects.Add(hit.collider.transform.root.gameObject);
                Debug.Log("hit");
                Debug.DrawLine(hit.point, hit.point + Vector3.up * 0.2f, Color.red);
            }
        }
    }

    private void DrawCone(Vector3 origin, Vector3 direction, float angle, float distance)
    {
        float halfAngle = angle * 0.5f * Mathf.Deg2Rad;
        float endRadius = distance * Mathf.Tan(halfAngle);
        Vector3 endCenter = origin + direction * distance;

        // Draw circle at the end of the cone
        DrawCircle(endCenter, direction, endRadius, Color.cyan);

        // Draw lines from origin to circle edge
        Vector3 right = Vector3.Cross(direction, Vector3.up).normalized;
        if (right.magnitude < 0.1f) right = Vector3.Cross(direction, Vector3.forward).normalized;
        Vector3 up = Vector3.Cross(right, direction).normalized;

        for (int i = 0; i < 12; i++)
        {
            float a = i * Mathf.PI * 2 / 12;
            Vector3 circlePoint = endCenter + (right * Mathf.Cos(a) + up * Mathf.Sin(a)) * endRadius;
            Debug.DrawLine(origin, circlePoint, Color.cyan);
        }
    }

    private void DrawCircle(Vector3 center, Vector3 normal, float radius, Color color, int segments = 20)
    {
        Vector3 right = Vector3.Cross(normal, Vector3.up).normalized;
        if (right.magnitude < 0.1f) right = Vector3.Cross(normal, Vector3.forward).normalized;
        Vector3 up = Vector3.Cross(right, normal).normalized;

        float angle = 0f;
        float increment = 360f / segments;

        Vector3 lastPoint = center + (right * Mathf.Cos(angle * Mathf.Deg2Rad) + up * Mathf.Sin(angle * Mathf.Deg2Rad)) * radius;

        for (int i = 1; i <= segments; i++)
        {
            angle += increment;
            Vector3 nextPoint = center + (right * Mathf.Cos(angle * Mathf.Deg2Rad) + up * Mathf.Sin(angle * Mathf.Deg2Rad)) * radius;
            Debug.DrawLine(lastPoint, nextPoint, color);
            lastPoint = nextPoint;
        }
    }

    public void DrawGizmos(Transform origin)
    {
        Vector3 direction = origin.forward;
        Gizmos.color = Color.cyan;
        Vector3 endPoint = origin.position + direction * _distance;
        Gizmos.DrawLine(origin.position, endPoint);

        // Draw cone gizmo
        float halfAngle = _angle * 0.5f * Mathf.Deg2Rad;
        float endRadius = _distance * Mathf.Tan(halfAngle);
        DrawGizmoCircle(endPoint, direction, endRadius);
    }

    private void DrawGizmoCircle(Vector3 center, Vector3 normal, float radius)
    {
        Vector3 right = Vector3.Cross(normal, Vector3.up).normalized;
        if (right.magnitude < 0.1f) right = Vector3.Cross(normal, Vector3.forward).normalized;
        Vector3 up = Vector3.Cross(right, normal).normalized;

        Vector3 lastPoint = center + right * radius;
        for (int i = 1; i <= 20; i++)
        {
            float angle = i * Mathf.PI * 2 / 20;
            Vector3 nextPoint = center + (right * Mathf.Cos(angle) + up * Mathf.Sin(angle)) * radius;
            Gizmos.DrawLine(lastPoint, nextPoint);
            lastPoint = nextPoint;
        }
    }
}

public class ObjectFacingReturnTrigger : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private ObjectRotationTracker _tracker;
    [SerializeField] private Transform _glassesTransform;
    [SerializeField] private AudioSource _beepAudio;
    [SerializeField] private ConecastHandler _conecastHandler = new ConecastHandler();

    [Header("Detection Settings")]
    [SerializeField] private float _movementThreshold = 0.1f;
    [SerializeField] private float _forwardMovementThreshold = 2;
    [SerializeField] private float _returnThreshold = 0.1f;
    [SerializeField] private float _forwardReturnThreshold = 10;
    [SerializeField] private float _maxReturnTime = 5f;
    [SerializeField] private float _minReturnTime = 0.7f;
    [SerializeField] private float _triggerCooldown = 10f;
    [SerializeField] private float _cumulativeThreshold = 0.5f;
    [SerializeField] private float _forwardCumulativeThreshold = 45;

    [SerializeField] private GameObject PlaneOrigin;
    [SerializeField] private GameObject lineRendererObject;
    [SerializeField] private Vector2 graphSize = new Vector2(10f, 2.5f);

    private float timeElapsed = 0f; // Time tracking for the x-axis
    private Vector3 prevUpdatedEndpoint;
    public float timeScale = 0.1f; // Time scale to slow down the graph movement


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

    private class ForwardHistorySample
    {
        public float timestamp;
        public Vector3 up;

        public Vector3 right;
    }

    private Dictionary<GameObject, List<DotHistorySample>> _dotHistory = new();
    private Dictionary<GameObject, List<ForwardHistorySample>> _forwardHistory = new();

    void Start(){
        prevUpdatedEndpoint = PlaneOrigin.transform.position;
    }


    void Update()
    {
        timeElapsed += Time.deltaTime * timeScale;
        float now = Time.time;
        var logs = _tracker.GetRotationLogs();
        if (!logs.ContainsKey(_glassesTransform.gameObject)) return;
        

        // Get objects currently in view of the cone
        HashSet<GameObject> visibleObjects = _conecastHandler.GetObjectsInSight(_glassesTransform);

        foreach (var kvp in logs)
        {
            GameObject obj = kvp.Key;
            if (obj == _glassesTransform.gameObject) continue;
            if (!visibleObjects.Contains(obj)) continue;

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

            

            // Get latest orientation samples
            ObjectRotationTracker.RotationHistory latestObjSample = objectHistory[^1];
            ObjectRotationTracker.RotationHistory latestGlassesSample = _tracker.GetClosestSnapshot(_glassesTransform.gameObject, latestObjSample.timestamp);
            if (latestGlassesSample == null) continue;

            // Compute dot product between forward directions
            Vector3 currentObjForward = latestObjSample.rotation * Vector3.forward;
            Vector3 currentGlassesForward = latestGlassesSample.rotation * Vector3.forward;
            float currentDot = Vector3.Dot(currentObjForward, currentGlassesForward);
            Vector3 currentObjUp = latestObjSample.rotation * Vector3.up;
            Vector3 currentObjRight = latestObjSample.rotation * Vector3.right;
            

            if (!_dotHistory.ContainsKey(obj))
                _dotHistory[obj] = new List<DotHistorySample>();

            if (!_forwardHistory.ContainsKey(obj))
                _forwardHistory[obj] = new List<ForwardHistorySample>();

            _dotHistory[obj].Add(new DotHistorySample { timestamp = now, dot = currentDot });
            _dotHistory[obj].RemoveAll(sample => now - sample.timestamp > _maxReturnTime);

            _forwardHistory[obj].Add(new ForwardHistorySample { timestamp = now, up = currentObjUp, right = currentObjRight});
            _forwardHistory[obj].RemoveAll(sample => now - sample.timestamp > _maxReturnTime);
            // if objects are not moving there's no reason to trigger
            if (!_tracker.IsObjectMoving(obj)) continue;

           
            float totalDotDeviation = 0f;
            float totalAngelChangeUp = 0f;
            float totalAngelChangeRight = 0f;
            float firstSignificantDeviationTime = -1f;
            

            // Analyze both dot product AND forward vector history
            for (int i = 0; i < _dotHistory[obj].Count; i++)
            {
                // Dot product analysis (existing behavior)
                float dotDelta = Mathf.Abs(_dotHistory[obj][i].dot - currentDot);
                totalDotDeviation += dotDelta;
    
                // Forward vector analysis (new potential functionality)
                float angleDeltaUp = Vector3.Angle(_forwardHistory[obj][i].up, currentObjUp);
                float angleDeltaRight = Vector3.Angle(_forwardHistory[obj][i].right, currentObjRight);
                totalAngelChangeUp += angleDeltaUp;
                totalAngelChangeRight += angleDeltaRight;

                // Track first significant deviation time
                if (dotDelta > _movementThreshold && firstSignificantDeviationTime < 0f)
                {
                    firstSignificantDeviationTime = _dotHistory[obj][i].timestamp;
                }
                else if (totalAngelChangeRight > _forwardMovementThreshold || totalAngelChangeUp > _forwardMovementThreshold && firstSignificantDeviationTime < 0f)
                {
                    firstSignificantDeviationTime = _forwardHistory[obj][i].timestamp;
                }
            }

            bool substantialDeviation = totalDotDeviation >= _cumulativeThreshold; 
            bool rightSubstantialDeviation = totalAngelChangeRight >= _forwardCumulativeThreshold;
            bool upSubstantialDeviation = totalAngelChangeUp >= _forwardCumulativeThreshold;
            bool CheckReturnToAlignment(GameObject obj, float currentDot, Vector3 currentObjRight, Vector3 currentObjUp, float now)
            {
                for(int i = 0; i < _dotHistory[obj].Count; i++)
                {
                    // Skip if sample is too recent (avoid comparing to itself)
                    if (now - _dotHistory[obj][i].timestamp < 0.1f) continue;
        
                    if (Mathf.Abs(currentDot - _dotHistory[obj][i].dot) < _returnThreshold)
                    {
                       // Debug.Log("alignment due to dot");
                        return true; // Found a matching historical alignment
                    }
                    else if(Vector3.Angle(currentObjUp, _forwardHistory[obj][i].up) < _forwardReturnThreshold || Vector3.Angle(currentObjRight, _forwardHistory[obj][i].right) < _forwardReturnThreshold)
                    {
                        //Debug.Log("alignment due to forward");
                        return true;
                    }
                    // else 
                    // {
                    //     Debug.Log("forward: " + Vector3.Angle(_forwardHistory[obj][i].up, currentObjUp));
                    // }
                    else
                    {
                        Debug.Log("no alignmetn" + Vector3.Angle(currentObjRight, _forwardHistory[obj][i].right));
                    }
                    
                }
                
                return false;
            }


            bool closeToPastAlignment = CheckReturnToAlignment(obj, currentDot, currentObjRight, currentObjUp, now);
           
            bool timeOK = firstSignificantDeviationTime > 0 &&
                        now - firstSignificantDeviationTime >= _minReturnTime &&
                        now - firstSignificantDeviationTime <= _maxReturnTime;
            if(closeToPastAlignment && !substantialDeviation)
            {
                Debug.Log("alighment but no deviation" + totalAngelChangeRight);
                
            }

            if (substantialDeviation && closeToPastAlignment && timeOK && rightSubstantialDeviation && upSubstantialDeviation)
            {
                
                Debug.Log($"{obj.name}");
                _beepAudio?.Play();
                _lastTriggerTimes[obj] = now;
                _dotHistory[obj].Clear();
                _forwardHistory[obj].Clear();
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (_glassesTransform != null)
        {
            _conecastHandler.DrawGizmos(_glassesTransform);
        }
    }
}