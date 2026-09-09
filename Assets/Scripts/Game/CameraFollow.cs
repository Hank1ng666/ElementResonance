// ─── 文件位置: Assets/Scripts/Game/CameraFollow.cs
// ─── 挂载对象: Main Camera
// ─── 说明: 平滑跟随玩家。offset 的 z=-10 保持俯视距离。

using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;                                // 拖 Player
    public Vector3 offset = new Vector3(0f, 0f, -10f);
    public float followSpeed = 5f;

    void LateUpdate()
    {
        if (target == null) return;
        transform.position = Vector3.Lerp(transform.position,
            target.position + offset, followSpeed * Time.deltaTime);
    }
}
