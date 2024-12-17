using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform[] holes; // Array of holes in the play area
    [SerializeField] private Transform[] pointBalls; // Array of point balls
    [SerializeField] private Transform playerBall; // Reference to the player ball

    [SerializeField] private float maxForce = 20f; // Maximum force bot can use
    [SerializeField] private float forceMultiplier = 5f; // Multiplier for force calculation
    [SerializeField] private float decisionDelay = 3f; // Delay before making a decision
    private bool isTurn = false; // To track if it's the bot's turn

    private Transform target; // The target the bot decides to attack

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (isTurn)
        {
            // Bot takes action only after a delay
            isTurn = false;
            Invoke(nameof(MakeDecision), decisionDelay);
        }
    }

    private void MakeDecision()
    {
        // Determine the target
        target = GetNearestTargetToHole();
        if (target != null)
        {
            // Execute the attack after the decision is made
            ExecuteAttack();
        }
        else
        {
            Debug.LogWarning("No valid targets found!");
            EndTurn();
        }
    }

    private void ExecuteAttack()
    {
        Vector3 directionToTarget = (target.position - transform.position).normalized;

        // Calculate the required force to push the ball into the hole
        float requiredForce = CalculateRequiredForce(transform.position, target.position);

        // Apply force towards the target
        ApplyForce(directionToTarget, requiredForce);

        // End the turn after the action
        EndTurn();
    }

    private float CalculateRequiredForce(Vector3 startPosition, Vector3 targetPosition)
    {
        // Calculate the distance between the bot and the target
        float distance = Vector3.Distance(startPosition, targetPosition);

        // Use a multiplier to adjust the force based on distance, clamped to maxForce
        float force = Mathf.Min(maxForce, distance * forceMultiplier);
        return force;
    }

    private Transform GetNearestTargetToHole()
    {
        Transform nearestTarget = null;
        float shortestDistance = Mathf.Infinity;

        // Check all point balls and player ball
        foreach (Transform target in pointBalls)
        {
            if (target == null) continue; // Skip if the target is null (already scored)

            foreach (Transform hole in holes)
            {
                float distance = Vector3.Distance(target.position, hole.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearestTarget = target;
                }
            }
        }

        // Compare with the player ball as well
        foreach (Transform hole in holes)
        {
            float distance = Vector3.Distance(playerBall.position, hole.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestTarget = playerBall;
            }
        }

        return nearestTarget;
    }

    private void ApplyForce(Vector3 direction, float power)
    {
        // Apply the calculated force in the given direction
        Vector3 force = direction * power;
        rb.AddForce(force, ForceMode.Impulse);
        Debug.Log($"Bot applied force: {force}, Magnitude: {force.magnitude}");
    }

    private void EndTurn()
    {
        Debug.Log("Bot turn ended.");
        GameManager.Instance.EndTurn(); // Signal the game manager to switch to the player's turn
    }

    public void StartTurn()
    {
        isTurn = true; // Signal the bot to take its turn
        Debug.Log("Bot turn started.");
    }
}
