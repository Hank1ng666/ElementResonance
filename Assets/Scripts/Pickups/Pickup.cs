// ─── 文件位置: Assets/Scripts/Pickups/Pickup.cs
// ─── 挂载对象: Pickup_Gem 预制体（expValue=1）和 Pickup_Heart 预制体（healAmount=30, expValue=0）
// ─── 说明: 一个脚本通吃经验宝石和回血包。进入玩家拾取半径后加速飞向玩家。

using UnityEngine;

public class Pickup : MonoBehaviour
{
    [Header("宝石填expValue, 回血包填healAmount")]
    public int expValue = 1;      // 宝石: 1（精英大宝石由敌人死亡时覆盖为10）
    public int healAmount = 0;    // 回血包: 30
    public float magnetAccel = 30f; // 吸附加速度

    [Header("音效（Day 9，可不填）")]
    public AudioClip pickupClip;

    PlayerStats playerStats;
    float magnetSpeed;
    bool collected;

    // 敌人死亡时调用，注入玩家引用
    public void Init(PlayerStats ps)
    {
        playerStats = ps;
    }

    void Update()
    {
        if (collected || playerStats == null || playerStats.isDead) return;

        float dist = Vector2.Distance(playerStats.transform.position, transform.position);
        if (dist < playerStats.pickupRadius)
        {
            magnetSpeed += magnetAccel * Time.deltaTime; // 越吸越快
            transform.position = Vector3.MoveTowards(transform.position,
                playerStats.transform.position, magnetSpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player")) return;
        collected = true;
        if (pickupClip != null) AudioSource.PlayClipAtPoint(pickupClip, transform.position);

        if (healAmount > 0) playerStats.Heal(healAmount);
        else playerStats.GainEXP(expValue);

        Destroy(gameObject);
    }
}
