using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Hook : MonoBehaviour
{
    // เรียกจาก FishingController เพื่อเปิด/ปิด trigger
    public bool IsActive { get; private set; }

    public void SetActive(bool active)
    {
        IsActive = active;
        GetComponent<Collider2D>().enabled = active;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsActive) return;

        var fish = other.GetComponent<Fish>();
        if (fish != null)
            fish.GetCaught();
    }
}
