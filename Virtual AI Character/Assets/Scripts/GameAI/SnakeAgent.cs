using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class SnakeAgent : Agent
{
    public AISnakeController snake;
    public LayerMask bodyMask = -1;

    // public FoodFactory foodFactory;

    // public Transform borderLeft;
    // public Transform borderRight;
    // public Transform borderTop;
    // public Transform borderBottom;

    private GameObject currentFood;

    // public int _currentEpisode = 0;
    // public float _cumulativeReward = 0f;

    public override void Initialize()
    {
        if (snake == null) snake = GetComponent<AISnakeController>();
        if (snake == null) Debug.LogError("AISnakeController not found on SnakeAgent!");

        // _currentEpisode = 0;
        // _cumulativeReward = 0f;

        // snake.onAteFood.AddListener(HandleAteFood);
        // snake.onDied.AddListener(HandleDied);
    }

    // private void HandleAteFood()
    // {
    //     AddReward(10.0f);
    //     _cumulativeReward = GetCumulativeReward();
    // }

    // private void HandleDied()
    // {
    //     AddReward(-20.0f);
    //     _cumulativeReward = GetCumulativeReward();
    //     EndEpisode();
    // }

    public override void OnEpisodeBegin()
    {
        // snake.ResetSnake();
        // GameObject[] foods = GameObject.FindGameObjectsWithTag("Food");
        // foreach (GameObject food in foods) { Destroy(food); }
        // _currentEpisode++;
        // _cumulativeReward = 0f;
        // SpawnFood();
    }

    // void SpawnFood()
    // {
    //     for (int i = 0; i < 5; i++)
    //     {
    //         float x = Random.Range(borderLeft.position.x + 0.2f, borderRight.position.x - 0.2f);
    //         float y = Random.Range(borderTop.position.y - 0.2f, borderBottom.position.y + 0.2f);
    //         foodFactory.InstantiateFood(x, y);
    //     }
    // }

    public override void CollectObservations(VectorSensor sensor)
    {
        for (int angle = 0; angle < 360; angle += 45)
        {
            Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.right;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, 5f, bodyMask);
            sensor.AddObservation(hit.distance > 5f ? 5f : hit.distance);
            sensor.AddObservation(hit.collider ? 1f : 0f);
        }

        currentFood = UpdateClosestFood();
        if (currentFood != null)
        {
            Vector2 relFood = ((Vector2)currentFood.transform.position - (Vector2)transform.position).normalized;
            sensor.AddObservation(relFood);
            sensor.AddObservation(Vector2.Distance(transform.position, currentFood.transform.position) / 10f);
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
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
        // AddReward(0.00005f);
        // _cumulativeReward = GetCumulativeReward();
    }

    GameObject UpdateClosestFood()
    {
        GameObject[] foods = GameObject.FindGameObjectsWithTag("Food");
        if (foods.Length == 0) { currentFood = null; return null; }

        currentFood = foods[0];
        float minDist = Vector2.Distance(transform.position, currentFood.transform.position);
        foreach (var f in foods)
        {
            float dist = Vector2.Distance(transform.position, f.transform.position);
            if (dist < minDist) { minDist = dist; currentFood = f; }
        }
        return currentFood;
    }
}