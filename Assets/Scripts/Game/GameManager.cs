// ─── 文件位置: Assets/Scripts/Game/GameManager.cs
// ─── 挂载对象: GameManager（场景物体，通常和 EnemySpawner / UpgradeManager 挂一起）
// ─── 说明: 游戏时间、状态、击杀数、胜负结算、重开。
//           ⚠ Restart 里必须把 timeScale 设回 1，否则重开后游戏永远暂停！

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameState state = GameState.Playing;
    [HideInInspector] public float gameTime = 0f;
    [HideInInspector] public int kills = 0;
    [HideInInspector] public Boss currentBoss;   // Boss生成/死亡时由Spawner和Boss维护

    [Header("连接（拖拽，Day 8 时再填也行）")]
    public GameObject gameOverPanel;
    public GameObject victoryPanel;

    [Header("音效（Day 9，可不填）")]
    public AudioClip gameOverClip;
    public AudioClip victoryClip;

    void Update()
    {
        if (state == GameState.Playing) gameTime += Time.deltaTime;
    }

    public void AddKill()
    {
        kills++;
    }

    public void GameOver()
    {
        if (state != GameState.Playing) return;
        state = GameState.GameOver;
        Time.timeScale = 0f;
        Debug.Log("游戏结束！存活 " + FormatTime(gameTime) + "，击杀 " + kills);
        if (gameOverClip != null) AudioSource.PlayClipAtPoint(gameOverClip, transform.position);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void Victory()
    {
        if (state != GameState.Playing) return;
        state = GameState.Victory;
        Time.timeScale = 0f;
        Debug.Log("胜利！用时 " + FormatTime(gameTime) + "，击杀 " + kills);
        if (victoryClip != null) AudioSource.PlayClipAtPoint(victoryClip, transform.position);
        if (victoryPanel != null) victoryPanel.SetActive(true);
    }

    // 结算面板的"重新开始"按钮直接绑这个方法（Button.OnClick → GameManager.Restart）
    public void Restart()
    {
        Time.timeScale = 1f;   // ⚠ 必须恢复，否则重开后永远暂停
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public string FormatTime(float t)
    {
        int m = (int)(t / 60f);
        int s = (int)(t % 60f);
        return m.ToString("00") + ":" + s.ToString("00");
    }
}
