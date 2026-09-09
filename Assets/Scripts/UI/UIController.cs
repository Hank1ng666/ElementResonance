// ─── 文件位置: Assets/Scripts/UI/UIController.cs
// ─── 挂载对象: Canvas
// ─── 说明: HUD每帧轮询刷新（GDD架构：10行轮询胜过100行事件系统）。
//           面板开关归 GameManager（胜负）和 UpgradeManager（三选一），这里只管HUD和提示。

using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("连接（拖拽）")]
    public PlayerStats playerStats;
    public GameManager gameManager;

    [Header("HUD组件")]
    public Slider hpSlider;
    public Slider expSlider;
    public Text levelText;
    public Text timerText;
    public Text killText;
    public Text toastText;      // 屏幕中央的共鸣提示

    [Header("Boss血条（Day 7/8）")]
    public GameObject bossHpBar;
    public Slider bossHpSlider;

    float toastTimer;

    void Update()
    {
        if (gameManager != null)
        {
            if (timerText != null) timerText.text = gameManager.FormatTime(gameManager.gameTime);
            if (killText != null) killText.text = "击杀 " + gameManager.kills;
        }

        if (playerStats != null)
        {
            if (hpSlider != null)
            {
                hpSlider.maxValue = playerStats.maxHP;
                hpSlider.value = playerStats.currentHP;
            }
            if (expSlider != null)
            {
                expSlider.maxValue = playerStats.XPToNext();
                expSlider.value = playerStats.currentEXP;
            }
            if (levelText != null) levelText.text = "Lv." + playerStats.level;
        }

        // 共鸣提示淡出（用unscaled时间，暂停时也能正常消失）
        if (toastTimer > 0f)
        {
            toastTimer -= Time.unscaledDeltaTime;
            if (toastTimer <= 0f && toastText != null) toastText.text = "";
        }

        UpdateBossBar();
    }

    void UpdateBossBar()
    {
        if (bossHpBar == null || gameManager == null) return;
        Boss b = gameManager.currentBoss;
        if (b != null && !b.isDead)
        {
            bossHpBar.SetActive(true);
            if (bossHpSlider != null)
            {
                bossHpSlider.maxValue = b.maxHP;
                bossHpSlider.value = b.hp;
            }
        }
        else
        {
            bossHpBar.SetActive(false);
        }
    }

    // PlayerStats 共鸣触发时调用
    public void ShowToast(string msg)
    {
        if (toastText != null)
        {
            toastText.text = msg;
            toastTimer = 2f;
        }
    }
}
