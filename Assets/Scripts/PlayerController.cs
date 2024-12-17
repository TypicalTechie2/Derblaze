using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //Player object component variables
    [SerializeField] private Rigidbody rb;
    [SerializeField] private LineRenderer lineRenderer;

    //player ball force variables
    private Vector3 startPoint;
    private Vector3 currentPoint;
    private bool isDragging = false;
    [SerializeField] private float maxForce = 20f;
    [SerializeField] private float forceMultiplier = 5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Initialize the LineRenderer component
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.positionCount = 2; // Start and end points
        lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Set a simple material
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
        lineRenderer.enabled = false; // Disable initially
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hole"))
        {
            Debug.Log("Touched the Hole");
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (other.CompareTag("TriggerArea"))
        {
            Debug.Log("Touched the Trigger Area");
            Destroy(gameObject);
        }
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == gameObject)
            {
                isDragging = true;
                startPoint = new Vector3(hit.point.x, 0, hit.point.z);
                lineRenderer.enabled = true;
            }
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                currentPoint = new Vector3(hit.point.x, 0, hit.point.z);
                ShowForce();
                UpdateDirectionIndicator();
            }
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 endPoint = new Vector3(hit.point.x, 0, hit.point.z);
                ApplyForce(endPoint);
            }
            lineRenderer.enabled = false;
        }
    }

    private void UpdateDirectionIndicator()
    {
        // Calculate the direction vector from startPoint to currentPoint
        Vector3 dragVector = startPoint - currentPoint;
        Vector3 direction = Vector3.ClampMagnitude(dragVector, maxForce);

        // Predict the ball path with rebounds
        Vector3[] pathPoints = PredictPath(transform.position, direction.normalized);

        // Update LineRenderer positions
        lineRenderer.positionCount = pathPoints.Length;
        lineRenderer.SetPositions(pathPoints);
    }

    private Vector3[] PredictPath(Vector3 startPosition, Vector3 initialDirection)
    {
        Vector3[] pathPoints = new Vector3[2]; // Only two points: start and end (after 1 rebound)
        pathPoints[0] = startPosition;

        Vector3 currentPosition = startPosition;
        Vector3 currentDirection = initialDirection;

        // Maximum length for each segment (to keep the path shorter)
        float maxSegmentLength = 50f; // Adjust this to control how long each path segment is

        // For a single rebound (maxRebounds = 1)
        if (Physics.Raycast(currentPosition, currentDirection, out RaycastHit hit))
        {
            // Calculate the distance to the hit point
            float segmentLength = Vector3.Distance(currentPosition, hit.point);

            // Limit the length of the segment to maxSegmentLength
            if (segmentLength > maxSegmentLength)
            {
                pathPoints[1] = currentPosition + currentDirection * maxSegmentLength;
            }
            else
            {
                pathPoints[1] = new Vector3(hit.point.x, startPosition.y, hit.point.z);
            }
        }
        else
        {
            // If no collision, show the last segment as straight
            pathPoints[1] = currentPosition + currentDirection * maxSegmentLength;
        }

        return pathPoints;
    }

    private void ShowForce()
    {
        Vector3 dragVector = startPoint - currentPoint;
        Vector3 force = Vector3.ClampMagnitude(dragVector * forceMultiplier, maxForce);

        // Log force magnitude in real-time
        Debug.Log($"Current Force: {force.magnitude}");
    }

    private void ApplyForce(Vector3 endPoint)
    {
        Vector3 dragVector = startPoint - endPoint;
        Vector3 force = Vector3.ClampMagnitude(dragVector * forceMultiplier, maxForce);

        // Apply the calculated force to the Rigidbody
        rb.AddForce(force, ForceMode.Impulse);

        // Wait for the player movement to complete before ending the turn
        StartCoroutine(WaitForPlayerMovement());
    }

    private IEnumerator WaitForPlayerMovement()
    {
        yield return new WaitForSeconds(1f); // Adjust delay based on expected movement duration
        GameManager.Instance.EndTurn(); // End the player's turn
    }
}
