// ─── 文件位置: Assets/Scripts/Enemy/EnemyBullet.cs
// ─── 挂载对象: BossBullet 预制体（由 Boss 生成，不用手动放场景）
// ─── 说明: Boss弹幕子弹。直线飞行，碰到玩家扣血，碰墙/超时销毁。

using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [HideInInspector] public Vector2 direction;
    [HideInInspector] public float speed;
    [HideInInspector] public float lifetime;
    [HideInInspector] public float damage;

    public void Init(Vector2 dir, float spd, float life, float dmg)
    {
        direction = dir;
        speed = spd;
        lifetime = life;
        damage = dmg;
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed) * Time.deltaTime;
        lifetime -= Time.deltaTime;
        if (lifetime <= 0f) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats ps = other.GetComponent<PlayerStats>();
            if (ps != null) ps.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
