// ─── 文件位置: Assets/Scripts/Enemy/Enemy.cs
// ─── 挂载对象: Enemy_Slime / Enemy_Bat / Enemy_Tank / Enemy_Elite 四个Prefab（数值各自填）
// ─── 说明: 一个脚本通吃4种敌人。移动=朝玩家方向（蝙蝠加蛇形摆动）。
//           玩家引用和难度系数由 EnemySpawner 出生时注入（Prefab上不用拖场景物体）。

using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("基础数值（与Excel Enemy表一致）")]
    public float maxHP = 20f;
    public float moveSpeed = 2.2f;
    public float contactDamage = 10f;
    public float damageInterval = 0.5f;   // 同一敌人对玩家的伤害间隔
    public float exp = 1f;                // 掉落宝石的经验值（史莱姆1/蝙蝠1/坦克3/精英10）

    [Header("移动方式：0=直线追击 1=蛇形追击")]
    public int moveType = 0;
    public float zigzagAmp = 1.5f;        // 蛇形摆动幅度（蝙蝠用）
    public float zigzagFreq = 6f;         // 蛇形摆动频率（蝙蝠用）

    [Header("掉落（拖Prefab）")]
    public GameObject expGemPrefab;       // 经验宝石（所有敌人都有）
    public GameObject heartPrefab;        // 回血包（只有精英掉）

    [Header("精英标记")]
    public bool isElite = false;          // 精英额外掉回血包

    [Header("运行时引用（由Spawner注入，不用手填）")]
    public Transform player;
    public PlayerStats playerStats;
    public EnemySpawner spawner;
    public GameManager gameManager;

    [HideInInspector] public bool isDead = false;
    [HideInInspector] public float orbitHitTime = -999f;  // 旋刃结算冷却用

    float hp;
    float currentContactDamage;
    float contactTimer;
    float burnTimer;
    float burnDPS;
    float slowTimer;
    float slowFactor = 1f;
    float zigzagSeed;

    // 出生时由 EnemySpawner 调用
    public void Init(Transform p, PlayerStats ps, EnemySpawner sp, float hpMul, float dmgMul, GameManager gm)
    {
        player = p;
        playerStats = ps;
        spawner = sp;
        gameManager = gm;
        hp = maxHP * hpMul;                       // 难度成长: HP × (1 + 0.4 × 分钟数)
        currentContactDamage = contactDamage * dmgMul; // 伤害 × (1 + 0.15 × 分钟数)
        zigzagSeed = Random.Range(0f, 100f);
    }

    void Update()
    {
        if (isDead || player == null) return;

        // 火之共鸣：燃烧持续掉血（不触发共鸣，防止无限连锁）
        if (burnTimer > 0f)
        {
            burnTimer -= Time.deltaTime;
            TakeDamage(burnDPS * Time.deltaTime, false);
            if (isDead) return;
        }

        // 冰之共鸣：减速
        if (slowTimer > 0f) slowTimer -= Time.deltaTime;
        float speedMul = slowTimer > 0f ? slowFactor : 1f;

        // 移动：朝玩家
        Vector2 dir = (player.position - transform.position);
        if (moveType == 1)
        {
            // 蛇形 = 移动方向 + 垂直方向 × 正弦摆动
            Vector2 perp = new Vector2(-dir.y, dir.x).normalized;
            dir += perp * Mathf.Sin(Time.time * zigzagFreq + zigzagSeed) * zigzagAmp;
        }
        transform.position += (Vector3)(dir.normalized * moveSpeed * speedMul) * Time.deltaTime;

        // 接触伤害冷却计时
        if (contactTimer > 0f) contactTimer -= Time.deltaTime;
    }

    // 碰到玩家 → 按间隔造成接触伤害
    void OnTriggerStay2D(Collider2D other)
    {
        if (isDead || !other.CompareTag("Player")) return;
        if (contactTimer <= 0f)
        {
            contactTimer = damageInterval;
            if (playerStats != null) playerStats.TakeDamage(currentContactDamage);
        }
    }

    // 受击入口。triggerResonance=false 用于燃烧跳伤/连锁跳伤（防止共鸣套共鸣）
    public void TakeDamage(float amount, bool triggerResonance)
    {
        if (isDead) return;
        hp -= amount;
        if (triggerResonance) ApplyResonance(amount);
        if (hp <= 0f) Die();
    }

    // 旋刃专用：同一敌人0.5秒内只结算一次
    public void TakeDamageOrbit(float amount)
    {
        if (isDead) return;
        if (Time.time - orbitHitTime < 0.5f) return;
        orbitHitTime = Time.time;
        TakeDamage(amount, true);
    }

    // 元素共鸣：命中时检查玩家的共鸣标志（GDD第5章）
    void ApplyResonance(float hitDamage)
    {
        if (playerStats == null) return;

        if (playerStats.hasFireResonance)
        {
            burnTimer = playerStats.burnDuration;
            burnDPS = playerStats.burnDPS;
        }
        if (playerStats.hasIceResonance)
        {
            slowTimer = playerStats.slowDuration;
            slowFactor = 1f - playerStats.slowRatio;
        }
        if (playerStats.hasLightningResonance && Random.value < playerStats.chainProcChance)
        {
            if (spawner != null)
            {
                // 连锁到6米内最近的"另一个"敌人，造成50%伤害（Boss不参与连锁）
                Enemy other = EnemyFinder.FindNearest(transform.position, playerStats.chainRange, spawner.aliveEnemies, this);
                if (other != null)
                {
                    float chainDamage = hitDamage * playerStats.chainDamageRatio;
                    other.TakeDamage(chainDamage, false);
                    Debug.Log("连锁闪电！额外伤害 " + chainDamage.ToString("0.0"));
                }
            }
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (isElite)
        {
            Drop(expGemPrefab, Mathf.RoundToInt(exp)); // 精英: 大宝石(经验10)
            Drop(heartPrefab, -1);                     //        + 回血包(+30HP)
        }
        else
        {
            Drop(expGemPrefab, Mathf.RoundToInt(exp));
        }

        if (spawner != null) spawner.OnEnemyDead(this);
        Destroy(gameObject);
    }

    // expOverride >= 0 时覆盖宝石经验值（回血包传-1不覆盖）
    void Drop(GameObject prefab, int expOverride)
    {
        if (prefab == null) return;
        GameObject go = Instantiate(prefab, transform.position, Quaternion.identity);
        Pickup p = go.GetComponent<Pickup>();
        if (p != null)
        {
            p.Init(playerStats);
            if (expOverride >= 0) p.expValue = expOverride;
        }
    }
}
