using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class AdvSnakeAgent : Agent
{
    public AISnakeController snake;
    public AISnakeController opponentSnake;
    public LayerMask bodyMask = -1;

    public FoodFactory foodFactory;

    public Transform borderLeft;
    public Transform borderRight;
    public Transform borderTop;
    public Transform borderBottom;

    private GameObject currentFood;
    private float previousDist;

    public int _currentEpisode = 0;
    public float _cumulativeReward = 0f;

    public override void Initialize()
    {
        if (snake == null) snake = GetComponent<AISnakeController>();
        if (snake == null) Debug.LogError("AISnakeController not found on AdvSnakeAgent!");

        _currentEpisode = 0;
        _cumulativeReward = 0f;

        snake.onAteFood.AddListener(HandleAteFood);
        snake.onDied.AddListener(HandleDied);

        if (opponentSnake != null)
        {
            opponentSnake.onDied.AddListener(RespawnOpponent);
            opponentSnake.onDied.AddListener(OpponentAteFood);
        }
    }

    private void HandleAteFood()
    {
        AddReward(10.0f);
        _cumulativeReward = GetCumulativeReward();
        SpawnFood();
    }

    private void OpponentAteFood()
    {
        SpawnFood();
        Debug.Log($"Episode {_currentEpisode}: Spawned food after opponent ate the food.");
    }

    private void HandleDied()
    {
        AddReward(-10.0f);
        _cumulativeReward = GetCumulativeReward();
        EndEpisode();
    }

    private void RespawnOpponent()
    {
        Vector3 startPosition = GetSafeStartPosition();
        opponentSnake.ResetSnake(startPosition);
    }

    public override void OnEpisodeBegin()
    {
        snake.ResetSnake(Vector3.zero);

        Vector3 startPosition = GetSafeStartPosition();
        opponentSnake.ResetSnake(startPosition);

        GameObject[] foods = GameObject.FindGameObjectsWithTag("Food");
        foreach (GameObject food in foods) { Destroy(food); }
        SpawnFood();

        previousDist = Vector2.Distance(transform.position, currentFood.transform.position);

        _currentEpisode++;
        _cumulativeReward = 0f;
    }

    void SpawnFood()
    {
            float x = Random.Range(borderLeft.position.x + 0.2f, borderRight.position.x - 0.2f);
            float y = Random.Range(borderTop.position.y - 0.2f, borderBottom.position.y + 0.2f);
            currentFood = foodFactory.InstantiateFood(x, y);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        for (int angle = 0; angle < 360; angle += 45)
        {
            Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.right;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, 5f, bodyMask);
            sensor.AddObservation(hit.distance > 5f ? 5f : hit.distance);
            sensor.AddObservation(hit.collider ? 1f : 0f);
            if (hit.collider != null)
            {
                sensor.AddObservation(hit.collider.CompareTag("Wall") ? 1f : 0f);
                sensor.AddObservation(hit.collider.CompareTag("Snake") ? 1f : 0f);
            }
            else
            {
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
            }
        }

        if (currentFood != null)
        {
            Vector2 relFood = ((Vector2)currentFood.transform.position - (Vector2)transform.position).normalized;
            sensor.AddObservation(relFood);
            sensor.AddObservation(Vector2.Distance(transform.position, currentFood.transform.position) / 10f);
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
        if (currentFood != null)
        {
            float currentDist = Vector2.Distance(transform.position, currentFood.transform.position);
            if (currentDist < previousDist) AddReward(0.01f);
            else AddReward(-0.01f);
            previousDist = currentDist;
        }
        AddReward(0.005f);
        _cumulativeReward = GetCumulativeReward();
    }

    private Vector3 GetSafeStartPosition()
    {
        int maxAttempts = 50;
        float checkRadius = 0.5f;

        for (int i = 0; i < maxAttempts; i++)
        {
            float x = Random.Range(borderLeft.position.x + 1f, borderRight.position.x - 1f);
            float y = Random.Range(borderTop.position.y - 1f, borderBottom.position.y + 1f);
            Vector3 spawnPos = new Vector3(x, y, 0);
            Collider2D hit = Physics2D.OverlapCircle(spawnPos, checkRadius);
            if (hit == null) return spawnPos;
        }

        return Vector3.zero; 
    }
}