using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SnakeController : MonoBehaviour
{
    public Vector2 dir = Vector2.up;
    public float step = 0.2f;
    public GameObject snakeBodyPrefab;
    private List<Transform> bodyParts = new List<Transform>();
    public GameObject GameController;

    void Start()
    {
        GameController = GameObject.Find("GameController");
        for (int i = 0; i < 3; i++)
        {
            AddBodyPart();
        }
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
        transform.Translate(dir * (step * 2));
        for (int i = 0; i < bodyParts.Count; i++)
        {
            Vector3 tempPos = bodyParts[i].position;
            bodyParts[i].position = headPrevPos;
            headPrevPos = tempPos;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Food"))
        {
            Destroy(collision.gameObject);
            GameDataManager.Instance.AddScore(1);
            GameController.GetComponent<GameController>().UpdateDisplayScores();
            AddBodyPart();
        }
        else
        {
            CancelInvoke();
            GameDataManager.Instance.ResetCurrentScore();
            GameController.GetComponent<GameController>().UpdateDisplayScores();
            GameController.GetComponent<GameController>().EndGame();
        }
    }

    void AddBodyPart()
    {
        GameObject newBody = Instantiate(snakeBodyPrefab, GameController.GetComponent<GameController>().snakeParent);
        Vector3 spawnPos;
        if (bodyParts.Count == 0) spawnPos = transform.position - (Vector3)dir * 0.2f;
        else spawnPos = bodyParts[bodyParts.Count - 1].position - (Vector3)dir * 0.2f;
        newBody.transform.position = spawnPos;
        bodyParts.Add(newBody.transform);
    }
}