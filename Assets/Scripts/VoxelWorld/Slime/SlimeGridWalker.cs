using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeGridWalker : MonoBehaviour
{
    [Header("이동 설정")]
    public float stepInterval = 0.8f;
    public float moveTime = 0.25f;

    [Header("점프 연출")]
    public float hopHeight = 0.4f;
    public float slimeHeightOffset = 0.5f;
    public bool useSquashStretch = true;
    public float stretchScale = 1.3f;
    public float squashScale = 0.7f;

    [Header("지형 판정")]
    public float scanHeight = 2f;
    public float maxStepUp = 1.5f;
    public float maxStepDown = 2f;
    public LayerMask groundMask;   // Ground 레이어만 지정 권장

    [Header("이동 후보")]
    public bool includeCenterCell = false;

    [Header("수확 설정")]
    public bool harvestOres = true;            // 광맥(오브젝트 태그)
    public bool harvestGroundBlocks = true;    // 땅(BlockType)
    public float harvestRadius = 0.45f;
    public LayerMask harvestMask = ~0;

    [Header("광맥 수확 쿨다운")]
    public float oreHarvestCooldown = 0.5f;

    bool isMoving;
    float timer;
    float lastGroundY;
    Vector3 baseScale;

    Rigidbody rb;

    Dictionary<int, float> lastHarvestTimeByOre = new Dictionary<int, float>();

    void Start()
    {
        baseScale = transform.localScale;

        groundMask = LayerMask.GetMask("Ground");

        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        UpdateGroundY();
    }

    void Update()
    {
        if (isMoving) return;

        timer += Time.deltaTime;
        if (timer >= stepInterval)
        {
            timer = 0f;
            TryStep();
        }
    }

    // -------------------------
    // 이동
    // -------------------------

    void UpdateGroundY()
    {
        Vector3 origin = transform.position + Vector3.up * scanHeight;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, scanHeight * 3f, groundMask))
        {
            lastGroundY = hit.point.y;
        }
        else
        {
            lastGroundY = transform.position.y - slimeHeightOffset;
        }
    }

    void TryStep()
    {
        UpdateGroundY();

        int cx = Mathf.RoundToInt(transform.position.x);
        int cz = Mathf.RoundToInt(transform.position.z);

        List<Vector3> candidates = new List<Vector3>();

        for (int dz = -1; dz <= 1; dz++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                if (!includeCenterCell && dx == 0 && dz == 0)
                    continue;

                Vector3 rayOrigin = new Vector3(cx + dx, lastGroundY + scanHeight, cz + dz);

                if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, scanHeight * 3f, groundMask))
                {
                    float groundY = hit.point.y;
                    float deltaY = groundY - lastGroundY;

                    if (deltaY > maxStepUp) continue;
                    if (deltaY < -maxStepDown) continue;

                    Vector3 target = hit.point + Vector3.up * slimeHeightOffset;
                    candidates.Add(target);
                }
            }
        }

        if (candidates.Count == 0) return;

        Vector3 targetPos = candidates[Random.Range(0, candidates.Count)];
        StopAllCoroutines();
        StartCoroutine(JumpRoutine(transform.position, targetPos));
    }

    IEnumerator JumpRoutine(Vector3 startPos, Vector3 endPos)
    {
        isMoving = true;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / moveTime;
            float eval = Mathf.Clamp01(t);

            float arc = Mathf.Sin(eval * Mathf.PI);
            float yOffset = arc * hopHeight;

            Vector3 pos = Vector3.Lerp(startPos, endPos, eval);
            pos.y += yOffset;
            transform.position = pos;

            if (useSquashStretch)
            {
                float yScale = Mathf.Lerp(1f, stretchScale, arc);
                float xzScale = Mathf.Lerp(1f, squashScale, arc);
                transform.localScale = new Vector3(
                    baseScale.x * xzScale,
                    baseScale.y * yScale,
                    baseScale.z * xzScale
                );
            }

            yield return null;
        }

        transform.position = endPos;
        transform.localScale = baseScale;
        isMoving = false;

        OnArrivedTile(endPos);
    }

    // -------------------------
    // 도착 & 수확
    // -------------------------

    void OnArrivedTile(Vector3 worldPos)
    {
        HarvestAt(worldPos);
    }

    void HarvestAt(Vector3 worldPos)
    {
        var gm = SlimeGameManager.Instance;
        if (gm == null) return;

        // 1) 광맥 오브젝트 수확 (계속 남아있음)
        if (harvestOres)
        {
            Collider[] hits = Physics.OverlapSphere(
                worldPos,
                harvestRadius,
                harvestMask,
                QueryTriggerInteraction.Collide
            );

            foreach (var hit in hits)
            {
                GameObject go = hit.gameObject;
                if (go == gameObject) continue;

                string tag = go.tag;

                bool isOre =
                    tag == "Copper" || tag == "Silver" || tag == "Gold" ||
                    tag == "Metal" || tag == "Mithril" || tag == "Soil" ||
                    tag.StartsWith("Ore");

                if (!isOre) continue;

                if (oreHarvestCooldown > 0f)
                {
                    int id = go.GetInstanceID();
                    float now = Time.time;

                    if (lastHarvestTimeByOre.TryGetValue(id, out float last) &&
                        now - last < oreHarvestCooldown)
                        continue;

                    lastHarvestTimeByOre[id] = now;
                }

                gm.CollectOreByTag(tag);
                break; // 한 번만 수확
            }
        }

        // 2) 땅(BlockType) 수확 → Soil
        if (harvestGroundBlocks && gm.voxelWorld != null && gm.voxelWorld.Data != null)
        {
            int bx = Mathf.RoundToInt(worldPos.x);
            int bz = Mathf.RoundToInt(worldPos.z);

            bx = Mathf.Clamp(bx, 0, gm.voxelWorld.Data.Width - 1);
            bz = Mathf.Clamp(bz, 0, gm.voxelWorld.Data.Depth - 1);

            int surfaceY = gm.voxelWorld.Data.GetSurfaceHeight(bx, bz);
            var blockType = gm.voxelWorld.Data.GetBlock(bx, surfaceY, bz);

            // 핵심: BlockType 그대로 넘김
            gm.CollectBlock(blockType);

#if UNITY_EDITOR
            Debug.Log($"[SlimeGridWalker] Harvest BlockType = {blockType}");
#endif
        }
    }

    // -------------------------
    // 디버그
    // -------------------------

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, harvestRadius);
    }
}
