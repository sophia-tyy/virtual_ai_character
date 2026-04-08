using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class GameController : MonoBehaviour
{
    [Header("Player Snake Settings")]
    public Transform playerSnakeHeadPrefab;
    public Transform playerSnakeParent;
    private PlayerSnakeController playerSnakeController;

    [Header("AI Snake Settings")]
    public Transform AISnakeHeadPrefab;
    public Transform AISnakeParent;
    private AISnakeController AISnakeController;

    [Header("Game Environment Settings")]
    public Transform foodPrefab;
    public Transform foodParent;

    public Transform borderLeft;
    public Transform borderRight;
    public Transform borderTop;
    public Transform borderBottom;

    [Header("UI & Events")]
    public GameObject gamePanel;
    public GameObject startPanel;

    public TMP_Text highestScoreText;
    public TMP_Text currentScoreText;

    public UnityEvent onGameEnd;

    void Start()
    {
        UpdateDisplayScores();
    }

    public void UpdateDisplayScores()
    {
        int HighestScore = GameDataManager.Instance.GetHighestScore();
        int CurrentScore = GameDataManager.Instance.GetCurrentScore();

        highestScoreText.text = HighestScore.ToString();
        currentScoreText.text = CurrentScore.ToString();
    }

    public void StartGame_Player_AI()
    {
        float x = (borderLeft.position.x + borderRight.position.x) / 2;
        float y = (borderTop.position.y + borderBottom.position.y) / 2;

        Instantiate(playerSnakeHeadPrefab, new Vector2(x - 2, y), Quaternion.identity, playerSnakeParent);
        playerSnakeController = FindObjectOfType<PlayerSnakeController>();
        if (playerSnakeController == null) Debug.LogError("PlayerSnakeController not found!");

        Instantiate(AISnakeHeadPrefab, new Vector2(x + 2, y), Quaternion.identity, AISnakeParent);
        AISnakeController = FindObjectOfType<AISnakeController>();
        if (AISnakeController == null) Debug.LogError("AISnakeController not found!");

        Spawn();
    }

    public void StartGame_Player()
    {
        float x = (borderLeft.position.x + borderRight.position.x) / 2;
        float y = (borderTop.position.y + borderBottom.position.y) / 2;

        Instantiate(playerSnakeHeadPrefab, new Vector2(x, y), Quaternion.identity, playerSnakeParent);
        playerSnakeController = FindObjectOfType<PlayerSnakeController>();
        if (playerSnakeController == null) Debug.LogError("PlayerSnakeController not found!");

        Spawn();
    }

    public void StartGame_AI()
    {
        float x = (borderLeft.position.x + borderRight.position.x) / 2;
        float y = (borderTop.position.y + borderBottom.position.y) / 2;

        Transform AISnake = Instantiate(AISnakeHeadPrefab, new Vector2(x, y), Quaternion.identity, AISnakeParent);
        AISnakeController = FindObjectOfType<AISnakeController>();
        if (AISnakeController == null) Debug.LogError("AISnakeController not found!");
        AISnake.GetComponent<SnakeAgent>().ChangeToSoloMode();

        Spawn();
    }

    public void Spawn()
    {
        float x = Random.Range(borderLeft.position.x + 0.2f, borderRight.position.x - 0.2f);
        float y = Random.Range(borderTop.position.y - 0.2f, borderBottom.position.y + 0.2f);
        Instantiate(foodPrefab, new Vector2(x, y), Quaternion.identity, foodParent);
    }

    public void TurnRight() {  playerSnakeController.ChangeDirection(1); }
    public void TurnLeft() {  playerSnakeController.ChangeDirection(2); }
    public void TurnUp() {  playerSnakeController.ChangeDirection(3); }
    public void TurnDown() {  playerSnakeController.ChangeDirection(4); }

    public void EndGame()
    {
        string temp = "";
        if (AISnakeController != null) AISnakeController.GetComponent<SnakeAgent>().HandleDied(temp);
        
        GameObject[] snakeObjects = GameObject.FindGameObjectsWithTag("Snake");
        foreach (GameObject snake in snakeObjects) Destroy(snake);

        GameObject[] foodObjects = GameObject.FindGameObjectsWithTag("Food");
        foreach (GameObject food in foodObjects) Destroy(food);

        gamePanel.SetActive(false);
        startPanel.SetActive(true);

        AffectionDataManager.Instance.FinishTask("Play Mini Game Once");
        onGameEnd.Invoke();
    }
}