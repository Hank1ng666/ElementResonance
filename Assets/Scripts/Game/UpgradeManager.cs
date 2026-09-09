// ─── 文件位置: Assets/Scripts/Game/UpgradeManager.cs
// ─── 挂载对象: GameManager
// ─── 说明: 升级三选一。强化池在Inspector里填10条（数值=GDD 9.1 / Excel Upgrade表）。
//           流程: 玩家升级 → timeScale=0 弹面板 → 点按钮 → 应用效果 → 恢复timeScale。
//           连续升级会连续弹面板（pendingLevelUps 队列）。

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 一条强化数据 = Excel Upgrade表的一行
[System.Serializable]
public class UpgradeData
{
    public string upgradeName;
    [TextArea] public string description;
    public ElementType element = ElementType.None;

    public EffectType effect1Type = EffectType.None;
    public float effect1Value;
    public EffectType effect2Type = EffectType.None;   // 没有第二效果就填 None
    public float effect2Value;

    public int maxCount = 99;                          // 99=无限重复, 1=唯一, 2=最多2次
    public WeaponType requiresWeapon = WeaponType.None;// 前置武器（旋刃强化要已拥有旋刃）
    public WeaponType grantWeapon = WeaponType.None;   // GrantWeapon 类型用：指定给哪把武器
    [HideInInspector] public int currentCount = 0;
}

public class UpgradeManager : MonoBehaviour
{
    [Header("强化池（按GDD 9.1填10条）")]
    public List<UpgradeData> upgrades = new List<UpgradeData>();

    [Header("连接（拖拽）")]
    public PlayerStats playerStats;
    public GameObject upgradePanel;      // 升级面板（初始 inactive）
    public Button[] optionButtons;       // 3个按钮
    public Text[] optionTexts;           // 每个按钮下的Text子物体（显示 名称+描述）

    List<UpgradeData> currentChoices = new List<UpgradeData>();

    void Start()
    {
        // 没拖 playerStats 就按Tag找一次（全项目唯一允许的Tag查找）
        if (playerStats == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerStats = p.GetComponent<PlayerStats>();
        }
        if (playerStats != null) playerStats.upgradeManager = this;

        if (optionButtons != null)
        {
            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (optionButtons[i] == null) continue;
                int index = i; // 闭包捕获
                optionButtons[i].onClick.AddListener(() => ChooseUpgrade(index));
            }
        }

        if (upgradePanel != null) upgradePanel.SetActive(false);
    }

    // PlayerStats 升级时调用
    public void OnLevelUp()
    {
        if (upgradePanel != null && upgradePanel.activeSelf) return; // 已经在弹了
        ShowChoices();
    }

    void ShowChoices()
    {
        if (playerStats == null || optionButtons == null || optionButtons.Length == 0 || optionTexts == null)
        {
            Debug.LogWarning("UpgradeManager 未配置好(检查 playerStats/optionButtons/optionTexts)，跳过本次升级");
            if (playerStats != null) playerStats.pendingLevelUps = 0;
            Time.timeScale = 1f;
            return;
        }

        Time.timeScale = 0f; // 暂停游戏

        List<UpgradeData> available = GetAvailable();
        if (available.Count == 0)
        {
            playerStats.pendingLevelUps = 0;
            Time.timeScale = 1f;
            return;
        }

        // 随机抽3个不重复
        currentChoices.Clear();
        for (int i = 0; i < 3 && available.Count > 0; i++)
        {
            int idx = Random.Range(0, available.Count);
            currentChoices.Add(available[idx]);
            available.RemoveAt(idx);
        }

        // 填UI
        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (i < currentChoices.Count)
            {
                optionButtons[i].gameObject.SetActive(true);
                if (optionTexts[i] != null)
                    optionTexts[i].text = currentChoices[i].upgradeName + "\n" + currentChoices[i].description;
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
            }
        }

        if (upgradePanel != null) upgradePanel.SetActive(true);
    }

    // 当前可出现的强化 = 没到重复上限 + 满足前置武器
    List<UpgradeData> GetAvailable()
    {
        List<UpgradeData> result = new List<UpgradeData>();
        for (int i = 0; i < upgrades.Count; i++)
        {
            UpgradeData u = upgrades[i];
            if (u.currentCount >= u.maxCount) continue;
            if (u.requiresWeapon == WeaponType.Orbit && !playerStats.hasOrbitWeapon) continue;
            if (u.requiresWeapon == WeaponType.Nova && !playerStats.hasNovaWeapon) continue;
            result.Add(u);
        }
        return result;
    }

    // 按钮点击（Start里已自动绑定 0/1/2）
    public void ChooseUpgrade(int index)
    {
        if (index < 0 || index >= currentChoices.Count) return;

        ApplyUpgrade(currentChoices[index]);
        playerStats.pendingLevelUps--;

        if (playerStats.pendingLevelUps > 0)
        {
            ShowChoices(); // 还有排队的升级，继续弹
        }
        else
        {
            if (upgradePanel != null) upgradePanel.SetActive(false);
            Time.timeScale = 1f; // ⚠ 与暂停成对出现
        }
    }

    void ApplyUpgrade(UpgradeData u)
    {
        u.currentCount++;
        ApplyEffect(u, u.effect1Type, u.effect1Value);
        ApplyEffect(u, u.effect2Type, u.effect2Value);
        if (u.element != ElementType.None) playerStats.AddElementStack(u.element);
        Debug.Log("选择强化: " + u.upgradeName);
    }

    void ApplyEffect(UpgradeData u, EffectType type, float value)
    {
        switch (type)
        {
            case EffectType.DamagePercent:
                playerStats.damageMultiplier += value;
                break;
            case EffectType.AttackIntervalReduce:
                playerStats.attackIntervalMultiplier *= (1f - value); // 乘法叠加(默认解释, 见Word 27.3)
                break;
            case EffectType.MoveSpeedPercent:
                playerStats.moveSpeedMultiplier += value;
                break;
            case EffectType.PickupRadiusPercent:
                playerStats.pickupRadius *= (1f + value);
                break;
            case EffectType.CritChancePercent:
                playerStats.critChance += value;
                break;
            case EffectType.AddProjectile:
                playerStats.extraProjectiles += Mathf.RoundToInt(value);
                break;
            case EffectType.GrantWeapon:
                playerStats.EnableWeapon(u.grantWeapon);
                break;
            case EffectType.WeaponDamagePercent:
                if (u.requiresWeapon == WeaponType.Orbit) playerStats.orbitDamageMultiplier += value;
                else if (u.requiresWeapon == WeaponType.Nova) playerStats.novaDamageMultiplier += value;
                break;
            case EffectType.OrbitSpeedPercent:
                playerStats.orbitSpeedMultiplier += value;
                break;
            case EffectType.AoeRadiusPercent:
                if (u.requiresWeapon == WeaponType.Nova) playerStats.novaRadiusMultiplier += value;
                break;
            case EffectType.MaxHPFlat:
                playerStats.maxHP += Mathf.RoundToInt(value);
                break;
            case EffectType.HealHPFlat:
                playerStats.Heal(Mathf.RoundToInt(value));
                break;
        }
    }
}
