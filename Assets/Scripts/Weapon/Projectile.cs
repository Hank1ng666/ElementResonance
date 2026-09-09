// ─── 文件位置: Assets/Scripts/Weapon/Projectile.cs
// ─── 挂载对象: Projectile_Dagger 预制体
// ─── 说明: 飞刀本体。直线飞行、超时销毁、命中敌人结算伤害并触发共鸣。
//           伤害等参数由 DaggerWeapon 生成时注入（Prefab上不用填伤害）。

using UnityEngine;

public class Projectile : MonoBehaviour
{
    [HideInInspector] public Vector2 direction;
    [HideInInspector] public float speed;
    [HideInInspector] public float lifetime;
    [HideInInspector] public float damage;

    [Header("音效（Day 9，可不填）")]
    public AudioClip hitClip;

    PlayerStats playerStats;

    public void Init(Vector2 dir, float spd, float life, float dmg, PlayerStats ps)
    {
        direction = dir;
        speed = spd;
        lifetime = life;
        damage = dmg;
        playerStats = ps;
        transform.up = dir;   // 精灵是竖长条：让局部Y轴对准飞行方向（2D标准瞄准法）
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed) * Time.deltaTime;
        lifetime -= Time.deltaTime;
        if (lifetime <= 0f) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy e = other.GetComponent<Enemy>();
            if (e != null && !e.isDead)
            {
                e.TakeDamage(damage, true); // true = 触发元素共鸣
                PlayHit();
                Destroy(gameObject);
            }
        }
        else if (other.CompareTag("Boss"))
        {
            Boss b = other.GetComponent<Boss>();
            if (b != null && !b.isDead)
            {
                b.TakeDamage(damage, true);
                PlayHit();
                Destroy(gameObject);
            }
        }
        else if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }

    void PlayHit()
    {
        if (hitClip != null) AudioSource.PlayClipAtPoint(hitClip, transform.position);
    }
}
