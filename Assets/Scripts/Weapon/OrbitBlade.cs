// ─── 文件位置: Assets/Scripts/Weapon/OrbitBlade.cs
// ─── 挂载对象: OrbitBlade 预制体（由 OrbitWeapon 自动实例化，不用手动放场景）
// ─── 说明: 单片刀刃。碰到敌人按 OrbitWeapon 的实时伤害结算（0.5秒/敌人/次冷却在Enemy侧）。

using UnityEngine;

public class OrbitBlade : MonoBehaviour
{
    [HideInInspector] public OrbitWeapon weapon;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (weapon == null) return;

        if (other.CompareTag("Enemy"))
        {
            Enemy e = other.GetComponent<Enemy>();
            if (e != null && !e.isDead) e.TakeDamageOrbit(weapon.GetDamage());
        }
        else if (other.CompareTag("Boss"))
        {
            Boss b = other.GetComponent<Boss>();
            if (b != null && !b.isDead) b.TakeDamageOrbit(weapon.GetDamage());
        }
    }
}
