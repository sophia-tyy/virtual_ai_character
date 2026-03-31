using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class SnakeAgent : Agent
{
    public GameObject GameController;
    [Header("Agent Components")]
    public AISnakeController snake;
    public LayerMask obstacleMask = -1;

    private GameObject targetFood;

    public override void Initialize()
    {
        GameController = GameObject.Find("GameController");
        
        if (snake == null) snake = GetComponent<AISnakeController>();
        if (snake == null) Debug.LogError("AISnakeController not found on SnakeAgent!");

        snake.onAteFood.AddListener(HandleAteFood);
        snake.onDied.AddListener(HandleDied);
    }

    private void HandleAteFood() { GameController.GetComponent<GameController>().Spawn(); }

    public void HandleDied()
    {
        EndEpisode();
        if (snake != null && snake.bodyParts != null)
        {
            foreach (Transform part in snake.bodyParts) Destroy(part.gameObject);
            snake.bodyParts.Clear();
        }
        Destroy(snake.gameObject);
    }

    public override void OnEpisodeBegin()
    {
        targetFood = null;
    }

    private void FindFood()
    {
        GameObject[] foods = GameObject.FindGameObjectsWithTag("Food");
        float closestDistance = Mathf.Infinity;
        targetFood = null;

        foreach (GameObject food in foods)
        {
            if (food == null) continue;

            float distance = Vector2.Distance(transform.position, food.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                targetFood = food;
            }
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        for (int angle = 0; angle < 360; angle += 45)
        {
            Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.right;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, 5f, obstacleMask);
            float distNorm = (hit.collider ? hit.distance : 5f) / 5f;
            sensor.AddObservation(distNorm);
            sensor.AddObservation(hit.collider ? 1f : 0f);
        }

        FindFood();
        if (targetFood != null)
        {
            Vector2 relFood = ((Vector2)targetFood.transform.position - (Vector2)transform.position).normalized;
            sensor.AddObservation(relFood);
            sensor.AddObservation(Vector2.Distance(transform.position, targetFood.transform.position) / 10f);
        }
        else
        {
            sensor.AddObservation(Vector2.zero);
            sensor.AddObservation(0f);
        }

        sensor.AddObservation(snake.dir == Vector2.right ? 1f : 0f);
        sensor.AddObservation(snake.dir == Vector2.left  ? 1f : 0f);
        sensor.AddObservation(snake.dir == Vector2.up    ? 1f : 0f);
        sensor.AddObservation(snake.dir == Vector2.down  ? 1f : 0f);
        sensor.AddObservation(snake.bodyParts.Count / 50f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int dirCode = actions.DiscreteActions[0];
        snake.ChangeDirection(dirCode);
    }
}