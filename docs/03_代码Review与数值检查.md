# 03 · 代码Review与数值检查

> 全部搭完后对照自查。前半是架构8项检查，后半是数值验算（有计算过程，不是"感觉合理"）。

---

## 〇、竣工修订记录（2026.09，3天集中施工后回写）

**代码修复（施工中发现并已修，均已同步到仓库代码）**：
1. `Enemy.cs` Update 移动行 `Vector3 += Vector2` 歧义（CS0034 编译错误）——加 `(Vector3)` 强转修复（同型代码在 Projectile/EnemyBullet/Boss 原本就正确，仅此一处遗漏）。
2. `Boss.cs` 冲锋前摇结束硬编码 `sr.color = Color.white`，会把预制体设的暗红覆盖成白色——改为 Init 时记录 baseColor、前摇结束恢复 baseColor。
3. 音效钩子（Day 9）：Projectile / Pickup / PlayerStats / Boss / GameManager 共 7 个 `AudioClip` 字段 + `PlayClipAtPoint` 调用，全部判空，不填静音不报错。
4. v1.1（2026-09-09，见 07_迭代记录）：GameManager 增加开始界面逻辑（startPanel 字段 + Start() 暂停 + StartGame() 恢复）与 victoryStatsText 战绩填充；Projectile.Init() 末尾增加 `transform.up = dir`（修复飞刀自 Day 3 起从不朝向飞行方向的视觉缺陷）。架构检查结论（判空/无单例/无Find）对新增代码同样成立。

**数值定稿（Day 9 三轮试玩调优，过程与证据见 05_意见箱）**：
- baseDamage 10→15（开局 DPS 12.5→18.75，反超史莱姆 13.3 血/秒的进场流量）
- Elite MaxHP 400→300（集火 21s→16s，一次伤害强化后 <13s）
- HP Scale 0.4→0.6；Damage Scale 0.15→0.25（修复中后期无压力）
- Wave3 Interval 0.7→0.55 且配比 2S+2B+1T → 1S+3B+1T（慢速怪对走位玩家无威胁，终局加权蝙蝠）
- SpawnRadius 13→20（16:9 视野外出生）
- 终局试玩基准：**6:17 胜利 / Lv17 / 400 击杀**

下文第三节的验算**保留原值推导过程作为方法示范**，读时以本节数值为准。

---

## 一、架构8项检查

### 检查1 · 循环依赖
| 引用关系 | 方向 | 结论 |
|---|---|---|
| PlayerStats ↔ UpgradeManager | 互相持有（UpgradeManager写PlayerStats，PlayerStats调UpgradeManager.OnLevelUp） | C#同程序集编译无问题；数据流单向（强化只从UpgradeManager写进PlayerStats），无逻辑死循环 ✅ |
| Enemy/Projectile/Boss → PlayerStats | 单向读+调用受击入口 | ✅ |
| 所有系统 → GameManager | 单向（时间/状态/击杀/结算） | GameManager是中枢，被引用但不反向引用业务系统（只存面板引用） ✅ |
| 继承关系 | 全部平级MonoBehaviour，零继承 | 无继承环 ✅ |

### 检查2 · Singleton
**0个**。没有任何 `Instance` / `DontDestroyOnLoad`。场景重载自动全部重置。

### 检查3 · no static / no singleton（GDD规则）
- `static` 出现处仅2处：`GameEnums.cs`（枚举定义，无状态）和 `EnemyFinder`（**纯函数静态类**，入参出参，没有任何static字段）。
- 无任何静态可变状态 → 重开场景不会有脏数据。符合GDD规则的精神（禁的是static状态）✅

### 检查4 · Inspector 字段能否全部正确赋值
逐脚本核对（对照 00蓝图第5节连接总表）：

| 脚本 | 必填字段 | 在哪一天填 | 漏填的后果（都有判空保护） |
|---|---|---|---|
| PlayerStats | gameManager / orbitWeaponObject / novaWeaponObject（uiController Day8） | Day1 / Day7 / Day8 | 死亡无结算/解锁武器无效，Console警告 |
| DaggerWeapon | projectilePrefab / **spawner** | Day3 | 不发射（Console无报错但无飞刀，最常见漏项） |
| Enemy(各Prefab) | expGemPrefab（elite加heartPrefab） | Day3/4 | 死亡不掉宝石 |
| EnemySpawner | player / playerStats / gameManager / enemyContainer / waves | Day3/4 | 完全不刷怪（gameManager为空直接return） |
| UpgradeManager | playerStats(可自动找) / upgradePanel / 3按钮+3文本 | Day6 | Console警告"未配置好"，跳过升级 |
| Boss(Prefab) | bulletPrefab / summonPrefab | Day7 | 对应技能静默跳过 |
| UIController | playerStats / gameManager + HUD组件 | Day8 | 对应UI不动 |
| CameraFollow | target | Day1 | 摄像机不动 |

### 检查5 · Prefab 引用丢失
- **全部敌人/子弹/宝石Prefab：自包含**（只引用资产Prefab和自身数值），不可能丢。
- **唯一风险点：Player场景实例**（它的 spawner/gameManager 是场景引用，存不进Prefab文件）。规则见 00蓝图 3.3：**不要删除场景里的Player再重拖**；万一重拖了，按蓝图5节重连5个引用即可。

### 检查6 · 脚本名 = 类名
18个文件全部一致：PlayerController / PlayerStats / Enemy / Boss / EnemyBullet / EnemyFinder / DaggerWeapon / OrbitWeapon / OrbitBlade / NovaWeapon / Projectile / Pickup / GameManager / EnemySpawner / UpgradeManager / CameraFollow / UIController / GameEnums（枚举文件，无MonoBehaviour，不受Unity文件名约束）。

### 检查7 · NullReferenceException 风险点（已全部加判空）
| 位置 | 保护 |
|---|---|
| DaggerWeapon.Fire | spawner/projectilePrefab/target 三重判空 |
| Enemy.Update | player 判空 + isDead 先判 |
| Enemy.ApplyResonance | playerStats/spawner 判空；连锁目标可为null |
| Enemy.Die→Drop | prefab判空（没拖宝石就静默不掉） |
| Pickup.Update | playerStats 未Init时直接return |
| Boss.Update | player/gameManager/state 四重判空 |
| UpgradeManager.ShowChoices | 面板未配置→警告+跳过+恢复timeScale |
| GameManager.GameOver/Victory | 面板未拖→只打日志 |
| Projectile/EnemyBullet 命中 | GetComponent 结果判空 |

### 检查8 · Find / FindObjectOfType / GetComponent 审计
- `FindObjectOfType`：**0处**。
- `FindGameObjectWithTag`：**1处**（UpgradeManager.Start 兜底找玩家）——GDD架构规则明确允许"Tag查找只用于找玩家"，且是Inspector忘拖时的兜底。
- `GetComponentInParent`：武器脚本拿同物体链上的PlayerStats（标准安全用法）。
- 触发器内 `GetComponent`：标准做法，均有判空。
- 其余全部 Inspector 拖拽 / Spawner 注入。✅ 符合"能用拖拽就不用Find"。

---

## 二、GDD/Excel ↔ 代码 的映射与3处实现层差异（非设计变更）

**映射**：Excel `Effect1Type/Effect1Value` → UpgradeData 的枚举+数值；Excel 的 GrantWeapon 值 `Weapon_002` 在代码里是 `grantWeapon` 下拉框（Orbit/Nova）；Excel Wave 的"数量" → Inspector SpawnTable 数组里放几个Prefab（数组抽签，GDD 12.1）；Excel Enemy 的 EXP 列 → Enemy.exp 字段，死亡时覆盖 Pickup.expValue。

**差异（都有意为之）**：
1. **Boss 不参与连锁闪电**（连锁只在普通敌人生效；点燃/减速对Boss正常生效）。原因：连锁目标从 aliveEnemies 取，Boss不在该列表；省一条"找最近非Boss目标"的分支。对Build影响：雷流打Boss少一个概率伤害来源，火/冰流无影响。
2. **旋刃0.5秒冷却是"每个敌人"而不是"每把刀"**（GDD写同一把刀）。实现更保守（少一半理论伤害），Enemy侧一个时间戳搞定；要还原就在 Enemy 加两个字段分别记录。
3. **Day 3/4 施工顺序合并**：最小刷怪提前到Day 3（飞刀依赖Spawner列表），完整波次表Day 4。日程不变、内容不变。

---

## 三、数值验算（基于Excel v1.0原值，全部Inspector可调）

### 3.1 玩家实际DPS推演（Boss战前，约6:00，等级≈12）

成长链路（选满型Build，参照Excel）：
- 火之精华×4 → damageMultiplier = 1+0.25×4 = **2.0**
- 雷之精华×4 → attackIntervalMultiplier = 0.85⁴ ≈ **0.522**
- 鹰眼×2 → crit 0.3，期望伤害倍率 = 1 + 0.3×(2-1) = **1.3**
- 分裂飞刀×2 → 3把飞刀（对单体Boss全中）

| Build | 单体DPS计算 | 结果 |
|---|---|---|
| 雷连锁流（上述配置，纯飞刀） | 3 × 10 × 2.0×0.75(假设火只×2) … 取代表性组合：3 × 10 × 1.5 × 1.3 ÷ (0.8 × 0.522) | **≈140 DPS** |
| 火燃爆流（火×4=×2.0，新星×1+新星强化×2：新星伤害×2.0，+点燃共鸣5/s） | 飞刀 10×2.0÷0.8=25；新星 20×2.0÷2.5=16；点燃 5 | **≈46 DPS**（但血最厚） |
| 中庸玩家（随意选：伤害×2、攻速×0.7、带暴击） | 10×2×1.15 ÷ 0.56 ≈ 41，加新星/旋翼 ≈ | **60～80 DPS** |

### 3.2 Boss HP 1500 是否匹配（GDD要求60～90秒战斗）

| 玩家状态 | 纯输出击杀时间 | 计入走位躲弹幕（×2.5～4） | 结论 |
|---|---|---|---|
| 顶配雷流 140 DPS | 1500÷140 ≈ **11秒** | 30～45秒 | 偏快但可接受（Boss要能被杀爽） |
| 中庸 60～80 DPS | 19～25秒 | **50～100秒** | ✅ 正中GDD 60～90秒区间 |
| 低保 46 DPS | 33秒 | 90～130秒 | 偏长但火流肉度高，能磨死 |
| 严重低保（总强化≤5个，DPS≈25） | 60秒 | **2～4分钟，大概率被弹幕耗死** | 设计上可接受：Boss前太弱=这局输了，肉鸽正常 |

**结论：Boss HP=1500 保持不变。** 若试玩普遍反馈"打不过"，最小修改只有一处：Enemy_Boss 预制体 MaxHP 1500→**1200**（对应低保线击杀时间-20%）。不要动其他任何数值。

### 3.3 敌人成长 vs 玩家成长（是否打得动、是否被打死）

| 时间点 | 敌人HP（史莱姆） | 玩家单刀伤害（中庸） | 击杀刀数 | 评价 |
|---|---|---|---|---|
| 0:00 | 20 | 10 | 2刀 | ✅ 节奏合适 |
| 3:00（×2.2） | 44 | ≈14（已拿2-3个强化） | 3刀 | ✅ |
| 6:00（×3.4） | 68 | ≈19.5 | 3～4刀 | ✅ 有压力但不恶心 |

玩家承伤：6:00接触伤害 10×1.9=19，玩家HP 100+强化≈150 → 围5只约2秒死。这就是死亡螺旋压力点，**符合割草设计**（0.5秒无敌+走位可解）。

### 3.4 升级节奏（GDD承诺一局10～13级）
刷怪总量：0-90s刷60只、90-180s刷75只、180-270s刷100只、270-360s刷129只 ≈ **364只**；按70%击杀率≈255杀。加权经验（坦克3/精英10）≈ 总经验270～300。升级累计需求到L13 = 5×12+3×66 = **258**。→ **Boss前约13级 ≈ 13次三选一** ✅ 达标。

### 3.5 结算
**无需整体重平衡。** 可选微调旋钮（都是单字段）：Boss MaxHP 1500→1200；XpBase 5→4（升级更快）；Wave[0] interval 1.5→1.2（开局更热闹）。先按原值玩3局再决定。
