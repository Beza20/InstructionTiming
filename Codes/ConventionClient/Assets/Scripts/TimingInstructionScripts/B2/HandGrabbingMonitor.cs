using System.Collections.Generic;
using UnityEngine;

public class HandGrabbingMonitor : MonoBehaviour
{
    [Header("Hand Rigidbodies")]
    [SerializeField] private Rigidbody leftHandRigidbody;
    [SerializeField] private Rigidbody rightHandRigidbody;

    [Header("Furniture Pieces")]
    [SerializeField] private List<GameObject> furniturePieces;

    [Header("Grab Settings")]
    [SerializeField] private float grabDistanceThreshold = 0.12f;

    public GameObject grabbedByLeftHand { get; private set; }
    public GameObject grabbedByRightHand { get; private set; }

    void Update()
    {
        if (leftHandRigidbody == null || rightHandRigidbody == null) return;

        Vector3 leftHandPos = leftHandRigidbody.position;
        Vector3 rightHandPos = rightHandRigidbody.position;

        grabbedByLeftHand = DetectClosestGrabbedObject(leftHandPos);
        grabbedByRightHand = DetectClosestGrabbedObject(rightHandPos);
    }

    private GameObject DetectClosestGrabbedObject(Vector3 handPosition)
    {
        foreach (GameObject obj in furniturePieces)
        {
            if (obj == null) continue;

            Collider col = obj.GetComponent<Collider>();
            if (col == null) continue;

            Vector3 closestPoint = col.ClosestPoint(handPosition);
            float distance = Vector3.Distance(closestPoint, handPosition);

            if (distance <= grabDistanceThreshold)
            {
                return obj;
            }
        }

        return null;
    }
}
