using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SlimeHarvester : MonoBehaviour
{
    public float jumpForce = 5f;
    public float moveRadius = 2f;
    public float moveForce = 2f;
    public float jumpIntervalMin = 0.8f;
    public float jumpIntervalMax = 1.4f;

    Rigidbody rb;
    Vector3 homePos;
    float nextJumpTime;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        homePos = transform.position;
        ScheduleNextJump();
    }

    void Update()
    {
        // 쫀득 스케일 (기존 코드)
        float vy = rb.velocity.y;
        float stretch = Mathf.Clamp(1f + (-vy * 0.1f), 0.6f, 1.4f);
        float squash = 1f / Mathf.Sqrt(stretch);
        transform.localScale = new Vector3(squash, stretch, squash);

        // 점프 타이밍 (기존 코드)
        if (Time.time >= nextJumpTime)
        {
            TryJump();
            ScheduleNextJump();
        }

        KeepInsideWorldBounds();
    }

    void KeepInsideWorldBounds()
    {
        // SlimeGameManager.Instance.voxelWorld 에 VoxelWorld가 연결돼 있다고 가정
        var gm = SlimeGameManager.Instance;
        if (gm == null || gm.voxelWorld == null) return;

        VoxelWorld world = gm.voxelWorld;

        // 월드 바운더리 (0~width, 0~depth 기준으로 살짝 안쪽으로)
        float minX = 0.5f;
        float maxX = world.width - 0.5f;
        float minZ = 0.5f;
        float maxZ = world.depth - 0.5f;

        Vector3 pos = transform.position;
        bool outOfBounds = false;

        // XZ 경계 클램프
        if (pos.x < minX) { pos.x = minX; outOfBounds = true; }
        if (pos.x > maxX) { pos.x = maxX; outOfBounds = true; }
        if (pos.z < minZ) { pos.z = minZ; outOfBounds = true; }
        if (pos.z > maxZ) { pos.z = maxZ; outOfBounds = true; }

        // 너무 아래로 떨어졌으면 지형 표면 위로 복귀
        if (pos.y < -5f)
        {
            int bx = Mathf.Clamp(Mathf.RoundToInt(pos.x), 0, world.width - 1);
            int bz = Mathf.Clamp(Mathf.RoundToInt(pos.z), 0, world.depth - 1);

            int surfaceY = world.Data.GetSurfaceHeight(bx, bz);
            pos.y = surfaceY + 1.2f;   // 지형 위로 살짝 띄우기
            outOfBounds = true;
        }

        if (outOfBounds)
        {
            transform.position = pos;

            // 경계에 부딪힌 느낌으로 속도 조금 죽이기
            rb.velocity = new Vector3(
                rb.velocity.x * 0.3f,
                rb.velocity.y,
                rb.velocity.z * 0.3f
            );
        }
    }

    void ScheduleNextJump()
    {
        nextJumpTime = Time.time + Random.Range(jumpIntervalMin, jumpIntervalMax);
    }

    void TryJump()
    {
        if (!IsGrounded()) return;

        Vector2 r = Random.insideUnitCircle.normalized;
        Vector3 dir = new Vector3(r.x, 0f, r.y) * moveForce;

        rb.AddForce(dir + Vector3.up * jumpForce, ForceMode.Impulse);
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 0.6f);
    }

    void OnCollisionEnter(Collision collision)
    {
        // 1) 광석 위로 착지했는지 체크 (프리팹 태그)
        string t = collision.gameObject.tag;
        if (t.StartsWith("Ore"))
        {
            SlimeGameManager.Instance.CollectOreByTag(t);
            Destroy(collision.gameObject);
            return;
        }

        // 2) 지형위 착지인 경우, 아래 블록 타입 확인
        //    -> 광석이 아닌 일반 지형일 때만
        CheckVoxelGround();
    }

    void CheckVoxelGround()
    {
        var gm = SlimeGameManager.Instance;
        if (gm == null || gm.voxelWorld == null) return;

        Ray ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 3f))
        {
            // 월드 좌표 -> 블록 좌표 (VoxelPicker의 WorldToBlock과 동일한 방식) :contentReference[oaicite:3]{index=3}
            Vector3 p = hit.point;
            int bx = Mathf.FloorToInt(p.x + 0.5f);
            int by = Mathf.FloorToInt(p.y + 0.5f);
            int bz = Mathf.FloorToInt(p.z + 0.5f);

            var blockType = gm.voxelWorld.Data.GetBlock(bx, by, bz);
            gm.CollectBlock(blockType);
        }
    }
}
