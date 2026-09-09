// ─── 文件位置: Assets/Scripts/Enemy/EnemyFinder.cs
// ─── 挂载对象: 无（纯函数工具类，不挂载）
// ─── 说明: 在 Spawner 的存活列表里找最近的敌人。
//           这是全项目唯一允许的 static 类——它没有任何static状态，场景重载安全。

using System.Collections.Generic;
using UnityEngine;

public static class EnemyFinder
{
    // fromPos: 从哪里找; maxRange: 最远距离; enemies: 存活敌人列表; exclude: 排除谁(连锁闪电不能连自己)
    public static Enemy FindNearest(Vector3 fromPos, float maxRange, List<Enemy> enemies, Enemy exclude)
    {
        if (enemies == null) return null;
        Enemy best = null;
        float bestSqr = maxRange * maxRange;
        for (int i = 0; i < enemies.Count; i++)
        {
            Enemy e = enemies[i];
            if (e == null || e.isDead || e == exclude) continue;
            float sqr = (e.transform.position - fromPos).sqrMagnitude;
            if (sqr <= bestSqr)
            {
                bestSqr = sqr;
                best = e;
            }
        }
        return best;
    }
}
