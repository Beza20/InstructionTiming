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
    [SerializeField] private float grabDistanceThreshold = 0.004f;

    public List<GameObject> grabbedByLeftHand { get; private set; } = new List<GameObject>();
    public List<GameObject> grabbedByRightHand { get; private set; } = new List<GameObject>();

    void Update()
    {
        if (leftHandRigidbody == null || rightHandRigidbody == null) return;

        Vector3 leftHandPos = leftHandRigidbody.position;
        Vector3 rightHandPos = rightHandRigidbody.position;

        grabbedByLeftHand = DetectGrabbedObjects(leftHandPos);
        grabbedByRightHand = DetectGrabbedObjects(rightHandPos);
    }

    private List<GameObject> DetectGrabbedObjects(Vector3 handPosition)
    {
        List<GameObject> grabbedObjects = new List<GameObject>();

        Collider[] hits = Physics.OverlapSphere(handPosition, grabDistanceThreshold);
        foreach (Collider hit in hits)
        {
            GameObject obj = hit.gameObject;

            // Check if this object is in the furniture list
            if (furniturePieces.Contains(obj))
            {
                if (!grabbedObjects.Contains(obj))
                {
                    grabbedObjects.Add(obj);
                    //Debug.Log($"✅ Grabbed {obj.name}");
                }
            }
        }

        return grabbedObjects;
    }
    void OnDrawGizmos()
    {
        foreach (var obj in furniturePieces)
        {
            if (obj == null) continue;

            MeshCollider col = obj.GetComponent<MeshCollider>();
            if (col != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireMesh(col.sharedMesh, obj.transform.position, obj.transform.rotation, obj.transform.lossyScale);
            }
        }

        if (leftHandRigidbody != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(leftHandRigidbody.position, grabDistanceThreshold);
        }
        if (rightHandRigidbody != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(rightHandRigidbody.position, grabDistanceThreshold);
        }
    }
}
