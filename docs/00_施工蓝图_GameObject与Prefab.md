# 00 · Unity 施工蓝图（GameObject / Prefab / 连接总表）

> 开工前通读一遍。所有脚本已在 `Assets/Scripts/` 里给齐，本蓝图告诉你"挂在哪、填什么、连什么"。
> **竣工说明（2026.09）**：本文档数值已按竣工实况回写（Day 9 三轮调优终稿），修订处标注【竣工】；调优过程与证据见 `05_意见箱`。

---

## 1. 文件夹结构（GDD 第19章，不新增）

在 Unity 的 Project 窗口 `Assets` 下创建：

```
Assets/
├── Scenes/
├── Scripts/          ← 把 UnityDevKit/Assets/Scripts 整个拷进来
│   ├── Player/
│   ├── Enemy/
│   ├── Weapon/
│   ├── Pickups/
│   ├── Game/
│   └── UI/
├── Prefabs/
├── Materials/
├── Sprites/
└── Audio/
```

## 2. 工程设置（Day 0 一次做完）

| 设置 | 位置 | 值 |
|---|---|---|
| Tag: `Enemy` | Edit → Project Settings → Tags and Layers | 加 Tag |
| Tag: `Wall` | 同上 | 加 Tag |
| Tag: `Boss` | 同上 | 加 Tag |
| ~~Tag: `Player`~~ | — | 【竣工勘误】`Player` 为 Unity 内置 Tag，**无需创建**，直接选用 |
| 主场景存为 `Scenes/Game.unity` | Ctrl+S | 并拖进 File → Build Settings（重开按钮需要！） |
| Camera | Main Camera 的 Inspector | Projection=Orthographic，Size=9 |
| 物理矩阵 | 默认不改 | — |

> 不需要创建任何 Layer。

---

## 3. 场景 GameObject 清单（Hierarchy 最终样子）

```
Game.unity
├── GameManager          ← 空物体，挂3个脚本
├── EnemyContainer       ← 空物体（刷出的怪都进它下面）
├── Ground               ← 纯视觉
├── Wall_Top / Wall_Bottom / Wall_Left / Wall_Right
├── Main Camera          ← 挂 CameraFollow
├── Player               ← 由Prefab拖入场景（引用拖在场景实例上！）
│   ├── Weapon_Dagger    ← 激活
│   ├── Weapon_Orbit     ← 默认关闭
│   └── Weapon_Nova      ← 默认关闭
└── Canvas               ← Day 8 建（含 EventSystem 自动生成）
    ├── StartPanel（v1.1：标题+操作说明+"开始游戏"按钮，见 07_迭代记录）
    ├── HUD（HP条/等级/经验条/计时/击杀/共鸣提示）
    ├── BossHpBar（Day 7/8）
    ├── UpgradePanel（Day 6）
    ├── GameOverPanel（Day 8）
    └── VictoryPanel（Day 8）
```

### 3.1 GameManager

| 项 | 内容 |
|---|---|
| Hierarchy 位置 | 根级，位置(0,0,0) |
| Tag | 无 |
| Component | `EnemySpawner`、`UpgradeManager`、`GameManager` 三个脚本挂同一个物体 |
| Inspector 填写 | 见下方三个脚本各自字段 |

**EnemySpawner（在 GameManager 上）字段：**

| 字段 | 填什么 |
|---|---|
| Waves → Size | `4`（按下面填） |
| Waves[0] | Start 0 / End 90 / Interval 1.5 / SpawnTable Size=4（4个都是 Enemy_Slime） |
| Waves[1] | Start 90 / End 180 / Interval 1.2 / SpawnTable Size=4（3个Slime+1个Bat） |
| Waves[2] | Start 180 / End 270 / Interval 0.9 / SpawnTable Size=5（2Slime+2Bat+1Tank） |
| Waves[3] | Start 270 / End 360 / Interval **0.55** / SpawnTable Size=5（1Slime+**3Bat**+1Tank）【竣工：Day 9 第3轮调优，原 0.7 / 2S+2B+1T】 |
| Elite Prefab | 拖 Enemy_Elite（Day 4 之前可空） |
| Elite Time 1 / 2 | 150 / 300 |
| Boss Prefab | Day 7 再拖 Enemy_Boss |
| Boss Time | 360 |
| Spawn Radius | **20**【竣工勘误：原13在16:9下左右两侧刷在视野内（左右视野±16>13）；20>对角线18.4且<边界21】 |
| Arena Half Size | 21 |
| HP Scale Per Minute | **0.6**【竣工：Day 9 调优，原0.4】 |
| Damage Scale Per Minute | **0.25**【竣工：Day 9 调优，原0.15】 |
| Difficulty Cap Time | 360 |
| **Player** | **拖场景中的 Player** |
| **Player Stats** | **拖场景中的 Player** |
| **Game Manager** | **拖本物体自己（GameManager）** |
| **Enemy Container** | **拖 EnemyContainer** |

**UpgradeManager（在 GameManager 上）字段（Day 6 填）：**

| 字段 | 填什么 |
|---|---|
| Upgrades → Size | `10`（10条数据见 01 文档 Day 6 的表格） |
| Player Stats | 拖 Player（不拖也行，脚本会按Tag自动找） |
| Upgrade Panel | 拖 Canvas 下的 UpgradePanel |
| Option Buttons | Size=3，拖 UpgradePanel 下的3个Button |
| Option Texts | Size=3，拖每个Button下的Text |

**GameManager 脚本字段（Day 8 填；v1.1 增补）：**

| 字段 | 填什么 |
|---|---|
| Game Over Panel | 拖 GameOverPanel |
| Victory Panel | 拖 VictoryPanel |
| Start Panel | 拖 StartPanel【v1.1：不填 = 点 Play 直接开局】 |
| Victory Stats Text | 拖 VictoryPanel 里的战绩 Text【v1.1】 |
| Start Panel 音效等其余字段 | gameOverClip / victoryClip（音效，可空） |

> v1.1 行为：有 Start Panel 时点 Play 先暂停（timeScale=0），按钮 OnClick 调 `GameManager.StartGame` 开局。

### 3.2 Player（场景实例）

| 项 | 内容 |
|---|---|
| 创建方式 | 【竣工勘误】Day 1 直接在场景建空物体挂齐组件使用，**未制作 Player Prefab**（场景实例即最终形态，引用也全在实例上） |
| Transform | (0, 0, 0) |
| Tag | **Player** |
| Component | SpriteRenderer（Square改色）、**Rigidbody2D**（Gravity Scale=0，Collision Detection=Discrete，Freeze Rotation Z=✓）、**CircleCollider2D**（**不勾** IsTrigger，半径0.5）、`PlayerController`、`PlayerStats` |
| PlayerStats Inspector | 数值全用默认（=Excel Player表）；连接字段：`Game Manager`←拖 GameManager，`Orbit Weapon Object`←拖子物体 Weapon_Orbit，`Nova Weapon Object`←拖子物体 Weapon_Nova，`Ui Controller`←Day 8 拖 Canvas |

### 3.3 ⚠ 重要警告：Player 的引用拖在场景实例上

Player 身上的 `DaggerWeapon.spawner`、`PlayerStats.gameManager` 等是**场景物体引用**，只能拖在**场景里的 Player 实例**上。如果你哪天删掉场景里的 Player 再重新从 Prefabs 拖一个新的进来——**所有引用要重新拖一遍**（Prefab 本身保存不了场景引用）。这是本项目唯一的引用维护点，其它 Prefab 全部自包含。

### 3.4 玩家武器子物体

| 子物体 | 挂的脚本 | 关键 Inspector | 初始状态 |
|---|---|---|---|
| Weapon_Dagger | `DaggerWeapon` | Projectile Prefab←拖 Projectile_Dagger 预制体；**Spawner←拖 GameManager**（要它的EnemySpawner）；数值（**15** / 0.8 / 12 / 2 / 15°）【竣工：Day 9 调优，伤害10→15】 | **激活** |
| Weapon_Orbit | `OrbitWeapon` | Blade Prefab←拖 OrbitBlade 预制体；数值默认（15 / 270°/s / 2.2 / 2把） | **关闭**（强化解锁时 PlayerStats.SetActive） |
| Weapon_Nova | `NovaWeapon` | 数值默认（20 / 2.5s / 3.5） | **关闭** |

三个武器子物体的 Transform 位置 = (0,0,0)（跟随玩家本体）。

### 3.5 场地（Day 0）

| 物体 | 组件 | 设置 |
|---|---|---|
| Ground | SpriteRenderer | Square精灵，Scale=(44,44,1)，颜色深灰，排序在最后（Order in Layer=-10） |
| Wall_Top | BoxCollider2D（**不勾**IsTrigger）+ SpriteRenderer | Scale=(46,1,1)，Position=(0,22,0)，Tag=**Wall** |
| Wall_Bottom | 同上 | Position=(0,-22,0) |
| Wall_Left | 同上 | Scale=(1,46,1)，Position=(-22,0,0) |
| Wall_Right | 同上 | Position=(22,0,0) |

---

## 4. Prefab 清单（全部放 Assets/Prefabs/）

> 通用规则（GDD）：**除墙以外所有 Collider2D 都勾 IsTrigger**。子弹/飞刀类需要 Rigidbody2D（Body Type=Kinematic）；敌人、宝石不需要 Rigidbody2D（玩家身上有 Dynamic RB，触发器照样响应）。

### 4.1 Projectile_Dagger（飞刀）

| 组件 | 设置 |
|---|---|
| SpriteRenderer | 小三角/细长方形精灵，改亮色 |
| Rigidbody2D | Body Type=**Kinematic** |
| CircleCollider2D | **IsTrigger=✓**，Radius≈0.15 |
| 脚本 | `Projectile` |
| Tag | 无（它靠 Tag 认别人） |
| Inspector | 全部隐藏字段由 DaggerWeapon 注入，**不用填** |

### 4.2 Enemy_Slime（史莱姆）

| 组件 | 设置 |
|---|---|
| SpriteRenderer | 圆形精灵，绿色，Scale≈(1,1,1) |
| CircleCollider2D | **IsTrigger=✓**，Radius=0.5 |
| Rigidbody2D | **不加** |
| 脚本 | `Enemy` |
| Tag | **Enemy** |
| Inspector | MaxHP=20，MoveSpeed=2.2，ContactDamage=10，DamageInterval=0.5，Exp=1，MoveType=0，ExpGemPrefab=拖 Pickup_Gem |

### 4.3 Enemy_Bat（蝙蝠）

同上结构（Tag=Enemy，无Rigidbody），差异：MaxHP=12，MoveSpeed=4，ContactDamage=8，Exp=1，**MoveType=1**，ZigzagAmp=1.5，ZigzagFreq=6。精灵用小一点的圆形改紫色。

### 4.4 Enemy_Tank（坦克）

同上结构，差异：MaxHP=80，MoveSpeed=1.2，ContactDamage=20，Exp=3，MoveType=0，精灵 Scale≈(1.6,1.6,1) 改棕色。

### 4.5 Enemy_Elite（精英·巨兽）

同上结构，差异：MaxHP=**300**【竣工：Day 9 调优，原400】，MoveSpeed=1.5，ContactDamage=25，Exp=**10**，**IsElite=✓**，ExpGemPrefab=拖 Pickup_Gem，HeartPrefab=拖 Pickup_Heart。精灵 Scale≈(2.5,2.5,1) 改深红色。

### 4.6 Pickup_Gem（经验宝石）

| 组件 | 设置 |
|---|---|
| SpriteRenderer | 小菱形/小方块，黄色，Scale≈(0.4,0.4,1) |
| CircleCollider2D | **IsTrigger=✓**，Radius=0.3 |
| Rigidbody2D | 不加 |
| 脚本 | `Pickup` |
| Inspector | ExpValue=1，HealAmount=0 |

### 4.7 Pickup_Heart（回血包）

同上结构（红色精灵），差异：ExpValue=0，**HealAmount=30**。

### 4.8 OrbitBlade（旋刃刀刃）

| 组件 | 设置 |
|---|---|
| SpriteRenderer | 长条精灵，银白色，Scale≈(0.7,0.25,1) |
| CircleCollider2D | **IsTrigger=✓**，Radius=0.35 |
| Rigidbody2D | 不加（父物体链上玩家的Dynamic RB会让触发器生效） |
| 脚本 | `OrbitBlade` |
| Inspector | 全部运行时注入 |

### 4.9 Enemy_Boss（Boss，Day 7 建）

| 组件 | 设置 |
|---|---|
| SpriteRenderer | 大方形精灵，暗红色，Scale≈(3,3,1) |
| CircleCollider2D | **IsTrigger=✓**，Radius=**0.5**【竣工勘误：原值1.5——Collider半径会被Scale放大，1.5×3=4.5格碰撞圈，"没碰到就掉血"；0.5×3=1.5与3×3身体一致】 |
| Rigidbody2D | 不加 |
| 脚本 | `Boss` |
| Tag | **Boss** |
| Inspector | 数值全默认（=Excel Boss_001）；BulletPrefab=拖 BossBullet 预制体；SummonPrefab=拖 Enemy_Slime 预制体；ArenaHalfSize=21 |

### 4.10 BossBullet（Boss子弹）

| 组件 | 设置 |
|---|---|
| SpriteRenderer | 小圆精灵，红色，Scale≈(0.3,0.3,1) |
| CircleCollider2D | **IsTrigger=✓**，Radius=0.2 |
| Rigidbody2D | Body Type=**Kinematic** |
| 脚本 | `EnemyBullet` |

### 4.11 Player（Prefab）【竣工：未采用】

原计划将场景实例另存为 Prefab 备份；实际施工直接使用场景实例，**未创建**。若日后需要，把场景 Player 拖进 Prefabs 文件夹即可（场景引用仍留在实例上，见 3.3 警告）。

---

## 5. 连接总表（谁拖到谁身上）

| 源（拖什么） | 目标（拖到哪） | 字段名 |
|---|---|---|
| 场景 Player | GameManager 的 EnemySpawner | Player / PlayerStats |
| GameManager 自己 | GameManager 的 EnemySpawner | Game Manager |
| EnemyContainer | GameManager 的 EnemySpawner | Enemy Container |
| GameManager（场景） | Player 的 PlayerStats | Game Manager |
| EnemySpawner（场景） | Weapon_Dagger | Spawner |
| Projectile_Dagger（Prefab资产） | Weapon_Dagger | Projectile Prefab |
| OrbitBlade（Prefab资产） | Weapon_Orbit | Blade Prefab |
| Weapon_Orbit 子物体 | Player 的 PlayerStats | Orbit Weapon Object |
| Weapon_Nova 子物体 | Player 的 PlayerStats | Nova Weapon Object |
| Pickup_Gem / Pickup_Heart（Prefab资产） | 各敌人Prefab | Exp Gem Prefab / Heart Prefab |
| UpgradePanel + 3按钮 + 3文本 | GameManager 的 UpgradeManager | Upgrade Panel / Option Buttons / Option Texts |
| Canvas（场景） | Player 的 PlayerStats | Ui Controller（Day 8） |
| 场景 Player | Main Camera 的 CameraFollow | Target |
| GameOverPanel / VictoryPanel | GameManager | Game Over Panel / Victory Panel（Day 8） |
| BossBullet / Enemy_Slime（Prefab资产） | Enemy_Boss 预制体 | Bullet Prefab / Summon Prefab（Day 7） |
| StartPanel | GameManager | Start Panel；开始按钮 OnClick → `GameManager.StartGame`【v1.1】 |
| 各音频文件 | 各预制体/场景物体 | 音效接线总表见 07_迭代记录（Day 9） |

**不需要任何手动连接的**：Enemy、Boss、Projectile、EnemyBullet、Pickup、OrbitBlade 的运行时引用全部由 Spawner / 武器脚本在生成时注入。
