// ─── 文件位置: Assets/Scripts/Weapon/NovaWeapon.cs
// ─── 挂载对象: Player 下的子物体 Weapon_Nova（初始设为 inactive！强化"新星"时被激活）
// ─── 说明: 每2.5秒以玩家为中心脉冲一圈，伤害范围内全部敌人。不需要Prefab。

using UnityEngine;

public class NovaWeapon : MonoBehaviour
{
    [Header("数值（与Excel Weapon_003一致）")]
    public float baseDamage = 20f;
    public float attackInterval = 2.5f;
    public float baseRadius = 3.5f;

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
            timer = attackInterval;
            Pulse();
        }
    }

    void Pulse()
    {
        float radius = baseRadius * stats.novaRadiusMultiplier;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);

        for (int i = 0; i < hits.Length; i++)
        {
            Enemy e = hits[i].GetComponent<Enemy>();
            if (e != null)
            {
                if (!e.isDead) e.TakeDamage(Damage(), true);
                continue;
            }
            Boss b = hits[i].GetComponent<Boss>();
            if (b != null && !b.isDead) b.TakeDamage(Damage(), true);
        }
        // Day 9 可在这里加圆环扩散特效
    }

    float Damage()
    {
        float dmg = baseDamage * stats.damageMultiplier * stats.novaDamageMultiplier;
        if (Random.value < stats.critChance) dmg *= stats.critMultiplier;
        return dmg;
    }
}
