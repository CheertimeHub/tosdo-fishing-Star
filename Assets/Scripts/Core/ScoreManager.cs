using UnityEngine;
using UnityEngine.Events;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int CurrentScore { get; private set; }
    public int BestScore    { get; private set; }

    public UnityEvent<int> OnScoreChanged = new();

    const string BestScoreKey = "BestScore";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
    }

    void OnEnable() => GameManager.Instance?.OnGameOver.AddListener(SaveBestScore);
    void OnDisable() => GameManager.Instance?.OnGameOver.RemoveListener(SaveBestScore);

    public void AddScore(int amount)
    {
        CurrentScore = Mathf.Max(0, CurrentScore + amount);
        OnScoreChanged.Invoke(CurrentScore);
    }

    void SaveBestScore()
    {
        if (CurrentScore > BestScore)
        {
            BestScore = CurrentScore;
            PlayerPrefs.SetInt(BestScoreKey, BestScore);
        }
    }
}
