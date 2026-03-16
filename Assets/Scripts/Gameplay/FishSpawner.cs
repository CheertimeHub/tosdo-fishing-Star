using System.Collections;
using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    [Header("Fish Types")]
    public FishData[] fishTypes;          // ลาก ScriptableObject ใส่ใน Inspector

    [Header("Spawn Settings")]
    public float spawnInterval   = 2f;
    public float minDepth        = -1.5f; // Y ตื้นสุด (ใต้น้ำ)
    public float maxDepth        = -5f;   // Y ลึกสุด
    public float spawnEdgeX      = 11f;   // ระยะขอบจอ

    [Header("Prefab")]
    public GameObject fishPrefab;         // Prefab ที่มี SpriteRenderer + Collider2D + Fish.cs

    void Start() => StartCoroutine(SpawnLoop());

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (GameManager.Instance.CurrentState != GameState.Playing) continue;
            if (fishTypes == null || fishTypes.Length == 0) continue;

            SpawnFish();
        }
    }

    void SpawnFish()
    {
        var data  = fishTypes[Random.Range(0, fishTypes.Length)];
        float dir = Random.value > 0.5f ? 1f : -1f;        // ทิศทางสุ่ม
        float spawnX = -dir * spawnEdgeX;
        float spawnY = Random.Range(maxDepth, minDepth);

        var go   = Instantiate(fishPrefab, Vector3.zero, Quaternion.identity);
        var fish = go.GetComponent<Fish>();
        fish.Init(data, spawnX, spawnY, dir);
    }
}
