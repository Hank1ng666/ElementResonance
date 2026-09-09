# 02 · 施工步骤 Day 6～Day 10（强化 · 共鸣 · Boss · UI · 收尾）

> 前置：Day 0～5 已完成，MVP可玩。
> **版本说明（2026.09）**：实际施工为 3 天集中完成（Day 5~7 / Day 8~10 各一天），Day 为阶段编号。

---

# Day 6 — 升级三选一 + 元素共鸣（灵魂日，多留2小时）

### Step 6.1 搭升级面板UI
1. Hierarchy 右键 → UI → **Canvas**（自动带 EventSystem；Canvas Scaler 保持默认即可）。
2. Canvas 右键 → UI → **Panel**，命名 `UpgradePanel`：
   - Image 颜色改黑色、Alpha=180（半透明遮罩）；
   - **Inspector 左上角取消勾选**（默认 inactive，升级时才显示）。
3. UpgradePanel 右键 → UI → **Button**，命名 `OptionButton1`；把按钮下的 Text 内容清空，Anchor 居中，Rect 宽400高150。复制两份：`OptionButton2`、`OptionButton3`，垂直排开（比如 Y=+90 / 0 / -90）。
4. **Text 必须用 Legacy Text**（和脚本 `UnityEngine.UI.Text` 对应）：Unity 2021+ 建UI时如果默认出 TMP，右键 → UI → **Legacy** → Text。每个按钮的 Text 锚点拉满按钮（Alt+点击 Stretch 图标），Alignment=居中。

### Step 6.2 配置 UpgradeManager
选中 GameManager（已有 UpgradeManager 组件）：
1. `Player Stats` ← 拖 Player。
2. `Upgrade Panel` ← 拖 UpgradePanel。
3. `Option Buttons` → Size=3，拖3个Button。
4. `Option Texts` → Size=3，拖每个Button下的Text。
5. `Upgrades` → Size=**10**，按下表逐条填（数值=Excel Upgrade表）：

| # | Name | Description | Element | Effect1 Type / Value | Effect2 Type / Value | MaxCount | RequiresWeapon | GrantWeapon |
|---|---|---|---|---|---|---|---|---|
| 1 | 火之精华 | 伤害+25% | Fire | DamagePercent / 0.25 | None / 0 | 99 | None | None |
| 2 | 雷之精华 | 攻击间隔-15% | Lightning | AttackIntervalReduce / 0.15 | None / 0 | 99 | None | None |
| 3 | 冰之精华 | 移速+12%，拾取范围+30% | Ice | MoveSpeedPercent / 0.12 | PickupRadiusPercent / 0.3 | 99 | None | None |
| 4 | 鹰眼 | 暴击率+15%（暴击=2倍伤害） | None | CritChancePercent / 0.15 | None / 0 | 2 | None | None |
| 5 | 分裂飞刀 | 飞刀+1（扇形射出，最多3把） | None | AddProjectile / 1 | None / 0 | 2 | None | None |
| 6 | 旋刃 | 获得新武器：旋刃 | None | GrantWeapon / 0 | None / 0 | 1 | None | **Orbit** |
| 7 | 旋刃强化 | 旋刃伤害+50%，转速+25% | None | WeaponDamagePercent / 0.5 | OrbitSpeedPercent / 0.25 | 2 | **Orbit** | None |
| 8 | 新星 | 获得新武器：新星 | None | GrantWeapon / 0 | None / 0 | 1 | None | **Nova** |
| 9 | 新星强化 | 新星伤害+50%，半径+30% | None | WeaponDamagePercent / 0.5 | AoeRadiusPercent / 0.3 | 2 | **Nova** | None |
| 10 | 生命护符 | 最大HP+25，并立刻回复50 | None | MaxHPFlat / 25 | HealHPFlat / 50 | 99 | None | None |

### Step 6.3 运行测试（三选一）
```
点Play，杀怪升级
↓ 游戏瞬间冻结，弹出半透明面板，3个按钮显示强化名+描述
↓ 点一个：面板消失，游戏继续，Console打印"选择强化: xxx"
↓ 拿几颗大经验快速连升：面板会连续弹（一次处理完所有排队升级）
↓ 选"生命护符"：PlayerStats.MaxHP+25且回50
↓ 选"分裂飞刀"×2：飞刀变3把扇形
```
> 实测注：数值类强化当日即可验收；**"旋刃/新星"选后无任何即时效果属预期**——两件武器物体 Day 7 才创建（PlayerStats 的 Orbit/Nova Weapon Object 字段为空，`EnableWeapon` 判空静默跳过）。Day 7 建好物体并拖好字段后即生效。

**常见错误**
- 点按钮没反应 → Option Buttons/Option Texts 数组没填或顺序错（Text拖成了按钮父物体）。
- 面板弹出但游戏还暂停着不恢复 → 检查是否手动改过 ChooseUpgrade（别改代码）；确认面板上的按钮没被一层透明Image挡住（Raycast Target）。
- 强化不生效 → 数值填成了百分数25而不是小数0.25（Excel约定：小数存储）。

### Step 6.4 验证元素共鸣
1. 用一局疯狂选 火之精华：第2次选中时 **Console 打印橙色"火之共鸣！攻击点燃敌人"**（Day 8后变成屏幕提示）。
2. 之后打怪观察：敌人受击后即使飞刀没再命中也会持续掉血烧死（点燃）。
3. 换雷：命中时 Console 间歇打印"连锁闪电！额外伤害…"；换冰：敌人明显变慢。
4. 三个共鸣都拿到：效果叠加（点火+减速同时生效）。

---

# Day 7 — 旋刃 / 新星 / Boss

### Step 7.1 旋刃（Build C核心）
1. 建 `OrbitBlade` Prefab：Sprite(Square) 银白色 Scale(0.7,0.25,1) + Circle Collider 2D(**IsTrigger**, R=0.35) + 脚本 `OrbitBlade`。无Rigidbody。
2. Player 下建空子物体 `Weapon_Orbit`（0,0,0），挂 `OrbitWeapon`：Blade Prefab←拖 OrbitBlade；数值默认（15/270/2.2/2把）。
3. **取消勾选 Weapon_Orbit 的激活框**（初始关闭）。
4. Player 的 PlayerStats → `Orbit Weapon Object` ← 拖 Weapon_Orbit。
5. 测试：开局用Debug方式快速验证——运行中在 Inspector 勾选 Weapon_Orbit 激活 → 2片刀刃出现并旋转，碰到敌人持续掉血（同一敌人0.5秒一次）。然后取消勾选、通过选2次"旋刃强化"前先选"旋刃"从面板正常解锁。

### Step 7.2 新星（Build A核心）
1. Player 下建空子物体 `Weapon_Nova`（0,0,0），挂 `NovaWeapon`（数值默认 20/2.5/3.5）。**取消激活**。
2. PlayerStats → `Nova Weapon Object` ← 拖 Weapon_Nova。
3. 测试：激活后每2.5秒脚下范围内敌人集体掉血。

### Step 7.3 Boss子弹Prefab
建 `BossBullet`：红色小圆 Scale(0.3,0.3,1) + Circle Collider 2D(IsTrigger, R=0.2) + **Rigidbody 2D(Kinematic)** + 脚本 `EnemyBullet`。拖进Prefabs。

### Step 7.4 BossPrefab
1. 建 Sprite(Square) `Enemy_Boss`，暗红，Scale(3,3,1)。
2. Circle Collider 2D(**IsTrigger**, R=**0.5**——×Scale3后=世界半径1.5，与身体3×3一致。勿填1.5，会被Scale放大成4.5导致"没碰到就掉血")，Tag=**Boss**，挂 `Boss` 脚本。
3. Inspector：数值全默认（=Excel Boss_001）；`Bullet Prefab`←拖 BossBullet；`Summon Prefab`←拖 Enemy_Slime；ArenaHalfSize=21。
4. 拖进Prefabs。GameManager 的 EnemySpawner → `Boss Prefab`←拖 Enemy_Boss（Boss Time=360 已默认）。

### Step 7.5 Boss战测试（把 Boss Time 临时改成 10 秒快测！）
```
点Play（Boss Time已临时改为10）
↓ 10秒后Console红字"BOSS 来袭！"
↓ Boss缓慢逼近；每5秒闪红0.8秒后猛冲（看到闪红横移躲开）
↓ 每6秒一圈12发子弹；每8秒身边冒3只史莱姆
↓ 打死它：Console"胜利！用时…"，游戏冻结
↓ 被它打死：Console"游戏结束"
↓ 测完把 Boss Time 改回 360
```

**常见错误**
- Boss 不放技能 → Bullet Prefab / Summon Prefab 忘拖（技能内部有判空，静默跳过）。
- 冲锋永远躲不掉 → 前摇方向是**前摇结束时**才锁定的，这是设计；确认没改Boss.cs。
- 刀刃/新星打不动Boss → Boss 的 Tag 必须是 **Boss**（飞刀/刀刃/新星都检查了这个Tag）。
- 旋刃伤害一帧掉一串 → 别自己"优化"冷却逻辑，0.5秒冷却在 Enemy.TakeDamageOrbit 里。

---

# Day 8 — UI完善 + 结算 + 重开

### Step 8.1 HUD
在 Canvas 下建（都用 **Legacy** Text/Slider）：
1. `HpSlider`（UI → Slider）：左上，宽300高25；删掉 Handle Slide Area 子物体和底部的填充可留；把 Fill Rect 的 Image 颜色改红绿之间。Background 颜色暗灰。
2. `ExpSlider`：屏幕底部通栏（宽=屏幕宽-40，高15），Fill 改黄色。
3. `LevelText`：HP条旁边；`TimerText`：顶部居中；`KillText`：顶部右侧。
4. `ToastText`：屏幕正中（大号字，默认空内容）。
5. `BossHpBar`：空物体 + Slider（红色Fill）放顶部居中宽500，**初始取消激活**。

### Step 8.2 连接UIController
1. Canvas 挂 `UIController`。
2. 拖：Player Stats←Player；Game Manager←GameManager；HpSlider/ExpSlider/LevelText/TimerText/KillText/ToastText←对应物体；Boss Hp Bar←BossHpBar；Boss Hp Slider←BossHpBar下的Slider。
3. Player 的 PlayerStats → `Ui Controller` ← 拖 Canvas。
4. GameManager → `Game Over Panel` / `Victory Panel`：接下来建。

### Step 8.3 结算面板
1. 建 `GameOverPanel`（Panel，半透明黑，默认**取消激活**）：中央大字 Text"你死了"，下面小字（存活时间/击杀数可Day 9再说），一个 Button"重新开始"。
2. 按钮 OnClick → 点+ → 拖 GameManager 对象 → 选 **GameManager.Restart**。
3. 复制改字"胜利！"建 `VictoryPanel`（同样默认取消激活），按钮同样绑 Restart。
4. GameManager → 两个字段分别拖入两个面板。

### Step 8.4 全流程验收
```
点Play完整打一局
↓ HUD实时显示：血条/经验条/等级/计时/击杀数
↓ 升级三选一正常；共鸣触发屏幕中央弹提示（2秒消失）
↓ 6:00 Boss出现，顶部血条出现并实时下降
↓ 击杀Boss：VictoryPanel弹出，游戏冻结，点"重新开始"
↓ 场景重载，一切归零，timeScale=1，能正常再玩一局
↓ 被打死：GameOverPanel，同样能重开
```

**常见错误**
- 重开后游戏卡死不动 → **timeScale没恢复**。确认用的是 GameManager.Restart（内置 timeScale=1），没有自己另写重开逻辑。
- 重开按钮点了没反应 → 场景没加进 Build Settings（Day 0 Step 0.6）。
- UI文字不动 → UIController 的字段拖错物体（比如把 Slider 背景 Image 拖进了 Text 槽）。
- 共鸣提示不消失 → toastText 用的是 unscaled 计时，确认没改代码；字太长就缩字号。
- 血条/经验条永远不满格 → Slider 出厂 Fill Area 的 Right=-20 且 Handle 残留：删掉 Handle Slide Area 子物体 + Fill Area 的 Right 改 0。（注：血量 100/100 时绿条仍差一截+右端小圆点）

---

# Day 9 — 手感：音效 / 反馈 / 数值调整（半天音频+半天调数值）

### Step 9.1 音效（不用AudioManager）
1. 找免费音效包（Kenney 的 Impact/Pickup 类，CC0）拖进 `Assets/Audio/`。
2. （当前版本）音效钩子**已内置在代码里**（5 个脚本共 7 个 AudioClip 字段，全部判空，不填静音不报错），只需在对应 Prefab / 场景物体上拖音频文件：
   - `Projectile.hitClip`（Projectile_Dagger 预制体）→ 受击声
   - `Pickup.pickupClip`（Pickup_Gem / Pickup_Heart 预制体）→ 拾取声
   - `PlayerStats.levelUpClip / resonanceClip`（Player）→ 升级 / 共鸣
   - `Boss.appearClip`（Enemy_Boss 预制体）→ Boss 登场
   - `GameManager.gameOverClip / victoryClip`（GameManager）→ 胜负
   - 音量说明：`PlayClipAtPoint` 固定音量 1 且 Unity 上限即 1，**嫌小/嫌大换文件**（或用 Audacity ±6dB），不要试图改代码。
3. BGM：GameManager 物体 Add Component → **Audio Source**，拖一首循环曲，勾 **Loop**，Play On Awake=✓，Volume=0.4。

### Step 9.2 打击感（纯代码小改，各5行以内）
1. 敌人受击闪白：`Enemy.TakeDamage` 里 `GetComponent<SpriteRenderer>().color = new Color(2,2,2);`（配合材质HDR）——更简单方案：记录`sr.color = Color.red;`并在Update里 `if (sr.color != 白) sr.color = Color.Lerp(sr.color, Color.white, 10*Time.deltaTime);`
2. 死亡缩小：`Enemy.Die` 里 Destroy 前用 `transform.localScale *= 0.8f` 循环太麻烦——直接接受现在的直接消失，或加 `StartCoroutine` 缩放0.1秒（会写协程再弄，不会就跳过）。
3. 新星圆环：在 `NovaWeapon.Pulse` 里 Instantiate 一个白色圆环Sprite，0.2秒放大淡出（可选，不会协程就跳过）。

### Step 9.3 数值试玩（方法论）
自己完整玩5局，每局记录：**活到几分钟？哪一段最无聊？哪一段最劝退？**
常用调整（全部在Inspector，不动代码）：
- 前期太空 → Wave[0] SpawnInterval 1.5→1.2
- 死得太快 → 玩家 InvincibleTime 0.5→0.7，或 Wave 后段 interval 加大
- Boss 太硬 → BossPrefab 的 MaxHP 1500→1200（有依据，见03文档数值检查）
- 升级太慢 → PlayerStats 的 XpBase 5→4

---

# Day 10 — Bug修复与收尾

### Step 10.1 边界情况逐个测
| 场景 | 预期 |
|---|---|
| 一颗精英大宝石连升2级 | 面板弹2次，选完才恢复游戏 |
| 升级面板弹出瞬间被Boss子弹打中 | 子弹冻结，面板优先，选完恢复 |
| 玩家死亡瞬间正在升级 | 死亡结算优先，不再弹面板 |
| Boss存在时吃大宝石升级 | 正常（timeScale冻结一切） |
| 重开后再死一遍 | 一切正常（重点验证timeScale） |
| 全部强化选满后继续升级 | 面板可选项<3个甚至没有，直接恢复不弹 |

### Step 10.2 写README.txt放工程根目录
内容三段：Unity版本、怎么打开（打开Scene/Game）、操作说明（WASD移动，其它全自动）。

### Step 10.3 交付前跑一遍 `04_最终验收Checklist.md`
全绿 = Demo完成。
