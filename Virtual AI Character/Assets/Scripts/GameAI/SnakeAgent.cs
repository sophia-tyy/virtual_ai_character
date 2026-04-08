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

    public void ChangeToSoloMode()
    {
        snake.onDied.RemoveListener(HandleDied);
        snake.onDied.AddListener(HandleDied_solo);
        Debug.Log("SnakeAgent switched to solo mode.");
    }

    private void HandleAteFood() { GameController.GetComponent<GameController>().Spawn(); }

    public void HandleDied(string objectTag)
    {
        EndEpisode();
        if (snake != null && snake.bodyParts != null)
        {
            foreach (Transform part in snake.bodyParts) Destroy(part.gameObject);
            snake.bodyParts.Clear();
        }
        Destroy(snake.gameObject);
    }

    public void HandleDied_solo(string objectTag)
    {
        GameController.GetComponent<GameController>().EndGame();
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
        Vector2 forward = snake.dir.normalized;
        if (forward == Vector2.zero) forward = Vector2.up;

        FindFood();
        if (targetFood != null)
        {
            Vector2 toFood = ((Vector2)targetFood.transform.position - (Vector2)transform.position).normalized;
            float forwardFood = Vector2.Dot(forward, toFood);
            float rightFood = forward.x * toFood.y - forward.y * toFood.x;
            sensor.AddObservation(forwardFood);
            sensor.AddObservation(rightFood);
            sensor.AddObservation(Vector2.Distance(transform.position, targetFood.transform.position) / 10f);
        }
        else
        {
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
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