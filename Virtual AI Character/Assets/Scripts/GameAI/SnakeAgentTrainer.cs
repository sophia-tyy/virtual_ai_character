using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class SnakeAgentTrainer : Agent
{
    public AISnakeController snake;
    public LayerMask obstacleMask = -1;

    public FoodFactory foodFactory;

    public Transform borderLeft;
    public Transform borderRight;
    public Transform borderTop;
    public Transform borderBottom;

    private GameObject currentFood;
    private float lastFoodDistance = -1f;

    [Header("Episode")]
    public int _currentEpisode = 0;
    public float _cumulativeReward = 0f;
    public int trainingMaxStep = 5000;
    public bool respawnFood = true;

    [Header("Rewards")]
    public float foodReward = 10.0f;
    public float deathPenalty = -20.0f;
    public float surviveReward = 0.00005f;
    public float towardFoodRewardScale = 0.01f;
    public float awayFromFoodPenaltyScale = 0.01f;

    public override void Initialize()
    {
        if (snake == null) snake = GetComponent<AISnakeController>();
        if (snake == null) Debug.LogError("AISnakeController not found on SnakeAgentTrainer!");

        snake.onAteFood.AddListener(HandleAteFood);
        snake.onDied.AddListener(HandleDied);

        if (!Academy.Instance.IsCommunicatorOn) MaxStep = 0;
        else MaxStep = trainingMaxStep;
        _currentEpisode = 0;
        _cumulativeReward = 0f;
    }

    private void HandleAteFood()
    {
        AddReward(foodReward);
        _cumulativeReward = GetCumulativeReward();
        SpawnFood();
    }

    private void HandleDied(string objectTag)
    {
        AddReward(deathPenalty);
        if (objectTag == "Snake") AddReward(-20f);
        _cumulativeReward = GetCumulativeReward();
        EndEpisode();
    }

    public override void OnEpisodeBegin()
    {
        snake.ResetSnake(Vector3.zero);

        GameObject[] foods = GameObject.FindGameObjectsWithTag("Food");
        foreach (GameObject food in foods) { Destroy(food); }

        _currentEpisode++;
        _cumulativeReward = 0f;

        SpawnFood();
    }

    void SpawnFood()
    {
        float x = Random.Range(borderLeft.position.x + 0.2f, borderRight.position.x - 0.2f);
        float y = Random.Range(borderTop.position.y - 0.2f, borderBottom.position.y + 0.2f);
        currentFood = foodFactory.InstantiateFood(x, y);

        lastFoodDistance = currentFood != null
            ? Vector2.Distance(transform.position, currentFood.transform.position)
            : -1f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector2 forward = snake.dir.normalized;
        if (forward == Vector2.zero) forward = Vector2.up;

        if (currentFood != null)
        {
            Vector2 toFood = ((Vector2)currentFood.transform.position - (Vector2)transform.position).normalized;
            float forwardFood = Vector2.Dot(forward, toFood);
            float rightFood = forward.x * toFood.y - forward.y * toFood.x;
            sensor.AddObservation(forwardFood);
            sensor.AddObservation(rightFood);
            sensor.AddObservation(Vector2.Distance(transform.position, currentFood.transform.position) / 10f);
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
        AddReward(surviveReward);

        if (currentFood != null)
        {
            float currentDistance = Vector2.Distance(transform.position, currentFood.transform.position);
            if (lastFoodDistance >= 0f)
            {
                float delta = lastFoodDistance - currentDistance;
                if (delta > 0f) AddReward(delta * towardFoodRewardScale);
                else if (delta < 0f) AddReward(delta * awayFromFoodPenaltyScale);
            }
            lastFoodDistance = currentDistance;
        }

        else
        {
            if (respawnFood) SpawnFood();
            else lastFoodDistance = -1f;
        }

        _cumulativeReward = GetCumulativeReward();
    }
}