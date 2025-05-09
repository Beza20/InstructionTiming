using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using TMPro;


public class A2TriggerSimplified : MonoBehaviour
{
    [SerializeField] private ObjectRotationTracker _tracker;
    [SerializeField] private Transform _glassesTransform;
    [SerializeField] private AudioSource beepAudio;
    [SerializeField] private AudioSource beepAudioV2;
    [SerializeField] private ConecastHandling _conecastHandler;
    [SerializeField] private TriggerInstructionPlayer instructionPlayer;


    [Header("Trigger States")]
    public bool isRotating = false;  // State for A1 GameObject (Checkbox)
    public bool isShifting = false;  // State for A2 GameObject (Checkbox)
    //[SerializeField] private float _maxReturnTime = 5f;

    public float returnThreshold = 5f; // Degrees
    public float movReturnThreshold = 0.005f;
    public float minReturnTime = 0.5f;
    public float maxReturnTime = 7f;
    [SerializeField] private float _triggerCooldown = 2.5f;

    public float deviationThreshold = 90f;
    public float shiftingThreshold = 0.05f;

    public TextMeshProUGUI rotate;
    public TextMeshProUGUI move;
    public TextMeshProUGUI visiblOBj;


    private float timeElapsed = 0f; 

    private Dictionary<GameObject, List<float>> returnTimestamps = new Dictionary<GameObject, List<float>>();
    private Dictionary<GameObject, float> lastTriggerTime = new Dictionary<GameObject, float>();
    private Dictionary<GameObject, float> _lastTriggerTimes = new Dictionary<GameObject, float>();

    private class QtrHistorySample
    {
        public float timestamp;
        public Quaternion Qtr;

        public Vector3 pos;
    }
     private Dictionary<GameObject, List<QtrHistorySample>> _qtrHistory = new();


    void Update()
    {
        float now = Time.time;
        var logs = _tracker.GetRotationLogs();
        foreach(var kvp in logs)
        {
            GameObject obj = kvp.Key;
            if (obj == _glassesTransform.gameObject) continue;
            kvp.Value.RemoveAll(sample => now - sample.timestamp > maxReturnTime);

        }

        bool anyRotating = false;
        bool anyMoving = false;
        
            

        if (!logs.ContainsKey(_glassesTransform.gameObject)) return;

        HashSet<GameObject> visibleObjects = _conecastHandler.GetObjectsInSight(_glassesTransform);
        visiblOBj.text = ""; // Clear first
        foreach (GameObject piece in visibleObjects)
        {
            visiblOBj.text += piece.name + "\n";
        }
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
            rotate.text = (now - _lastTriggerTimes[obj]).ToString();

            // Check cooldown
            if (now - _lastTriggerTimes[obj] < _triggerCooldown)
            {
                
                continue;
            }

            // Get latest orientation samples
            ObjectRotationTracker.RotationHistory latestObjSample = objectHistory[^1];
            ObjectRotationTracker.RotationHistory latestGlassesSample = _tracker.GetClosestSnapshot(_glassesTransform.gameObject, latestObjSample.timestamp);
            if (latestGlassesSample == null) continue;
            
            if (!_qtrHistory.ContainsKey(obj))
            {
                _qtrHistory[obj] = new List<QtrHistorySample>();
            }
            
            _qtrHistory[obj].Add(new QtrHistorySample { timestamp = now, Qtr = latestObjSample.rotation, pos = latestObjSample.position });
            _qtrHistory[obj].RemoveAll(sample => now - sample.timestamp > maxReturnTime);
            
            

            if (_tracker.IsObjectMovingA2(obj))
            {
                anyRotating = true;
            }
            
            if (_tracker.IsObjectMoving(obj))
            {
                anyMoving = true;
            }

            // Set rotation text once, based on anyRotating
            if (anyRotating)
            {
                //rotate.text = "rotation exists";
                rotate.color = Color.green;
            }
            else
            {
                //rotate.text = "nothing is rotating";
                rotate.color = Color.red;
            }

            // Set movement text once, based on anyMoving
            if (anyMoving)
            {
                move.text = "movement exists";
                move.color = Color.green;
            }
            else
            {
                move.text = "nothing is moving";
                move.color = Color.red;
            }
            if (!_tracker.IsObjectMovingA2(obj)) 
            {
                // rotate.text = "nothing is rotating";
                // rotate.color = Color.red;
                if (isRotating)
                {
                    continue;
                }
                
            }
            if (!_tracker.IsObjectMoving(obj)) 
            {
                // move.text = "nothing is moving";
                // move.color = Color.red;
                if (isShifting)
                {
                    continue;
                }
            }

           

            
            // Check history for return patterns
            bool hasDeviated = false;
            bool hasMoved = false;
            var history = _qtrHistory[obj];
            for (int i = 0; i < history.Count - 1; i++)
            {
                var pastSample = history[i];
                float age = now - pastSample.timestamp;
                int k = 0;
                
                if (age >= minReturnTime && age <= maxReturnTime)
                {
                    float dot = Quaternion.Dot(latestObjSample.rotation.normalized, pastSample.Qtr.normalized);
                    float angularDifference = Mathf.Acos(Mathf.Min(Mathf.Abs(dot), 1f)) * 2f * Mathf.Rad2Deg; // angle in degrees
                    float currentAngleToPast = Mathf.Acos(Mathf.Min(Mathf.Abs(Quaternion.Dot(latestObjSample.rotation.normalized, pastSample.Qtr.normalized)), 1f)) * 2f * Mathf.Rad2Deg;

                    float pos_diff = Vector3.Distance(latestObjSample.position, pastSample.pos);

                   // float currentAngleToPast = Quaternion.Angle(latestObjSample.rotation, pastSample.Qtr);
                    float totalDeviation = 0;
                    float totalMovement = 0;

                    // Look for deviation between 'past' and now
                    
                    for (int j = 0; j < i - 1; j++)
                    {
                        var middleSample = history[j];
                        var middleSampleNxt = history[j + 1];
                        float deviation = Quaternion.Angle(middleSample.Qtr, middleSampleNxt.Qtr);
                        float movement = Vector3.Distance(middleSample.pos, middleSampleNxt.pos);
                        totalDeviation += deviation;
                        totalMovement += movement;
                       
                        if (totalDeviation > deviationThreshold)
                        {
                            //Debug.Log("enough deviation " + totalDeviation);
                            hasDeviated = true;
                            k = j;
                            break;
                        }
                        if (totalMovement > shiftingThreshold)
                        {
                            Debug.Log("enough deviation " + totalMovement);
                            hasMoved = true;
                            k = j;
                            break;
                        }

                    }
                    //Debug.Log("before trigger currentAngleToPast " + currentAngleToPast + "deviation seen " + k + "return at " + i);
                    

                    // Final trigger condition: Returned + Deviation happened
                    if (currentAngleToPast < returnThreshold && hasDeviated && isRotating)
                    {
                        Debug.Log("currentAngleToPast " + currentAngleToPast + "deviation seen " + k + "return at " + i);
                        
                        // Log timestamp
                        if (!returnTimestamps.ContainsKey(obj))
                            returnTimestamps[obj] = new List<float>();
                        returnTimestamps[obj].Add(now);

                        // Play beep
                        instructionPlayer?.OnTriggerEvent();

                        // Set cooldown
                        _lastTriggerTimes[obj] = now;
                        break; // Triggered once per object per check
                    }
                    if (pos_diff < movReturnThreshold && hasMoved && isShifting)
                    {
                        
                        // Play beep
                        Debug.Log("posdiff " + pos_diff + "pos diff seen " + k + " return at " + i);
                        instructionPlayer?.OnTriggerEvent();
                        _lastTriggerTimes[obj] = now;
                        break; 

                    }
                }
            }
        }
    }
}
