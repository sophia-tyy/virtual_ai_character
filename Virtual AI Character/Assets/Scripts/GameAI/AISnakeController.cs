using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AISnakeController : MonoBehaviour
{
    public GameObject GameController;
    public Vector2 dir = Vector2.up;
    public float step = 0.2f;
    public GameObject snakeBodyPrefab;
    public List<Transform> bodyParts = new List<Transform>();
    private int pendingDir = 0;
    public UnityEvent onAteFood;
    public UnityEvent<string> onDied;

    void Start()
    {
        GameController = GameObject.Find("GameController");
        for (int i = 0; i < 3; i++) AddBodyPart();
        InvokeRepeating("Move", 0.3f, 0.5f);
    }

    public void ChangeDirection(int dirCode)
    {
        pendingDir = dirCode;
    }

    void Move()
    {
        if (pendingDir != 0)
        {
            Vector2 newDir = dir;
            if (pendingDir == 1) {newDir = new Vector2(dir.y, -dir.x); }
            else if (pendingDir == 2) { newDir = new Vector2(-dir.y, dir.x); }
            dir = newDir;
            pendingDir = 0;
        }

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
        GameObject newBody;
        if (GameController == null) newBody = Instantiate(snakeBodyPrefab);
        else newBody = Instantiate(snakeBodyPrefab, GameController.GetComponent<GameController>().AISnakeParent);
        Vector3 spawnPos;
        if (bodyParts.Count == 0) spawnPos = transform.position - (Vector3)dir * 0.2f;
        else spawnPos = bodyParts[bodyParts.Count - 1].position - (Vector3)dir * 0.2f;
        newBody.transform.position = spawnPos;
        bodyParts.Add(newBody.transform);
    }

    public void ResetSnake(Vector3 startPosition)
    {
        transform.position = startPosition;
        dir = Vector2.up;
        pendingDir = 0;

        foreach (Transform part in bodyParts) Destroy(part.gameObject);
        bodyParts.Clear();
        for (int i = 0; i < 2; i++) AddBodyPart();
        
        CancelInvoke();
        InvokeRepeating("Move", 0.3f, 0.5f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Food"))
        {
            Destroy(collision.gameObject);
            AddBodyPart();
            onAteFood?.Invoke();
        }
        else onDied?.Invoke(collision.gameObject.tag);
    }
}