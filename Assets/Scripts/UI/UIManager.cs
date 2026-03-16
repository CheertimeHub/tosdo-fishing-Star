using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Score & Timer")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;

    [Header("Hunger Gauge")]
    public Slider  hungerSlider;
    public Image   hungerFill;       // Image component ของ Fill
    public Color   fullColor   = Color.green;
    public Color   dangerColor = Color.red;

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI bestScoreText;

    void Start()
    {
        gameOverPanel.SetActive(false);

        ScoreManager.Instance.OnScoreChanged.AddListener(UpdateScore);
        HungerSystem.Instance.OnHungerChanged.AddListener(UpdateHunger);
        GameManager.Instance.OnGameOver.AddListener(ShowGameOver);

        UpdateScore(0);
        UpdateHunger(HungerSystem.Instance.HungerPercent);
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameState.Playing) return;
        float t = GameManager.Instance.TimeRemaining;
        timerText.text = $"{Mathf.CeilToInt(t):D2}";
    }

    void UpdateScore(int score)
    {
        scoreText.text = score.ToString("N0");
    }

    void UpdateHunger(float percent)
    {
        if (hungerSlider != null)
            hungerSlider.value = percent;

        if (hungerFill != null)
            hungerFill.color = Color.Lerp(dangerColor, fullColor, percent);
    }

    void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
        finalScoreText.text = $"Score: {ScoreManager.Instance.CurrentScore:N0}";
        bestScoreText.text  = $"Best:  {ScoreManager.Instance.BestScore:N0}";
    }
}
