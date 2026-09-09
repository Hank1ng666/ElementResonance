// ─── 文件位置: Assets/Scripts/Game/GameEnums.cs
// ─── 挂载对象: 无（纯枚举定义，不挂载到任何物体）
// ─── 说明: 全项目共用枚举。数值全部来自 GDD v1.0 / Excel 配置表，未做改动。

public enum ElementType { None, Fire, Lightning, Ice }

public enum WeaponType { None, Dagger, Orbit, Nova }

// 强化效果类型（与 Excel Upgrade 表的 Effect1Type / Effect2Type 一一对应）
public enum EffectType
{
    None,
    DamagePercent,          // 伤害+X（火之精华）
    AttackIntervalReduce,   // 攻击间隔-X（雷之精华，乘法叠加）
    MoveSpeedPercent,       // 移速+X（冰之精华）
    PickupRadiusPercent,    // 拾取范围+X（冰之精华）
    CritChancePercent,      // 暴击率+X（鹰眼）
    AddProjectile,          // 飞刀+1（分裂飞刀）
    GrantWeapon,            // 获得新武器（旋刃/新星，用 grantWeapon 字段指定）
    WeaponDamagePercent,    // 武器伤害+X（旋刃强化/新星强化，按 requiresWeapon 区分）
    OrbitSpeedPercent,      // 旋刃转速+X（旋刃强化）
    AoeRadiusPercent,       // 新星半径+X（新星强化）
    MaxHPFlat,              // 最大HP+X（生命护符）
    HealHPFlat,             // 立刻回复X HP（生命护符）
}

public enum GameState { Playing, GameOver, Victory }
