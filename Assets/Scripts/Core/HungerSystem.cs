using UnityEngine;
using UnityEngine.Events;

public class HungerSystem : MonoBehaviour
{
    public static HungerSystem Instance { get; private set; }

    [Header("Settings")]
    [Range(0f, 100f)] public float maxHunger = 100f;
    [Range(0f, 100f)] public float startHunger = 70f;
    public float hungerDecayPerSecond = 5f;   // ลดต่อวินาที

    public float CurrentHunger { get; private set; }
    public float HungerPercent => CurrentHunger / maxHunger;

    public UnityEvent<float> OnHungerChanged = new();   // ส่ง 0-1
    public UnityEvent        OnStarved       = new();   // หิวจนตาย

    bool _starved;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        CurrentHunger = startHunger;
        OnHungerChanged.Invoke(HungerPercent);
    }

    void Update()
    {
        if (_starved) return;
        if (GameManager.Instance.CurrentState != GameState.Playing) return;

        CurrentHunger -= hungerDecayPerSecond * Time.deltaTime;
        CurrentHunger  = Mathf.Clamp(CurrentHunger, 0f, maxHunger);

        OnHungerChanged.Invoke(HungerPercent);

        if (CurrentHunger <= 0f)
        {
            _starved = true;
            OnStarved.Invoke();
            GameManager.Instance.TriggerGameOver();
        }
    }

    // เรียกเมื่อจับปลาได้
    public void Restore(float amount)
    {
        CurrentHunger = Mathf.Min(CurrentHunger + amount, maxHunger);
        OnHungerChanged.Invoke(HungerPercent);
    }
}
