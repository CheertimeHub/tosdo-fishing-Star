using UnityEngine;

[CreateAssetMenu(fileName = "NewFish", menuName = "Fishing/Fish Data")]
public class FishData : ScriptableObject
{
    public string fishName = "Fish";
    public int scoreValue = 100;
    public float hungerRestore = 20f;   // % ที่เติม hunger gauge
    public float moveSpeed = 2f;
    public float size = 1f;
    public Color placeholderColor = Color.yellow;
    public bool isJunk = false;          // กระป๋อง/ขยะ → ลดแต้ม
}
