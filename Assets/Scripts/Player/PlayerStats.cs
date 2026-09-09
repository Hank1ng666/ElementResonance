// ─── 文件位置: Assets/Scripts/Player/PlayerStats.cs
// ─── 挂载对象: Player
// ─── 说明: 玩家全部数值容器 + 受伤/死亡 + 经验升级 + 元素共鸣。
//           所有数值都在 Inspector 可调（与 Excel Player 表一致）。

using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("生命（Excel Player表）")]
    public int maxHP = 100;
    public int currentHP = 100;
    public float invincibleTime = 0.5f;   // 受伤后的无敌时间
    public int levelUpHeal = 15;          // 每次升级回复的HP

    [Header("移动与拾取")]
    public float baseMoveSpeed = 5f;
    public float moveSpeedMultiplier = 1f;   // 冰之精华叠加
    public float pickupRadius = 2.5f;        // 宝石吸附半径

    [Header("攻击属性（强化叠加）")]
    public float damageMultiplier = 1f;          // 火之精华叠加
    public float attackIntervalMultiplier = 1f;  // 雷之精华叠加（乘法）
    public float critChance = 0f;                // 鹰眼叠加，上限0.3
    public float critMultiplier = 2f;
    public int extraProjectiles = 0;             // 分裂飞刀叠加，最多2

    [Header("武器专属倍率（对应武器强化）")]
    public float orbitDamageMultiplier = 1f;
    public float orbitSpeedMultiplier = 1f;
    public float novaDamageMultiplier = 1f;
    public float novaRadiusMultiplier = 1f;

    [Header("经验与等级（Excel Balance表）")]
    public int level = 1;
    public int currentEXP = 0;
    public int xpBase = 5;        // 升级所需 = 5 + 3 × (等级-1)
    public int xpPerLevel = 3;
    [HideInInspector] public int pendingLevelUps = 0;

    [Header("元素计数与共鸣（同元素累计2个触发）")]
    public int fireStacks = 0;
    public int lightningStacks = 0;
    public int iceStacks = 0;
    public bool hasFireResonance = false;
    public bool hasLightningResonance = false;
    public bool hasIceResonance = false;

    [Header("共鸣数值（与Excel Skill表一致）")]
    public float burnDPS = 5f;             // 火: 每秒5点, 共3秒=15点
    public float burnDuration = 3f;
    public float chainProcChance = 0.3f;   // 雷: 30%概率
    public float chainDamageRatio = 0.5f;  // 雷: 50%伤害
    public float chainRange = 6f;          // 雷: 6米内
    public float slowRatio = 0.3f;         // 冰: 减速30%
    public float slowDuration = 2f;        // 冰: 2秒

    [Header("武器解锁状态（运行时）")]
    [HideInInspector] public bool hasOrbitWeapon = false;
    [HideInInspector] public bool hasNovaWeapon = false;
    [HideInInspector] public bool isDead = false;

    [Header("音效（Day 9，可不填）")]
    public AudioClip levelUpClip;
    public AudioClip resonanceClip;

    [Header("连接（从Hierarchy拖拽）")]
    public GameManager gameManager;        // 拖 GameManager
    public UpgradeManager upgradeManager;  // UpgradeManager.Start 会自动回填，可不拖
    public UIController uiController;      // 拖 Canvas 上的 UIController（Day 8）
    public GameObject orbitWeaponObject;   // 拖 Player 下的 Weapon_Orbit 子物体
    public GameObject novaWeaponObject;    // 拖 Player 下的 Weapon_Nova 子物体

    float invincibleTimer;

    void Awake()
    {
        currentHP = maxHP;
    }

    void Update()
    {
        if (invincibleTimer > 0f) invincibleTimer -= Time.deltaTime;
    }

    // 升到下一级所需经验
    public int XPToNext()
    {
        return xpBase + xpPerLevel * (level - 1);
    }

    public void GainEXP(int amount)
    {
        if (isDead) return;
        currentEXP += amount;
        while (currentEXP >= XPToNext())
        {
            currentEXP -= XPToNext();
            level++;
            pendingLevelUps++;
            Heal(levelUpHeal);
            Debug.Log("升级！当前等级 " + level + "，待选强化 " + pendingLevelUps);
            if (levelUpClip != null) AudioSource.PlayClipAtPoint(levelUpClip, transform.position);
        }
        if (pendingLevelUps > 0 && upgradeManager != null)
            upgradeManager.OnLevelUp();
    }

    public void TakeDamage(float damage)
    {
        if (isDead || invincibleTimer > 0f) return;
        currentHP -= Mathf.CeilToInt(damage);
        invincibleTimer = invincibleTime;
        if (currentHP <= 0) Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log("玩家死亡，游戏结束！存活击杀数见GameManager");
        if (gameManager != null) gameManager.GameOver();
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        currentHP = Mathf.Min(currentHP + amount, maxHP);
    }

    // 元素计数与共鸣触发（同元素累计2个 → 共鸣，只触发一次）
    public void AddElementStack(ElementType element)
    {
        if (element == ElementType.Fire) fireStacks++;
        else if (element == ElementType.Lightning) lightningStacks++;
        else if (element == ElementType.Ice) iceStacks++;

        if (!hasFireResonance && fireStacks >= 2)
        {
            hasFireResonance = true;
            Announce("火之共鸣！攻击点燃敌人");
        }
        if (!hasLightningResonance && lightningStacks >= 2)
        {
            hasLightningResonance = true;
            Announce("雷之共鸣！攻击概率连锁闪电");
        }
        if (!hasIceResonance && iceStacks >= 2)
        {
            hasIceResonance = true;
            Announce("冰之共鸣！攻击减速敌人");
        }
    }

    void Announce(string msg)
    {
        Debug.Log("<color=orange>" + msg + "</color>");
        if (resonanceClip != null) AudioSource.PlayClipAtPoint(resonanceClip, transform.position);
        if (uiController != null) uiController.ShowToast(msg);
    }

    // 强化"获得新武器"时调用：打开对应武器子物体
    public void EnableWeapon(WeaponType t)
    {
        if (t == WeaponType.Orbit && orbitWeaponObject != null)
        {
            orbitWeaponObject.SetActive(true);
            hasOrbitWeapon = true;
            Debug.Log("获得新武器：旋刃");
        }
        if (t == WeaponType.Nova && novaWeaponObject != null)
        {
            novaWeaponObject.SetActive(true);
            hasNovaWeapon = true;
            Debug.Log("获得新武器：新星");
        }
    }
}
