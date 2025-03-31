using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using FurnitureState;


public class TaskProgress : MonoBehaviour
{

    //[SerializeField] ProgressFeatures taskProgress = new ProgressFeatures();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}


// [System.Serializable]
// public class ProgressFeatures{
//     public double posErrorWeight;
//     public double rotErrorWeight;
//     public StateElement furnitureStatus;
//     public Vector3 goalDistanceOne;
//     public Vector3 goalDistanceTwo;
//     public Vector3 goalDistanceThree;

//     public Quaternion goalRottDiffOne;
//     public Quaternion goalRottDiffTwo;
//     public Quaternion goalRottDiffThree;

//     public double normPosError;
//     public double normRotError;

//     public double error_quantified;

// }
// public class TaskProgress : MonoBehaviour
// {

//     [SerializeField] ProgressFeatures taskProgress = new ProgressFeatures();
//     // Start is called before the first frame update
//     void Start()
//     {
        
//     }

//     // Update is called once per frame
//     void Update()
//     {
//         double calculatedPosError = (furnitureStatus.DistanceOne - taskProgress.goalDistanceOne).magnitude/furnitureStatus.PosThreshold.magnitude;
//         taskProgress.normPosError = min(1, calculatedPosError);

//         double calculatedRotError = (1 - furnitureState.rottDiffOne * taskProgress.goalRottDiffOne)/furnitureState.RotThreshold.magnitude;
//         taskProgress.normRotError = min(1, calculatedRotError);


//     }
// }
