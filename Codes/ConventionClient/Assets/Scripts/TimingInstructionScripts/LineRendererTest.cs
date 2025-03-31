using UnityEngine;

public class LineRendererTest : MonoBehaviour
{
    private LineRenderer lineRenderer;

    void Start()
    {
        // Create the LineRenderer component
        lineRenderer = gameObject.AddComponent<LineRenderer>();

        // Set the number of positions to draw the line between two points
        lineRenderer.positionCount = 2;

        // Set the start and end points of the line
        lineRenderer.SetPosition(0, new Vector3(0, 0, 0));  // Start point
        lineRenderer.SetPosition(1, new Vector3(1, 1, 0));  // End point

        // Set the width of the line
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;

        // Add a material with a color
        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        lineRenderer.material.color = Color.red;

        // Optional: Disable World Space if working in local coordinates
        lineRenderer.useWorldSpace = true;
    }
}
