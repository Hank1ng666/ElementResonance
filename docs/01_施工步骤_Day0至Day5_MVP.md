# 01 · 施工步骤 Day 0～Day 5（MVP核心循环）

> 每一步都按顺序做。做完每个阶段的"运行测试"再进入下一阶段。
> **竣工说明（2026.09）**：实际施工为 3 天集中完成（Day 0~4 / Day 5~7 / Day 8~10），"Day N"为阶段编号而非日历天。本文档已按竣工实况回写勘误，修订处标注【竣工勘误】。
> 配套文件：`00_施工蓝图`（物体/组件参数）、`03_代码Review`（出错时查）。

---

# Day 0 — 工程准备（约30分钟）

### Step 0.1 建项目
1. Unity Hub → New Project → **2D (Built-In)** 模板，项目名 `ElementResonance`，Unity 版本 2021.3 LTS 或更新。

### Step 0.2 建文件夹
在 Project 窗口 Assets 下右键 → Create → Folder，建：
```
Assets/Scenes、Assets/Scripts、Assets/Prefabs、Assets/Materials、Assets/Sprites、Assets/Audio
```

### Step 0.3 拷入脚本
1. 把本开发包 `UnityDevKit/Assets/Scripts/` **整个文件夹**拷到工程的 `Assets/` 下（直接资源管理器复制粘贴，Unity会自动刷新）。
2. 回到 Unity，等左下角编译转圈结束，**Console 没有红色报错** = 18个脚本全部编译通过。

### Step 0.4 建3个Tag【竣工勘误：原文写4个】
Edit → Project Settings → Tags and Layers → Tags 点 +，依次添加：`Enemy`、`Wall`、`Boss`（大小写完全一致）。`Player` 是 Unity 内置 Tag，**无需创建**，直接在物体 Tag 下拉里选用。

### Step 0.5 建场地
1. Hierarchy 右键 → 2D Object → Sprites → Square，命名 `Ground`：Scale=(44,44,1)，颜色深灰（SpriteRenderer 的 Color），SpriteRenderer 里 **Additional Settings → Sorting Order = -10**。
2. 再建4个 Square：`Wall_Top`(Pos 0,22,0, Scale 46,1,1)、`Wall_Bottom`(0,-22,0)、`Wall_Left`(-22,0,0, Scale 1,46,1)、`Wall_Right`(22,0,0)。每个：Add Component → **Box Collider 2D**（不勾 IsTrigger），Tag 改成 **Wall**，颜色随意。
3. 建空物体 `EnemyContainer`（Position 0,0,0）。
4. 建空物体 `GameManager`（Position 0,0,0）。

### Step 0.6 保存场景并注册
1. Ctrl+S 保存到 `Assets/Scenes/Game.unity`。
2. File → Build Settings → Add Open Scenes（重开按钮依赖这一步，**必做**）。

**Day 0 检查**：Console 无报错；**Scene 视图**（滚轮缩小）能看到灰色场地和四面墙。【竣工勘误：原文写 Game 视图——摄像机 Size=9 视野只有±9，墙在±22 之外，Game 视图看不见墙属正常】

---

# Day 1 — 玩家移动

### Step 1.1 建 Player
1. Hierarchy 右键 → Create Empty，命名 `Player`，Position(0,0,0)。
2. Add Component：**Sprite Renderer**（Sprite=Square，Color 亮青色，Scale=(1,1,1)）。
3. Add Component：**Rigidbody 2D** → 设置：Gravity Scale=**0**，Collision Detection=Discrete，Constraints 里勾选 **Freeze Rotation Z**。
4. Add Component：**Circle Collider 2D** → Radius=0.5，**不勾 IsTrigger**。
5. Tag 下拉 → **Player**。
6. Add Component：`PlayerController`；Add Component：`PlayerStats`。

### Step 1.2 连接
1. Player 的 PlayerStats → `Game Manager` 字段：把 Hierarchy 里的 GameManager 拖进去。

### Step 1.3 摄像机
1. 选中 Main Camera：Projection 改 **Orthographic**，Size=9。
2. Add Component：`CameraFollow` → `Target` 字段：拖 Hierarchy 里的 Player。

### Step 1.4 运行测试
```
点Play
↓ W/S/A/D 或方向键：玩家八方向平滑移动，斜向不加速
↓ 撞墙：停住，不穿墙、不旋转
↓ 摄像机跟着玩家走
↓ Console 无报错
```

**常见错误**
- 玩家往下掉/飞出屏幕 → Rigidbody2D 的 Gravity Scale 忘了改 0。
- 玩家撞墙后疯狂旋转 → 忘了勾 Freeze Rotation Z。
- 按键没反应 → Game 视图没获得焦点（鼠标点一下Game窗口）。

---

# Day 2 — 敌人

### Step 2.1 做史莱姆Prefab
1. Hierarchy 右键 → 2D Object → Sprites → Circle，命名 `Enemy_Slime`，颜色绿色。
2. Add Component：**Circle Collider 2D** → **IsTrigger=✓**。
3. Tag → **Enemy**。
4. Add Component：`Enemy`。
5. Inspector 填：MaxHP=20，MoveSpeed=2.2，ContactDamage=10，DamageInterval=0.5，Exp=1，MoveType=0。
6. 从 Hierarchy 拖到 `Prefabs` 文件夹 → 生成Prefab → **删除场景里的这个**（以后全靠Spawner刷）。

### Step 2.2 蝙蝠和坦克Prefab
用同样方法复制两条（把Prefab拖进场景改数值再Apply回Prefab，或重新建）：
- `Enemy_Bat`：HP12，Speed 4，Damage 8，Exp 1，**MoveType=1**（蛇形），精灵Scale(0.7,0.7,1)紫色。
- `Enemy_Tank`：HP80，Speed 1.2，Damage 20，Exp 3，MoveType=0，精灵Scale(1.6,1.6,1)棕色。
- 每个的 Tag 都必须是 **Enemy**，Collider 都勾 IsTrigger。

### Step 2.3 手动测试敌人【竣工勘误：重写本步】
> 原版预期"手拖的敌人会追人会扣血"不成立：敌人引用由 Spawner 的 `Init()` 注入（Day 3 才有），手拖实例的 player/playerStats 为空 → `Enemy.Update` 首行判空直接 return，**原地不动且接触伤害为 0**。

1. 从 Prefabs 拖1个 Enemy_Slime 到场景里玩家旁边。
2. 在该实例 Enemy 组件的"运行时引用"区，**手动临时拖入**：Player 字段 ← Player，Player Stats 字段 ← Player（测试用，测完删实例，切勿 Apply 回 Prefab）。
3. 运行：史莱姆朝你飘过来（验证移动逻辑）；蝙蝠（MoveType=1）为蛇形走位。
4. 接触伤害与死亡结算**顺延到 Day 3** 挂好 Spawner 后自然验证。

**常见错误**
- 敌人穿墙跑出去 → 出生点在墙外（Day 4 会修，手动测试时拖在场地中间即可）。
- 碰到玩家没反应 → 敌人的 Collider 忘勾 IsTrigger，或玩家忘了 Tag=Player。
- 敌人互相把玩家弹飞 → 检查玩家 Collider 是不是也被勾了 IsTrigger（玩家的**不勾**）。

---

# Day 3 — 自动攻击 + 最小刷怪（GDD的Day3/Day4合并施工）

> 与GDD日程的小差异：飞刀需要 Spawner 的存活列表才能找目标，所以把"最小刷怪"提前到今天，明天再补完整波次表。

### Step 3.1 飞刀Prefab
1. Hierarchy 右键 → 2D Object → Sprites → Square，命名 `Projectile_Dagger`，Scale=(0.15,0.5,1)，颜色亮黄。
2. Add Component：**Rigidbody 2D** → Body Type=**Kinematic**。
3. Add Component：**Circle Collider 2D** → IsTrigger=✓，Radius=0.15。
4. Add Component：`Projectile`（Inspector全部留空——运行时注入）。
5. 拖进 Prefabs 文件夹 → 删除场景里的。

### Step 3.2 给玩家装飞刀
1. Hierarchy 的 Player 右键 → Create Empty，命名 `Weapon_Dagger`（Local Position 0,0,0）。
2. 挂 `DaggerWeapon`。
3. 拖引用：`Projectile Prefab` ← Prefabs 里的 Projectile_Dagger；`Spawner` ← Hierarchy 里的 **GameManager**。
   > 【竣工勘误】Spawner 字段类型是 EnemySpawner 组件，而该组件在 Step 3.5 才挂上——**实际施工顺序：先做 Step 3.5（挂 Spawner + 连线），再回来做本步**，否则拖不进去。

### Step 3.3 建宝石Prefab
1. 建 Sprite(Square)，命名 `Pickup_Gem`，Scale=(0.4,0.4,1)，颜色黄色。
2. Add Component：**Circle Collider 2D** → IsTrigger=✓，Radius=0.3（**不加**Rigidbody）。
3. Add Component：`Pickup` → ExpValue=1，HealAmount=0。
4. 拖进 Prefabs → 删除场景里的。

### Step 3.4 给敌人Prefab挂宝石
1. Project 里选中 Enemy_Slime → Inspector 的 Enemy → `Exp Gem Prefab`：拖 Prefabs 里的 Pickup_Gem。
2. Bat / Tank 同样拖（**点Prefab修改后记得点Override → Apply All**，或直接在Prefab模式里改）。

### Step 3.5 挂 Spawner（最小波次）
1. 选中 GameManager → Add Component：`EnemySpawner`。
2. 填连接：`Player`←拖Player，`Player Stats`←拖Player，`Game Manager`←拖GameManager（自己），`Enemy Container`←拖EnemyContainer。
3. Waves → Size=**1**：StartTime=0，EndTime=9999（测试用超长），SpawnInterval=1.5，SpawnTable Size=1 → 元素0拖 Enemy_Slime。
4. SpawnRadius=**20**，ArenaHalfSize=21。【竣工勘误：原值 13 只保证上下方向屏外出生；16:9 下左右视野±16 > 13，左右两侧会刷在眼前。20 > 视野对角线 18.4 且 < 场地边界 21，全方向屏外】

### Step 3.6 运行测试
```
点Play
↓ 屏幕外不断出现绿色史莱姆，越聚越多
↓ 你站着不动：飞刀自动飞向最近的敌人
↓ 命中后敌人消失（死够快时一击死），原地掉一颗黄色宝石
↓ 宝石飞向玩家并消失（Console打印"升级！"，因为你还没做升级面板，等级在涨）
↓ 蝙蝠会蛇形逼近（拖一只手动看）
```

**常见错误**
- 飞刀不发射 → DaggerWeapon 的 Spawner 字段忘了拖（这是最容易漏的一步）。
- 飞刀报 NullReferenceException → Projectile Prefab 字段没拖。
- 敌人不掉宝石 → 敌人Prefab的 Exp Gem Prefab 没拖。
- 宝石不飞向玩家 → Pickup 的 Init 没被调用（确认掉落来自Enemy.Die而不是手动放场景里的宝石）。

---

# Day 4 — 完整波次 + 难度成长 + 精英

### Step 4.1 补全波次表
选中 GameManager → EnemySpawner → Waves：
- Size 改成 **4**，按下表填（SpawnTable 里多放同一个Prefab就是提高出现概率——"数组抽签"）：

| 段 | Start | End | Interval | SpawnTable |
|---|---|---|---|---|
| 0 | 0 | 90 | 1.5 | Slime×4 |
| 1 | 90 | 180 | 1.2 | Slime, Slime, Slime, Bat |
| 2 | 180 | 270 | 0.9 | Slime, Slime, Bat, Bat, Tank |
| 3 | 270 | 360 | 0.7 | Slime, Slime, Bat, Bat, Tank |

### Step 4.2 做精英Prefab
1. 复制 Enemy_Tank 的做法建 `Enemy_Elite`：HP400，Speed 1.5，Damage 25，Exp=**10**，**Is Elite=✓**，精灵Scale(2.5,2.5,1)深红。
2. `Exp Gem Prefab` 拖 Pickup_Gem；`Heart Prefab`：先做一个 Pickup_Heart Prefab（红色精灵，Pickup脚本，HealAmount=30，ExpValue=0），拖进来。
3. 回到 GameManager 的 EnemySpawner：`Elite Prefab` 拖 Enemy_Elite，Elite Time 1=150，Elite Time 2=300。

### Step 4.3 建回血包Prefab
按 4.2 所述：Pickup_Heart = 红色小方块 + CircleCollider2D(IsTrigger) + Pickup(HealAmount=30, ExpValue=0) → 拖进Prefabs。

### Step 4.4 运行测试【竣工勘误：原"挂机到2:30见精英"预期不成立】
> 原数值下飞刀 12.5 DPS < 史莱姆 13.3 血/秒的进场流量，站桩 **30 秒内必死**（数值必然而非 bug）。验收分两轮：

```
第1轮·站桩送死（验死亡结算链）
↓ Console 两行日志（玩家死亡 + 游戏结束/存活/击杀）、全场冻结（timeScale=0）、停止后可再开

第2轮·走位风筝（验波次与精英）
↓ 1:30 蝙蝠混入（蛇形）；2:30 精英出现；3:00 坦克登场；后期怪更硬
↓ 走位可长期存活（移速差：玩家5 > 蝙蝠4 > 史莱姆2.2）
↓ 杀精英掉大宝石+回血包，吃包 CurrentHP+30
```
（数值失衡的完整分析与 Day 9 三轮调优记录见 `05_意见箱`。）

**常见错误**
- 敌人刷在墙外卡住不动 → Arena Half Size 忘了设 21。
- 精英不掉回血包 → Heart Prefab 字段没拖，或 Is Elite 没勾。
- 时间不走 → GameManager 脚本没挂在 GameManager 物体上（Spawner 在读它的 gameTime）。

---

# Day 5 — 经验与升级闭环 ✅ MVP达成日

### Step 5.1 确认经验链路
代码已全部就位（GainEXP/升级回复/待选强化计数）。今天只验证：
1. 运行，杀怪吃宝石。
2. Console 观察：`升级！当前等级 2，待选强化 1`——**吃满 5 颗宝石**升到 2 级（宝石经验 1/颗，首级需求 5）。【竣工勘误：原文"第一颗宝石经验5直接升一级"有误】
3. Hierarchy 选中 Player → PlayerStats：Level 在涨、CurrentHP 升级时+15。【竣工勘误：原文让看 Pending Level Ups——该字段是 HideInInspector，Inspector 不可见属正常，看 Level 与 CurrentHP 即可】

### Step 5.2 临时"消化"待选强化（明天下单调面板）
现在没有面板，Pending Level Ups 会一直堆积。临时验证升级循环：直接把 `UpgradeManager` 挂到 GameManager（Day 6 才填面板）——**Console 会提示"UpgradeManager 未配置好"，这是预期行为**，说明调用链已通。

### Step 5.3 MVP验收
```
移动 → 刷怪 → 追击 → 自动攻击 → 命中 → 死亡 → 掉宝石 → 吸附拾取
→ 经验上涨 → 升级(+15HP) → 敌人越来越强 → 玩家死亡 → 游戏结束日志
```
**全部串起来 = GDD的MVP达成。** 截图存档，这是第一个里程碑。

**常见错误**
- 升级没反应 → Pickup 的 expValue=0 了（宝石和回血包字段填反）。
- 一次升5级只提示1次 → 正常，Console 只在GainEXP里打一次"待选强化"总数；看 Pending Level Ups 字段确认。
