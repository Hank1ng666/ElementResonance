// ─── 文件位置: Assets/Scripts/Weapon/OrbitWeapon.cs
// ─── 挂载对象: Player 下的子物体 Weapon_Orbit（初始设为 inactive！强化"旋刃"时被激活）
// ─── 说明: 刀刃围绕玩家旋转。刀刃由本脚本在激活时自动生成（数量/半径可调）。
//           伤害实时读取 PlayerStats（强化立即生效）。

using UnityEngine;

public class OrbitWeapon : MonoBehaviour
{
    [Header("数值（与Excel Weapon_002一致）")]
    public float baseDamage = 15f;
    public float rotateSpeed = 270f;   // 度/秒
    public float orbitRadius = 2.2f;
    public int bladeCount = 2;

    [Header("连接（拖拽）")]
    public GameObject bladePrefab;     // 拖 OrbitBlade 预制体

    PlayerStats stats;

    void Start()
    {
        stats = GetComponentInParent<PlayerStats>();
        for (int i = 0; i < bladeCount; i++)
        {
            if (bladePrefab == null) break;
            GameObject blade = Instantiate(bladePrefab, transform);
            float angle = i * 360f / bladeCount;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            blade.transform.localPosition = dir * orbitRadius;
            OrbitBlade b = blade.GetComponent<OrbitBlade>();
            if (b != null) b.weapon = this;
        }
    }

    void Update()
    {
        if (stats == null) return;
        transform.Rotate(0f, 0f, rotateSpeed * stats.orbitSpeedMultiplier * Time.deltaTime);
    }

    // 旋刃刀刃每次命中时实时计算伤害（含伤害倍率/旋刃强化/暴击）
    public float GetDamage()
    {
        if (stats == null) return baseDamage;
        float dmg = baseDamage * stats.damageMultiplier * stats.orbitDamageMultiplier;
        if (Random.value < stats.critChance) dmg *= stats.critMultiplier;
        return dmg;
    }
}
