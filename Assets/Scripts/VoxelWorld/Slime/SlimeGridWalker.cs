using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeGridWalker : MonoBehaviour
{
    [Header("이동 주기")]
    public float stepInterval = 0.8f;      // 다음 칸으로 이동 시도 간격 (초)
    public float moveTime = 0.25f;         // 한 칸 이동에 걸리는 시간

    [Header("점프 연출")]
    public float hopHeight = 0.4f;         // 점프 아크 높이
    public float slimeHeightOffset = 0.5f; // 지면에서 얼마나 띄울지
    public bool useSquashStretch = true;
    public float stretchScale = 1.3f;      // 점프 중간에 위로 늘어나는 비율
    public float squashScale = 0.7f;       // 점프 중간에 옆으로 찌그러지는 비율

    [Header("지형 판정")]
    public float scanHeight = 2f;          // 레이캐스트 시작 높이 (현재 위치 기준 위쪽)
    public float maxStepUp = 1.5f;         // 한 번에 올라갈 수 있는 최대 높이 차이
    public float maxStepDown = 2f;         // 한 번에 내려갈 수 있는 최대 높이 차이
    public LayerMask groundMask = ~0;      // 땅으로 쓸 레이어 (기본: 전부)

    [Header("후보 칸 설정")]
    public bool includeCenterCell = false; // true면 제자리 점프도 가능

    bool isMoving = false;
    float timer = 0f;
    Vector3 baseScale;
    float lastGroundY;     // 현재 슬라임이 서 있는 지면 높이 추정

    Rigidbody rb;

    void Start()
    {
        baseScale = transform.localScale;

        // 혹시 Rigidbody 있으면 물리 끄기 (낙하 방지)
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // 시작 시 한번 지면 높이 샘플
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

    // 현재 위치 기준으로 아래로 레이 쏴서 지면 높이 업데이트
    void UpdateGroundY()
    {
        Vector3 origin = transform.position + Vector3.up * scanHeight;
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, scanHeight * 3f, groundMask))
        {
            lastGroundY = hit.point.y;
        }
        else
        {
            // 못 찾으면 현재 Y를 기준으로 잡음
            lastGroundY = transform.position.y - slimeHeightOffset;
        }
    }

    void TryStep()
    {
        UpdateGroundY();

        // 현재 발밑 기준으로 그리드 중앙 좌표 계산 (1유닛 격자)
        int centerX = Mathf.RoundToInt(transform.position.x);
        int centerZ = Mathf.RoundToInt(transform.position.z);

        List<Vector3> candidates = new List<Vector3>();

        for (int dz = -1; dz <= 1; dz++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                if (!includeCenterCell && dx == 0 && dz == 0)
                    continue; // 제자리 제외 옵션

                Vector3 cellCenter = new Vector3(centerX + dx, lastGroundY + scanHeight, centerZ + dz);

                // 각 칸의 위에서 아래로 레이캐스트
                if (Physics.Raycast(cellCenter, Vector3.down, out RaycastHit hit,
                                    scanHeight * 3f, groundMask))
                {
                    float groundY = hit.point.y;
                    float deltaY = groundY - lastGroundY;

                    // 너무 높거나 너무 낮은 칸은 제외
                    if (deltaY > maxStepUp) continue;
                    if (deltaY < -maxStepDown) continue;

                    Vector3 target = hit.point + Vector3.up * slimeHeightOffset;
                    candidates.Add(target);
                }
            }
        }

        if (candidates.Count == 0)
        {
            // 갈만한 칸이 없으면 제자리
            return;
        }

        // 후보 중 랜덤으로 하나 선택
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

            // 호(arc) 모양 점프
            float arc = Mathf.Sin(eval * Mathf.PI);
            float yOffset = arc * hopHeight;

            Vector3 pos = Vector3.Lerp(startPos, endPos, eval);
            pos.y += yOffset;
            transform.position = pos;

            // 쫀득 스쿼시/스트레치
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
        transform.localScale = baseScale; // 스케일 원복
        isMoving = false;

        // 도착한 칸에서 무언가 하고 싶으면 여기서 처리
        OnArrivedTile(endPos);
    }

    void OnArrivedTile(Vector3 worldPos)
    {
        // 여기에 광석 채집 / 자원 획득 / 이펙트 등 넣으면 됨.
        // 예:
        // Collider[] hits = Physics.OverlapSphere(worldPos, 0.4f);
        // ...
    }
}

