using UnityEngine;

/// <summary>
/// ใส่ไว้ใน GameManager object — spawn ScorePopup ทุกครั้งที่จับปลาได้
/// ต้องส่ง event มาจาก Fish.GetCaught() ผ่าน static event
/// </summary>
public class CatchEffectSpawner : MonoBehaviour
{
    public GameObject scorePopupPrefab;

    public Color positiveColor = Color.yellow;
    public Color negativeColor = Color.red;

    void OnEnable()  => Fish.OnCaught += SpawnPopup;
    void OnDisable() => Fish.OnCaught -= SpawnPopup;

    void SpawnPopup(int score, Vector3 worldPos)
    {
        if (scorePopupPrefab == null) return;

        var go    = Instantiate(scorePopupPrefab, worldPos + Vector3.up * 0.3f, Quaternion.identity);
        var popup = go.GetComponent<ScorePopup>();
        popup?.Play(score, score >= 0 ? positiveColor : negativeColor);
    }
}
