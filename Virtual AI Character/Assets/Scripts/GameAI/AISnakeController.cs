using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

public class AISnakeController : MonoBehaviour
{
    public Vector2 dir = Vector2.up;
    public float step = 0.2f;
    public GameObject snakeBodyPrefab;
    public List<Transform> bodyParts = new List<Transform>();
    public SnakeAgent snakeAgent;


    void Start()
    {
        snakeAgent = GetComponent<SnakeAgent>();
        for (int i = 0; i < 3; i++) AddBodyPart();
        InvokeRepeating("Move", 0.3f, 0.5f);
    }

    public void ChangeDirection(int direction)
    {
        if (direction == 1 && dir != Vector2.left) dir = Vector2.right;
        else if (direction == 2 && dir != Vector2.right) dir = Vector2.left;
        else if (direction == 3 && dir != Vector2.down) dir = Vector2.up;
        else if (direction == 4 && dir != Vector2.up) dir = Vector2.down;
    }

    void Move()
    {
        Vector3 headPrevPos = transform.position;
        transform.Translate(dir * step);
        for (int i = 0; i < bodyParts.Count; i++)
        {
            Vector3 tempPos = bodyParts[i].position;
            bodyParts[i].position = headPrevPos;
            headPrevPos = tempPos;
        }
    }

    void AddBodyPart()
    {
        GameObject newBody = Instantiate(snakeBodyPrefab);
        newBody.transform.position = bodyParts.Count > 0 ? bodyParts[bodyParts.Count - 1].position : transform.position;
        bodyParts.Add(newBody.transform);
    }

    public void ResetSnake()
    {
        transform.position = Vector3.zero;
        dir = Vector2.up;

        foreach (Transform part in bodyParts) Destroy(part.gameObject);
        bodyParts.Clear();
        for (int i = 0; i < 3; i++) AddBodyPart();
        
        CancelInvoke();
        InvokeRepeating("Move", 0.3f, 0.5f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Food"))
        {
            Destroy(collision.gameObject);
            // GameDataManager.Instance.AddScore(1);
            // GameController.GetComponent<GameController>().UpdateDisplayScores();
            snakeAgent.AddReward(5.0f);
            snakeAgent._cumulativeReward = snakeAgent.GetCumulativeReward();
            AddBodyPart();

            GameObject[] foods = GameObject.FindGameObjectsWithTag("Food");
            if (foods.Length == 0) { snakeAgent.EndEpisode(); }
        }
        else
        {
            snakeAgent.AddReward(-20.0f);
            snakeAgent._cumulativeReward = snakeAgent.GetCumulativeReward();
            snakeAgent.EndEpisode();
            // CancelInvoke();
            // GameDataManager.Instance.ResetCurrentScore();
            // GameController.GetComponent<GameController>().UpdateDisplayScores();
            // GameController.GetComponent<GameController>().EndGame();
        }
    }
}