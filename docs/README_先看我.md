# 《元素共鸣》Unity 开发包 · 先看我

**阅读顺序（按施工阶段）：**

| 顺序 | 文件 | 干什么用 |
|---|---|---|
| 1 | **00_施工蓝图_GameObject与Prefab.md** | 开工前通读一遍：建什么物体、什么Prefab、谁连谁 |
| 2 | **01_施工步骤_Day0至Day5_MVP.md** | 跟着Step做，Day 5结束时核心循环可玩（MVP） |
| 3 | **02_施工步骤_Day6至Day10.md** | 三选一强化、共鸣、Boss、UI、收尾 |
| 4 | **03_代码Review与数值检查.md** | 全部搭完后自查用（架构8项检查+DPS验算） |
| 5 | **04_最终验收Checklist.md** | 交付前逐条打勾 |
| 6 | **05_平衡优化_意见箱.md** | 施工期随手记的手感/数值问题，Day 9平衡日的工作清单 |
| 7 | **06_2.0愿望单.md** | 下一版本的候选清单（2.0开工看这个，不翻1.0的案） |
| 8 | **07_竣工后迭代记录.md** | v1.0交付后的增量更新日志（开始界面等v1.1内容），追版本时看 |

> **竣工说明（2026.09）**：项目实际以 3 天集中施工完成（Day 0~4 / 5~7 / 8~10 各一天），Day 为阶段编号。00~04 已按竣工实况勘误回写（标【竣工】/【竣工勘误】），数值定稿与三轮调优过程见 05。

## 这个包里有什么

```
UnityDevKit/
├── README_先看我.md               ← 本文件
├── 00_施工蓝图_GameObject与Prefab.md
├── 01_施工步骤_Day0至Day5_MVP.md
├── 02_施工步骤_Day6至Day10.md
├── 03_代码Review与数值检查.md
├── 04_最终验收Checklist.md
└── Assets/
    └── Scripts/                   ← 18个C#脚本，Day 1一次性拷进Unity工程
        ├── Player/   PlayerController.cs  PlayerStats.cs
        ├── Enemy/    Enemy.cs  Boss.cs  EnemyBullet.cs  EnemyFinder.cs
        ├── Weapon/   DaggerWeapon.cs  OrbitWeapon.cs  OrbitBlade.cs
        │             NovaWeapon.cs  Projectile.cs
        ├── Pickups/  Pickup.cs
        ├── Game/     GameEnums.cs  GameManager.cs  EnemySpawner.cs
        │             UpgradeManager.cs  CameraFollow.cs
        └── UI/       UIController.cs
```

## 最重要的3条使用规则

1. **脚本全部提前给齐了**——Day 1就把整个 `Scripts` 文件夹拷进 Unity 的 `Assets` 下（一次性，之后不再写代码，只做"挂脚本+填Inspector"）。工程里比GDD多两个小文件是正常的：`GameEnums.cs`（共用枚举）和 `OrbitBlade.cs`（旋刃刀刃），见Review文档说明。
2. **所有数值都在 Inspector 里调**，不要改代码。每个脚本顶部的注释写了它的文件位置和挂载对象。
3. **Prefab 上永远不拖场景物体**。需要场景引用的（Spawner、GameManager等）都由 Spawner 在敌人生成时注入；Player 身上的引用在**场景中的 Player 实例**上拖（见蓝图 3.3 节的警告）。

## 与 GDD/Excel 的关系

- 数值全部来自 `Roguelike_Prototype_Data.xlsx`（GDD v1.0 没有改动）。
- 枚举 `EffectType` 与 Excel Upgrade 表的 Effect 字段一一对应；Excel 的 `Effect1Value=Weapon_002`（GrantWeapon 类型）在代码里用 `grantWeapon` 下拉框表示，见 `03_代码Review` 第 9 条。
- 与 GDD 的两处实现层差异（都不是设计变更）：Boss 不参与连锁闪电；Day 3/4 的刷怪搭建顺序合并。详见 `03_代码Review` 第 10、11 条。
