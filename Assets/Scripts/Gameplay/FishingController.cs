using UnityEngine;

/// <summary>
/// กลไกตกปลา:
///   กด   → สายเบ็ดยาวลงเรื่อยๆ (ลึกขึ้น)
///   ปล่อย → hook หยุดอยู่ที่ความลึกนั้น รอ waitTime วินาที แล้วดึงขึ้น
/// </summary>
public class FishingController : MonoBehaviour
{
    [Header("References")]
    public Transform   rodTip;       // จุดที่สายเบ็ดออกมา (position ของปลาย rod)
    public Transform   hookTransform;
    public Hook        hook;
    public LineRenderer fishingLine;

    [Header("Settings")]
    public float descentSpeed = 3f;   // ความเร็วที่ hook ลง (units/sec)
    public float ascentSpeed  = 5f;   // ความเร็วที่ hook ขึ้น
    public float maxDepth     = -6f;  // ลึกสุดที่ hook ไปได้
    public float waitAtDepth  = 0.5f; // รอกี่วินาทีก่อนดึงขึ้น

    enum HookState { Idle, Descending, Waiting, Ascending }

    HookState _state = HookState.Idle;
    float     _waitTimer;
    Vector3   _hookOrigin;  // ตำแหน่งเริ่มต้น (rodTip)

    void Start()
    {
        _hookOrigin = rodTip.position;
        hookTransform.position = _hookOrigin;
        hook.SetActive(false);
        UpdateLine();
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameState.Playing) return;

        switch (_state)
        {
            case HookState.Idle:
                if (Input.GetMouseButton(0))
                    StartDescend();
                break;

            case HookState.Descending:
                Descend();
                if (!Input.GetMouseButton(0) || hookTransform.position.y <= maxDepth)
                    StartWait();
                break;

            case HookState.Waiting:
                _waitTimer -= Time.deltaTime;
                if (_waitTimer <= 0f)
                    StartAscend();
                break;

            case HookState.Ascending:
                Ascend();
                if (hookTransform.position.y >= _hookOrigin.y)
                    FinishAscend();
                break;
        }

        UpdateLine();
    }

    // ── State transitions ────────────────────────────────────────────────────

    void StartDescend()
    {
        _state = HookState.Descending;
        hook.SetActive(true);
    }

    void Descend()
    {
        var pos = hookTransform.position;
        pos.y = Mathf.Max(pos.y - descentSpeed * Time.deltaTime, maxDepth);
        hookTransform.position = pos;
    }

    void StartWait()
    {
        _state      = HookState.Waiting;
        _waitTimer  = waitAtDepth;
    }

    void StartAscend()
    {
        _state = HookState.Ascending;
        hook.SetActive(false);   // ปิด trigger ตอนดึงขึ้น
    }

    void Ascend()
    {
        var pos = hookTransform.position;
        pos.y = Mathf.Min(pos.y + ascentSpeed * Time.deltaTime, _hookOrigin.y);
        hookTransform.position = pos;
    }

    void FinishAscend()
    {
        hookTransform.position = _hookOrigin;
        _state = HookState.Idle;
    }

    // ── Visual ───────────────────────────────────────────────────────────────

    void UpdateLine()
    {
        if (fishingLine == null) return;
        fishingLine.SetPosition(0, rodTip.position);
        fishingLine.SetPosition(1, hookTransform.position);
    }
}
