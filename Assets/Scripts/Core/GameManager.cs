using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Settings")]
    public float gameDuration = 90f;

    public GameState CurrentState { get; private set; } = GameState.Menu;
    public float TimeRemaining { get; private set; }

    [HideInInspector] public UnityEvent OnGameStart = new();
    [HideInInspector] public UnityEvent OnGameOver  = new();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start() => StartGame();

    public void StartGame()
    {
        TimeRemaining = gameDuration;
        CurrentState  = GameState.Playing;
        OnGameStart.Invoke();
    }

    void Update()
    {
        if (CurrentState != GameState.Playing) return;

        TimeRemaining -= Time.deltaTime;
        if (TimeRemaining <= 0f)
        {
            TimeRemaining = 0f;
            TriggerGameOver();
        }
    }

    public void TriggerGameOver()
    {
        if (CurrentState == GameState.GameOver) return;
        CurrentState = GameState.GameOver;
        OnGameOver.Invoke();
        Debug.Log($"Game Over! Score: {ScoreManager.Instance.CurrentScore}");
    }
}

public enum GameState { Menu, Playing, GameOver }
