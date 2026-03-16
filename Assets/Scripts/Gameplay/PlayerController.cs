using UnityEngine;

/// <summary>
/// ตัวละครผู้เล่นที่นั่งอยู่บนผิวน้ำ — มี bobbing เล็กน้อย
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Bobbing")]
    public float bobAmplitude = 0.08f;
    public float bobSpeed     = 1.5f;

    [Header("Flip when casting")]
    public SpriteRenderer spriteRenderer;

    Vector3 _basePos;
    FishingController _fishing;

    void Start()
    {
        _basePos = transform.position;
        _fishing = GetComponentInChildren<FishingController>();
    }

    void Update()
    {
        // Bobbing ขึ้นลงนิดหน่อยเหมือนลอยน้ำ
        float y = _basePos.y + Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        transform.position = new Vector3(_basePos.x, y, _basePos.z);
    }
}
