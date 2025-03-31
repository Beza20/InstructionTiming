using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FilteredGraphRendering : MonoBehaviour
{
    [SerializeField] private TimelyInstruction timelyInstruction;  // Reference to TimelyInstruction class for filtered progress
    [SerializeField] private float maxTime = 600f;                 // Total time to plot (e.g., 60 seconds)
    [SerializeField] private int maxPoints = 1000;                 // Maximum number of points on the graph
    [SerializeField] private Vector2 graphSize = new Vector2(10f, 2.5f); // Size of the graph in world space

    [SerializeField] private GameObject PlaneOrigin;
    [SerializeField] private GameObject lineRendererObject;

    private float timeElapsed = 0f; // Time tracking for the x-axis
    private Vector3 prevUpdatedEndpoint;

    public float timeScale = 0.02f; // Time scale to slow down the graph movement

    void Start()
    {
        prevUpdatedEndpoint = PlaneOrigin.transform.position;
    }

    void Update()
    {
        // Increment time elapsed
        timeElapsed += Time.deltaTime * timeScale;

        // Get the filtered progress value from TimelyInstruction
        float filteredProgress = timelyInstruction.filteredProgress;  // Y-axis (use filtered value)

        // Scale and calculate the graph coordinates
        float xValue = timeElapsed;
        float yValue = filteredProgress * graphSize.y;

        Vector3 updatedEndpoint = new Vector3(xValue, yValue, 0) + PlaneOrigin.transform.position;

        // Instantiate a new line to connect the previous point with the current one
        GameObject newLine = Instantiate(lineRendererObject);
        LineRenderer lineRenderer = newLine.GetComponent<LineRenderer>();

        lineRenderer.SetPosition(0, prevUpdatedEndpoint);
        lineRenderer.SetPosition(1, updatedEndpoint);

        // Update the previous endpoint
        prevUpdatedEndpoint = updatedEndpoint;
    }
}
