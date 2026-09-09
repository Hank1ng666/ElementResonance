// ─── 文件位置: Assets/Scripts/Enemy/Boss.cs
// ─── 挂载对象: Boss 预制体
// ─── 说明: 独立脚本（刻意不继承Enemy，GDD 15.3）。三招循环=三个计时器+if。
//           冲锋前摇0.8秒闪红，方向在前摇结束时才锁定（玩家可躲）。
//           引用由 Spawner 生成时注入。共鸣：点燃/减速对Boss生效，连锁闪电不连Boss（见Review）。

using UnityEngine;

public class Boss : MonoBehaviour
{
    [Header("基础数值（与Excel Boss_001一致）")]
    public float maxHP = 1500f;
    public float moveSpeed = 2f;
    public float contactDamage = 30f;
    public float damageInterval = 0.5f;

    [Header("招式1：冲锋")]
    public float chargeCooldown = 5f;
    public float chargeWindup = 0.8f;   // 前摇（闪红停顿）
    public float chargeDuration = 1f;
    public float chargeSpeed = 15f;
    public float chargeDamage = 25f;

    [Header("招式2：环形弹幕")]
    public int bulletCount = 12;
    public float bulletCooldown = 6f;
    public float bulletSpeed = 6f;
    public float bulletLifetime = 4f;
    public float bulletDamage = 15f;
    public EnemyBullet bulletPrefab;    // 拖 BossBullet 预制体

    [Header("招式3：召唤")]
    public int summonCount = 3;
    public float summonCooldown = 8f;
    public Enemy summonPrefab;          // 拖 Enemy_Slime 预制体

    [Header("音效（Day 9，可不填）")]
    public AudioClip appearClip;

    [Header("场地")]
    public float arenaHalfSize = 21f;   // 冲锋别冲出场地

    [Header("运行时引用（由Spawner注入，不用手填）")]
    public Transform player;
    public PlayerStats playerStats;
    public EnemySpawner spawner;
    public GameManager gameManager;

    [HideInInspector] public bool isDead = false;
    [HideInInspector] public float hp;
    [HideInInspector] public float orbitHitTime = -999f;

    float contactTimer;
    float chargeTimer;
    float bulletTimer;
    float summonTimer;
    bool windingUp;
    float windupTimer;
    bool charging;
    float chargeTimeLeft;
    Vector2 chargeDir;
    float burnTimer;
    float burnDPS;
    float slowTimer;
    float slowFactor = 1f;
    SpriteRenderer sr;
    Color baseColor;

    // 出生时由 EnemySpawner 调用
    public void Init(Transform p, PlayerStats ps, EnemySpawner sp, GameManager gm)
    {
        player = p;
        playerStats = ps;
        spawner = sp;
        gameManager = gm;
        hp = maxHP;
        chargeTimer = 2f;
        bulletTimer = 3f;
        summonTimer = 5f;
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) baseColor = sr.color;
        Debug.Log("<color=red>BOSS 来袭！</color>");
        if (appearClip != null) AudioSource.PlayClipAtPoint(appearClip, transform.position);
    }

    void Update()
    {
        if (isDead || player == null || gameManager == null || gameManager.state != GameState.Playing) return;

        // 燃烧（火之共鸣对Boss同样生效）
        if (burnTimer > 0f)
        {
            burnTimer -= Time.deltaTime;
            TakeDamage(burnDPS * Time.deltaTime, false);
            if (isDead) return;
        }
        if (slowTimer > 0f) slowTimer -= Time.deltaTime;
        float speedMul = slowTimer > 0f ? slowFactor : 1f;

        // ── 前摇阶段：原地停顿 + 闪红 ──
        if (windingUp)
        {
            windupTimer -= Time.deltaTime;
            if (sr != null) sr.color = Color.red;
            if (windupTimer <= 0f)
            {
                windingUp = false;
                charging = true;
                chargeTimeLeft = chargeDuration;
                chargeDir = (player.position - transform.position).normalized; // 前摇结束才锁定方向
                if (sr != null) sr.color = baseColor;
            }
            return;
        }

        // ── 冲锋阶段：直线猛冲 ──
        if (charging)
        {
            transform.position += (Vector3)(chargeDir * chargeSpeed) * Time.deltaTime;
            ClampToArena();
            chargeTimeLeft -= Time.deltaTime;
            if (chargeTimeLeft <= 0f) charging = false;
            return;
        }

        // ── 平时：缓慢追击 ──
        Vector2 dir = (player.position - transform.position).normalized;
        transform.position += (Vector3)(dir * moveSpeed * speedMul) * Time.deltaTime;
        ClampToArena();

        chargeTimer -= Time.deltaTime;
        bulletTimer -= Time.deltaTime;
        summonTimer -= Time.deltaTime;

        if (chargeTimer <= 0f)
        {
            chargeTimer = chargeCooldown;
            windingUp = true;
            windupTimer = chargeWindup;
        }
        if (bulletTimer <= 0f)
        {
            bulletTimer = bulletCooldown;
            FireBullets();
        }
        if (summonTimer <= 0f)
        {
            summonTimer = summonCooldown;
            Summon();
        }

        if (contactTimer > 0f) contactTimer -= Time.deltaTime;
    }

    void FireBullets()
    {
        if (bulletPrefab == null) return;
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = i * 360f / bulletCount;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            EnemyBullet b = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            b.Init(dir, bulletSpeed, bulletLifetime, bulletDamage);
        }
    }

    void Summon()
    {
        if (spawner == null || summonPrefab == null) return;
        for (int i = 0; i < summonCount; i++)
            spawner.SpawnEnemyAt(transform.position, summonPrefab);
    }

    // 碰到玩家：冲锋中=25伤害，平时=30接触伤害（都走0.5秒间隔）
    void OnTriggerStay2D(Collider2D other)
    {
        if (isDead || !other.CompareTag("Player")) return;
        if (contactTimer <= 0f)
        {
            contactTimer = damageInterval;
            if (playerStats != null)
                playerStats.TakeDamage(charging ? chargeDamage : contactDamage);
        }
    }

    // 玩家武器调用的受击入口
    public void TakeDamage(float amount, bool triggerResonance)
    {
        if (isDead) return;
        hp -= amount;
        if (triggerResonance && playerStats != null)
        {
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
        }
        if (hp <= 0f) Die();
    }

    // 旋刃命中Boss：同样0.5秒/次
    public void TakeDamageOrbit(float amount)
    {
        if (isDead) return;
        if (Time.time - orbitHitTime < 0.5f) return;
        orbitHitTime = Time.time;
        TakeDamage(amount, true);
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        if (gameManager != null)
        {
            gameManager.currentBoss = null;
            gameManager.Victory();
        }
        Destroy(gameObject);
    }

    void ClampToArena()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -arenaHalfSize, arenaHalfSize);
        pos.y = Mathf.Clamp(pos.y, -arenaHalfSize, arenaHalfSize);
        transform.position = pos;
    }
}
