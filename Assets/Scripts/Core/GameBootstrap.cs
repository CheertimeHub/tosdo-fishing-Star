using UnityEngine;

/// <summary>
/// สร้าง placeholder objects ทั้งหมดใน scene แบบ runtime
/// ใช้สำหรับ prototype/test — ไม่ต้องเซ็ต scene ด้วยมือ
///
/// วิธีใช้: สร้าง Empty GameObject แล้วใส่ script นี้
///          กด Play ได้เลย
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    [Header("Auto-create scene?")]
    public bool autoSetup = true;

    void Awake()
    {
        if (!autoSetup) return;
        SetupManagers();
        SetupCamera();
        SetupPlayer();
        SetupFishSpawner();
        SetupUI();
    }

    // ── Managers ─────────────────────────────────────────────────────────────

    void SetupManagers()
    {
        var root = new GameObject("--- Managers ---");

        root.AddComponent<GameManager>().gameDuration = 90f;
        root.AddComponent<ScoreManager>();

        var hunger = root.AddComponent<HungerSystem>();
        hunger.startHunger        = 70f;
        hunger.hungerDecayPerSecond = 5f;
    }

    // ── Camera ───────────────────────────────────────────────────────────────

    void SetupCamera()
    {
        Camera.main.backgroundColor = new Color(0.2f, 0.5f, 0.7f);
        Camera.main.orthographicSize = 5f;
    }

    // ── Player ───────────────────────────────────────────────────────────────

    void SetupPlayer()
    {
        // Body (placeholder สี่เหลี่ยมแดง)
        var player = CreatePlaceholder("Player", Color.red, 0.4f, 0.5f);
        player.transform.position = new Vector3(-5f, 0.3f, 0f);
        player.AddComponent<PlayerController>();

        // Rod tip
        var rodTip = new GameObject("RodTip");
        rodTip.transform.SetParent(player.transform);
        rodTip.transform.localPosition = new Vector3(0.3f, 0.2f, 0f);

        // Hook
        var hookGo = CreatePlaceholder("Hook", Color.white, 0.15f, 0.15f);
        hookGo.transform.SetParent(player.transform);
        hookGo.transform.localPosition = new Vector3(0.3f, 0.2f, 0f);
        var col = hookGo.AddComponent<CircleCollider2D>();
        col.radius    = 0.15f;
        col.isTrigger = true;
        var hookComp = hookGo.AddComponent<Hook>();

        // Fishing Line
        var lineGo   = new GameObject("FishingLine");
        lineGo.transform.SetParent(player.transform);
        var lr       = lineGo.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.startWidth    = 0.02f;
        lr.endWidth      = 0.02f;
        lr.material      = new Material(Shader.Find("Sprites/Default"));
        lr.startColor    = Color.white;
        lr.endColor      = Color.white;

        // FishingController
        var fc           = player.AddComponent<FishingController>();
        fc.rodTip        = rodTip.transform;
        fc.hookTransform = hookGo.transform;
        fc.hook          = hookComp;
        fc.fishingLine   = lr;
        fc.maxDepth      = -5.5f;
    }

    // ── Fish Spawner ─────────────────────────────────────────────────────────

    void SetupFishSpawner()
    {
        var go      = new GameObject("FishSpawner");
        var spawner = go.AddComponent<FishSpawner>();

        spawner.fishPrefab     = BuildFishPrefab();
        spawner.fishTypes      = BuildFishDataArray();
        spawner.spawnInterval  = 1.8f;
        spawner.minDepth       = -1.2f;
        spawner.maxDepth       = -5f;
    }

    GameObject BuildFishPrefab()
    {
        var prefab = new GameObject("FishPrefab_Runtime");
        prefab.SetActive(false);   // ไม่ active จนกว่าจะ Instantiate

        var sr            = prefab.AddComponent<SpriteRenderer>();
        sr.sprite         = CreateCircleSprite();

        var col           = prefab.AddComponent<CircleCollider2D>();
        col.radius        = 0.4f;
        col.isTrigger     = true;

        prefab.AddComponent<Fish>();
        return prefab;
    }

    FishData[] BuildFishDataArray()
    {
        return new FishData[]
        {
            MakeFishData("ปลาทอง",    100,  15f, 2.5f, 0.7f, new Color(1f,0.8f,0f),   false),
            MakeFishData("ปลาใหญ่",   500,  35f, 1.2f, 1.4f, new Color(0.4f,0.8f,1f),  false),
            MakeFishData("ปลาเร็ว",   200,  10f, 4.0f, 0.6f, new Color(0.6f,1f,0.6f),  false),
            MakeFishData("หม้อโอเดน",2000,  60f, 0.8f, 1.8f, new Color(0.9f,0.6f,0.3f),false),
            MakeFishData("กระป๋อง",  -150,   0f, 1.5f, 0.7f, new Color(0.5f,0.5f,0.5f),true),
        };
    }

    FishData MakeFishData(string name, int score, float hunger,
                          float speed, float size, Color color, bool isJunk)
    {
        var d               = ScriptableObject.CreateInstance<FishData>();
        d.fishName          = name;
        d.scoreValue        = score;
        d.hungerRestore     = hunger;
        d.moveSpeed         = speed;
        d.size              = size;
        d.placeholderColor  = color;
        d.isJunk            = isJunk;
        return d;
    }

    // ── UI ───────────────────────────────────────────────────────────────────

    void SetupUI()
    {
        // UI ง่ายๆ ใช้ OnGUI แทน Canvas ตอน prototype
        var go = new GameObject("DebugUI");
        go.AddComponent<DebugHUD>();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    GameObject CreatePlaceholder(string name, Color color, float w, float h)
    {
        var go = new GameObject(name);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite(color, w, h);
        return go;
    }

    Sprite CreateSquareSprite(Color color, float w, float h)
    {
        int texW = Mathf.RoundToInt(w * 64);
        int texH = Mathf.RoundToInt(h * 64);
        var tex  = new Texture2D(texW, texH);
        var pixels = new Color[texW * texH];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, texW, texH), new Vector2(0.5f, 0.5f), 64f);
    }

    Sprite CreateCircleSprite()
    {
        int size = 64;
        var tex  = new Texture2D(size, size);
        var pixels = new Color[size * size];
        float r = size / 2f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = x - r, dy = y - r;
            pixels[y * size + x] = (dx * dx + dy * dy) < r * r ? Color.white : Color.clear;
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64f);
    }
}
