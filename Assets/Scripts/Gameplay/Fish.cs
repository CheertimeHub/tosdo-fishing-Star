using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Fish : MonoBehaviour
{
    public FishData Data { get; private set; }

    // (score, worldPosition) — CatchEffectSpawner ฟัง event นี้
    public static event Action<int, Vector3> OnCaught;

    float _dir;          // +1 ว่ายขวา, -1 ว่ายซ้าย
    bool  _caught;

    static readonly float OffscreenX = 12f;

    public void Init(FishData data, float spawnX, float spawnY, float dir)
    {
        Data  = data;
        _dir  = dir;

        transform.localScale = new Vector3(data.size * (dir < 0 ? -1 : 1), data.size, 1f);
        transform.position   = new Vector3(spawnX, spawnY, 0f);

        // Placeholder visual — วงกลมสี
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = data.placeholderColor;
    }

    void Update()
    {
        if (_caught) return;
        transform.Translate(Vector3.right * (_dir * Data.moveSpeed * Time.deltaTime));

        // ออกนอกจอ → ทำลายตัวเอง
        if (Mathf.Abs(transform.position.x) > OffscreenX)
            Destroy(gameObject);
    }

    // เรียกจาก Hook
    public void GetCaught()
    {
        if (_caught) return;
        _caught = true;

        if (!Data.isJunk)
        {
            ScoreManager.Instance.AddScore(Data.scoreValue);
            HungerSystem.Instance.Restore(Data.hungerRestore);
        }
        else
        {
            ScoreManager.Instance.AddScore(Data.scoreValue); // ค่าติดลบ
        }

        OnCaught?.Invoke(Data.scoreValue, transform.position);
        Destroy(gameObject);
    }
}
