// ─── 文件位置: Assets/Scripts/Game/EnemySpawner.cs
// ─── 挂载对象: GameManager（同一个物体，也可以单独建一个物体挂）
// ─── 说明: 按GDD 12.1的波次时间表刷怪 + 难度成长 + 精英/Boss定时事件。
//           维护 aliveEnemies 列表（飞刀找目标、连锁闪电都从这里查）。

using System.Collections.Generic;
using UnityEngine;

// 波次数据：在Inspector里像填表格一样填（每段一个时间范围+间隔+敌人Prefab数组）
[System.Serializable]
public class WaveData
{
    public float startTime;
    public float endTime;
    public float spawnInterval;
    public Enemy[] spawnTable;   // 想提高某种怪概率，就在数组里多放几个它的Prefab
}

public class EnemySpawner : MonoBehaviour
{
    [Header("波次表（按GDD 12.1填4段）")]
    public List<WaveData> waves = new List<WaveData>();

    [Header("精英与Boss事件")]
    public Enemy elitePrefab;
    public float eliteTime1 = 150f;   // 2:30
    public float eliteTime2 = 300f;   // 5:00
    public Boss bossPrefab;           // Day 7之前留空即可
    public float bossTime = 360f;     // 6:00

    [Header("刷怪参数（Excel Balance表）")]
    public float spawnRadius = 13f;        // 以玩家为圆心的出生半径(屏幕外)
    public float arenaHalfSize = 21f;      // 场地44×44的一半再留1米边距
    public float hpScalePerMinute = 0.4f;  // 敌人HP成长
    public float damageScalePerMinute = 0.15f;
    public float difficultyCapTime = 360f; // Boss登场后难度封顶

    [Header("连接（拖拽）")]
    public Transform player;
    public PlayerStats playerStats;
    public GameManager gameManager;
    public Transform enemyContainer;       // 空物体，刷出来的怪都扔它下面

    public List<Enemy> aliveEnemies = new List<Enemy>();

    float spawnTimer;
    bool elite1Done;
    bool elite2Done;
    bool bossSpawned;

    public float CurrentHPMultiplier()
    {
        float t = Mathf.Min(gameManager != null ? gameManager.gameTime : 0f, difficultyCapTime);
        return 1f + hpScalePerMinute * (t / 60f);
    }

    public float CurrentDamageMultiplier()
    {
        float t = Mathf.Min(gameManager != null ? gameManager.gameTime : 0f, difficultyCapTime);
        return 1f + damageScalePerMinute * (t / 60f);
    }

    void Update()
    {
        if (gameManager == null || gameManager.state != GameState.Playing) return;
        if (player == null || playerStats == null || playerStats.isDead) return;

        float t = gameManager.gameTime;

        // 当前时间段
        WaveData wave = null;
        for (int i = 0; i < waves.Count; i++)
        {
            if (t >= waves[i].startTime && t < waves[i].endTime)
            {
                wave = waves[i];
                break;
            }
        }

        if (wave != null && wave.spawnTable != null && wave.spawnTable.Length > 0)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                float interval = wave.spawnInterval;
                if (bossSpawned) interval *= 2f;   // Boss存在期间刷怪间隔×2
                spawnTimer = interval;
                Enemy prefab = wave.spawnTable[Random.Range(0, wave.spawnTable.Length)];
                SpawnEnemyAt(SpawnPosition(), prefab);
            }
        }

        // 定时事件
        if (!elite1Done && t >= eliteTime1)
        {
            elite1Done = true;
            if (elitePrefab != null) SpawnEnemyAt(SpawnPosition(), elitePrefab);
        }
        if (!elite2Done && t >= eliteTime2)
        {
            elite2Done = true;
            if (elitePrefab != null) SpawnEnemyAt(SpawnPosition(), elitePrefab);
        }
        if (!bossSpawned && bossPrefab != null && t >= bossTime)
        {
            bossSpawned = true;
            Boss b = Instantiate(bossPrefab, SpawnPosition(), Quaternion.identity, enemyContainer);
            b.Init(player, playerStats, this, gameManager);
            if (gameManager != null) gameManager.currentBoss = b;
        }
    }

    // 在指定位置生成敌人（Boss召唤小怪也走这里）。返回生成的敌人。
    public Enemy SpawnEnemyAt(Vector3 pos, Enemy prefab)
    {
        if (prefab == null) return null;
        Enemy e = Instantiate(prefab, pos, Quaternion.identity, enemyContainer);
        e.Init(player, playerStats, this, CurrentHPMultiplier(), CurrentDamageMultiplier(), gameManager);
        aliveEnemies.Add(e);
        return e;
    }

    // 玩家周围半径13的圆上随机取点，再夹回场地内（保证在屏幕外出生）
    Vector3 SpawnPosition()
    {
        Vector2 dir = Random.insideUnitCircle.normalized;
        Vector3 pos = player.position + new Vector3(dir.x, dir.y, 0f) * spawnRadius;
        pos.x = Mathf.Clamp(pos.x, -arenaHalfSize, arenaHalfSize);
        pos.y = Mathf.Clamp(pos.y, -arenaHalfSize, arenaHalfSize);
        return pos;
    }

    // 敌人死亡时回调：从列表移除 + 击杀数+1
    public void OnEnemyDead(Enemy e)
    {
        aliveEnemies.Remove(e);
        if (gameManager != null) gameManager.AddKill();
    }
}
