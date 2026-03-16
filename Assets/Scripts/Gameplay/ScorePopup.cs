using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// ตัวเลขลอยขึ้นเมื่อจับปลาได้  — Prefab: TextMeshPro + ScorePopup
/// </summary>
public class ScorePopup : MonoBehaviour
{
    public TextMeshPro label;
    public float floatSpeed  = 1.5f;
    public float fadeDuration = 0.8f;

    public void Play(int score, Color color)
    {
        label.text  = score > 0 ? $"+{score}" : score.ToString();
        label.color = color;
        StartCoroutine(Animate());
    }

    IEnumerator Animate()
    {
        float elapsed = 0f;
        Color startColor = label.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            transform.position += Vector3.up * floatSpeed * Time.deltaTime;
            label.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);

            yield return null;
        }

        Destroy(gameObject);
    }
}
