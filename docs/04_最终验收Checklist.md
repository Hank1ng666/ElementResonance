# 04 · 最终验收 Checklist（Day 10 交付前逐条打勾）

## A. 工程健康
- [✓ ] Console 无红色报错（Play前后各看一次）
- [ ✓] 场景 `Game.unity` 已加入 Build Settings
- [✓ ] 4个Tag齐全：Player / Enemy / Wall / Boss
- [✓ ] Hierarchy 结构 = 00蓝图第3节（GameManager三脚本、Player三武器子物体、EnemyContainer、Canvas）

## B. 物理配置抽查（新手90%的bug在这里）
- [✓ ] 玩家 Rigidbody2D：Gravity Scale=0，Freeze Rotation Z=✓
- [✓ ] 玩家 CircleCollider2D：**不勾** IsTrigger
- [✓ ] 所有敌人/宝石/飞刀/刀刃/子弹/Boss 的 Collider：**全部勾了** IsTrigger
- [ ✓] 飞刀与Boss子弹的 Rigidbody2D：Body Type=Kinematic
- [ ✓] 四面墙：Collider **不勾** IsTrigger，Tag=Wall

## C. 连接抽查（对照00蓝图第5节）
- [ ✓] Weapon_Dagger.spawner → GameManager（最常见漏项）
- [✓ ] Player 的 PlayerStats：gameManager / 两把武器物体已拖
- [✓ ] EnemySpawner：player / playerStats / gameManager / enemyContainer 已拖
- [ ✓] UpgradeManager：面板+3按钮+3文本已拖
- [ ✓] 敌人Prefab：Exp Gem Prefab 已拖（精英还有Heart）

## D. 玩法流程（完整一局）
- [✓ ] WASD移动流畅，斜向不加速，撞墙不穿
- [✓ ] 敌人屏幕外出生、三种普通怪行为不同（直线/蛇形/慢肉）
- [✓ ] 飞刀自动打最近敌人；暴击时Console有体现（可临时加日志验证）
- [✓ ] 敌死掉宝石、宝石吸附、升级弹三选一、选后立即生效
- [✓ ] 同元素×2 → 共鸣提示 + 对应效果肉眼可见（点燃掉血/闪电弹射/减速）
- [✓ ] 2:30 和 5:00 精英出现，掉大宝石+回血包
- [✓ ]  难度随时间上升（怪更多更硬）
- [✓ ] 6:00 Boss：冲锋（前摇闪红）/环形弹幕/召唤三招齐全，顶部血条正常
- [✓ ] 击杀Boss → 胜利面板 → 重开正常
- [✓ ] 被打死 → 失败面板 → 重开正常
- [✓ ] **重开一局后 timeScale=1，一切数值归零，无残留敌人/宝石**

## E. 边界情况
- [✓ ] 大宝石连升2级：面板连续弹，选完才恢复
- [✓ ] 强化全拿满后升级：面板选项<3甚至跳过，不报错
- [✓ ] 升级面板期间被Boss子弹命中：无异常
- [✓ ] Boss死亡瞬间场上还有它的子弹：无异常（子弹自然超时）

## F. 交付物
- [✓ ] 工程根目录 README.txt（版本/打开方式/操作说明）
- [✓ ] 连续3局无报错无卡死
- [✓ ] 对照 `03_文档` 的数值结论玩过3局【竣工备注：Boss MaxHP 保持 1500 未动——Day 9 三轮调优后试玩 6:17 胜利，击杀体验达标】
- [✓ ] **（2026.09 竣工）全部项目已验收，Prototype 交付。**

**全部打勾 → 《元素共鸣》Prototype 完成。下一个版本的想法全部写进2.0愿望单，现在不做。**
