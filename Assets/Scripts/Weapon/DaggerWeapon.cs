// ─── 文件位置: Assets/Scripts/Weapon/DaggerWeapon.cs
// ─── 挂载对象: Player 下的子物体 Weapon_Dagger（开局自带，一直激活）
// ─── 说明: 每0.8秒朝最近的敌人投掷飞刀。分裂飞刀强化后变成±15°扇形多把。
//           唯一需要拖场景引用的武器脚本：spawner（拖场景里的 EnemySpawner）。

using UnityEngine;

public class DaggerWeapon : MonoBehaviour
{
    [Header("数值（与Excel Weapon_001一致）")]
    public float baseDamage = 10f;
    public float attackInterval = 0.8f;
    public float projectileSpeed = 12f;
    public float projectileLifetime = 2f;
    public float spreadAngle = 15f;      // 多把飞刀的扇形间隔角度

    [Header("连接（拖拽）")]
    public Projectile projectilePrefab;  // 拖 Projectile_Dagger 预制体
    public EnemySpawner spawner;         // 拖场景里的 EnemySpawner（要它的存活列表）

    PlayerStats stats;
    float timer;

    void Start()
    {
        stats = GetComponentInParent<PlayerStats>();
    }

    void Update()
    {
        if (stats == null || stats.isDead) return;
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = attackInterval * stats.attackIntervalMultiplier; // 雷之精华缩短间隔
            Fire();
        }
    }

    void Fire()
    {
        if (spawner == null || projectilePrefab == null) return;

        Enemy target = EnemyFinder.FindNearest(transform.position, 100f, spawner.aliveEnemies, null);
        if (target == null) return; // 场上没有敌人就不射

        int count = 1 + stats.extraProjectiles;
        Vector2 baseDir = (target.transform.position - transform.position).normalized;

        for (int i = 0; i < count; i++)
        {
            // 多把飞刀围绕目标方向呈扇形展开
            float offset = (i - (count - 1) * 0.5f) * spreadAngle;
            Vector2 dir = Rotate(baseDir, offset);
            Projectile p = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            p.Init(dir, projectileSpeed, projectileLifetime, FinalDamage(), stats);
        }
    }

    float FinalDamage()
    {
        float dmg = baseDamage * stats.damageMultiplier;
        if (Random.value < stats.critChance) dmg *= stats.critMultiplier; // 暴击×2
        return dmg;
    }

    Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }
}
