using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressPlotter : MonoBehaviour
{
   
    [SerializeField] private FurnitureState furnitureState;  // Reference to your FurnitureState class
    [SerializeField] private float maxTime = 600f;            // Total time to plot (e.g., 60 seconds)
    [SerializeField] private int maxPoints = 1000;            // Maximum number of points on the graph
    [SerializeField] private Vector2 graphSize = new Vector2(10f, 5f);  // Size of the graph in world space

    [SerializeField] private GameObject PlaneOrigin;
    //[SerializeField] private MeshRenderer Plane;
    [SerializeField] private GameObject lineRendererObject;

    private List<Vector3> points = new List<Vector3>();      // Stores the points for the graph
    private float timeElapsed = 0f;                          // Time tracking for the x-axis

    public float timeScale = 0.02f;  // Use a time scale less than 1 to slow down the graph movement
    Vector3 prevUpdated_endpoint;

    void Start()
    {
        points.Clear();
        prevUpdated_endpoint = PlaneOrigin.transform.position;
    }

    void Update()
    {
         // Increment time elapsed
        timeElapsed += Time.deltaTime * timeScale;

        // Get the progress value from FurnitureState
        float progress = furnitureState.GetTaskProgress();  // Y-axis

        // Calculate the X and Y coordinates for the graph (scaled to the panel size)
        float xValue = timeElapsed;
       // Mathf.Clamp(timeElapsed / maxTime, 0f, 1f) * graphPanel.rect.width;
        float yValue = progress * 5;
        //Mathf.Clamp(progress, 0f, 1f) * 

        Vector3 updated_endpoint = new Vector3(xValue, yValue, 0) + PlaneOrigin.transform.position;
        GameObject new_line = Instantiate(lineRendererObject);
        LineRenderer lineRenderer = new_line.GetComponent<LineRenderer>();

        lineRenderer.SetPosition(0, prevUpdated_endpoint);
        lineRenderer.SetPosition(1, updated_endpoint);
        prevUpdated_endpoint = updated_endpoint;

    }
}
