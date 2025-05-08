using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ConecastHandling
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
