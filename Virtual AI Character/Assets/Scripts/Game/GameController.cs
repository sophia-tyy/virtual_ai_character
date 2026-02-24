using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public Transform snakeHeadPrefab;
    public Transform snakeParent;
    private SnakeController snakeController;

    public Transform foodPrefab;
    public Transform foodParent;

    public Transform borderLeft;
    public Transform borderRight;
    public Transform borderTop;
    public Transform borderBottom;

    public GameObject gamePanel;
    public GameObject startButton;

    public void StartGame()
    {
        float x = (borderLeft.position.x + borderRight.position.x) / 2;
        float y = (borderTop.position.y + borderBottom.position.y) / 2;

        Instantiate(snakeHeadPrefab, new Vector2(x, y), Quaternion.identity, snakeParent);
        snakeController = FindObjectOfType<SnakeController>();
        if (snakeController == null) Debug.LogError("SnakeController not found!");

        InvokeRepeating("Spawn", 3, 10);
    }

    void Spawn()
    {
        float x = Random.Range(borderLeft.position.x + 0.2f, borderRight.position.x - 0.2f);
        float y = Random.Range(borderTop.position.y - 0.2f, borderBottom.position.y + 0.2f);
        Instantiate(foodPrefab, new Vector2(x, y), Quaternion.identity, foodParent);
    }

    public void TurnRight() {  snakeController.ChangeDirection(1); }
    public void TurnLeft() {  snakeController.ChangeDirection(2); }
    public void TurnUp() {  snakeController.ChangeDirection(3); }
    public void TurnDown() {  snakeController.ChangeDirection(4); }

    public void EndGame()
    {
        CancelInvoke();

        GameObject[] snakeObjects = GameObject.FindGameObjectsWithTag("Snake");
        foreach (GameObject snake in snakeObjects)
        {
            Destroy(snake);
        }

        GameObject[] foodObjects = GameObject.FindGameObjectsWithTag("Food");
        foreach (GameObject food in foodObjects)
        {
            Destroy(food);
        }

        gamePanel.SetActive(false);
        startButton.SetActive(true);
    }
}