using System.Collections.Generic;
using UnityEngine;
using static UserState;
using static FurnitureState;
public class GraphRendering : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public FurnitureState furnitureState;
    public UserState userState;
    public int maxPoints = 100;  // Maximum number of points on the graph
    public float pointSpacing = 0.00001f;  // Adjust to control the spacing between points

    private List<Vector3> points = new List<Vector3>();  // Stores points for the graph

    public float velocityWeight = 0.00005f;  // Weight for calculating user effort
    public float timeFactor = 0.000000000001f;  // Time influence on effort

    void Start()
    {
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        // Initial setup for the LineRenderer
        lineRenderer.positionCount = 0;
        lineRenderer.widthMultiplier = 0.00000001f;
    }

    void Update()
    {
        // Calculate user effort (x-coordinate)
        float headVelocity = userState.headState.HeadVelocity;
        float leftWristVelocity = userState.leftHandState.wristVelocity;
        float rightWristVelocity = userState.rightHandState.wristVelocity;
        float userEffort = CalculateUserEffort(headVelocity, leftWristVelocity, rightWristVelocity);

        // Get task progress (y-coordinate)
        float taskProgress = furnitureState.GetTaskProgress();

        // Add the new point to the graph
        AddPoint(new Vector3(userEffort, taskProgress, 0));

        // Update the LineRenderer with the new points
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }

    // This method calculates user effort based on velocities and time
    private float CalculateUserEffort(float headVelocity, float leftWristVelocity, float rightWristVelocity)
    {
        // User effort is a weighted sum of the velocities plus a time factor
        return (velocityWeight * headVelocity) + (velocityWeight * leftWristVelocity) + 
               (velocityWeight * rightWristVelocity) + timeFactor;
    }

    // Adds a new point (x=userEffort, y=taskProgress) to the graph
    private void AddPoint(Vector3 newPoint)
    {
        points.Add(newPoint);

        // If the number of points exceeds the maximum allowed, remove the oldest point
        if (points.Count > maxPoints)
        {
            points.RemoveAt(0);
        }

        // Adjust x-axis positions to keep the graph within the viewport
        for (int i = 0; i < points.Count; i++)
        {
            points[i] = new Vector3(i * pointSpacing, points[i].y, points[i].z);
        }
    }
}
