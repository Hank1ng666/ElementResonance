// ─── 文件位置: Assets/Scripts/Player/PlayerController.cs
// ─── 挂载对象: Player（同一个物体上必须有 PlayerStats）
// ─── 说明: 只负责读输入和移动。速度统一在 PlayerStats 里调（baseMoveSpeed × moveSpeedMultiplier）。

using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rb;
    PlayerStats stats;
    Vector2 input;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal"); // A/D 和左右方向键
        float y = Input.GetAxisRaw("Vertical");   // W/S 和上下方向键
        input = new Vector2(x, y).normalized;     // 归一化防止斜向更快
    }

    void FixedUpdate()
    {
        if (rb == null) return;
        if (stats != null && stats.isDead)
        {
            rb.velocity = Vector2.zero;
            return;
        }
        float speed = stats != null ? stats.baseMoveSpeed * stats.moveSpeedMultiplier : 5f;
        rb.velocity = input * speed;
    }
}
