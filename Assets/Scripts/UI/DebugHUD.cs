using UnityEngine;

/// <summary>
/// HUD แบบ OnGUI สำหรับ prototype — ไม่ต้องตั้ง Canvas
/// </summary>
public class DebugHUD : MonoBehaviour
{
    GUIStyle _bigStyle;
    GUIStyle _barStyle;

    void OnGUI()
    {
        if (_bigStyle == null) InitStyles();

        var gm = GameManager.Instance;
        var sm = ScoreManager.Instance;
        var hs = HungerSystem.Instance;

        if (gm == null) return;

        // ── Score ─────────────────────────────────────────
        GUI.Label(new Rect(10, 10, 300, 50), $"SCORE  {sm.CurrentScore:N0}", _bigStyle);

        // ── Timer ─────────────────────────────────────────
        int sec = Mathf.CeilToInt(gm.TimeRemaining);
        GUI.Label(new Rect(10, 55, 200, 40), $"TIME   {sec:D2}", _bigStyle);

        // ── Hunger Bar ────────────────────────────────────
        float percent = hs != null ? hs.HungerPercent : 1f;
        GUI.Label(new Rect(10, 95, 150, 30), "HUNGER", _bigStyle);

        // background
        GUI.color = Color.gray;
        GUI.Box(new Rect(100, 98, 200, 20), GUIContent.none);

        // fill
        GUI.color = Color.Lerp(Color.red, Color.green, percent);
        GUI.Box(new Rect(100, 98, 200f * percent, 20), GUIContent.none);

        GUI.color = Color.white;

        // ── Game Over ─────────────────────────────────────
        if (gm.CurrentState == GameState.GameOver)
        {
            GUI.Label(new Rect(Screen.width / 2f - 150, Screen.height / 2f - 60, 300, 60),
                      "GAME OVER", _bigStyle);
            GUI.Label(new Rect(Screen.width / 2f - 150, Screen.height / 2f,     300, 40),
                      $"Score: {sm.CurrentScore:N0}   Best: {sm.BestScore:N0}", _bigStyle);

            if (GUI.Button(new Rect(Screen.width / 2f - 60, Screen.height / 2f + 50, 120, 35),
                           "Play Again"))
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

    void InitStyles()
    {
        _bigStyle           = new GUIStyle(GUI.skin.label);
        _bigStyle.fontSize  = 22;
        _bigStyle.fontStyle = FontStyle.Bold;
        _bigStyle.normal.textColor = Color.white;
    }
}
