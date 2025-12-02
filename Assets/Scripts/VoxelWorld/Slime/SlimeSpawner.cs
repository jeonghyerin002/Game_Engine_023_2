using UnityEngine;

public class SlimeSpawner : MonoBehaviour
{
    [Header("생성 설정")]
    public GameObject slimePrefab;   // 비워두면 핑크 슬라임 자동 생성
    public float spawnInterval = 2f;
    public float spawnRadius = 1.5f;
    public int maxSlimes = 10;

    [Header("충돌 체크 (선택)")]
    public LayerMask obstacleMask;
    public float slimeCheckRadius = 0.4f;

    float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            TrySpawnSlime();
        }
    }

    void TrySpawnSlime()
    {
        // 너무 많으면 생성 안함
        int currentSlimes = GameObject.FindGameObjectsWithTag("Slime").Length;
        if (currentSlimes >= maxSlimes) return;

        // 스폰 위치 찾기
        Vector3 pos = GetSpawnPosition();

        GameObject slimeObj;

        if (slimePrefab != null)
        {
            slimeObj = Instantiate(slimePrefab, pos, Quaternion.identity);
        }
        else
        {
            // 프리팹 없으면 핑크 슬라임 구체 생성
            slimeObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            slimeObj.transform.position = pos;
            slimeObj.transform.localScale = Vector3.one * 0.8f;

            var renderer = slimeObj.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(1f, 0.5f, 1f); // 핑크 슬라임
                mat.SetFloat("_Metallic", 0.1f);
                mat.SetFloat("_Glossiness", 0.8f);
                renderer.material = mat;
            }

            slimeObj.AddComponent<Rigidbody>();
        }

        slimeObj.tag = "Slime";

        // 쫀득 점프/채집 로직 붙이고 싶으면 여기서 스크립트 추가
        if (slimeObj.GetComponent<SlimeHarvester>() == null)
            slimeObj.AddComponent<SlimeHarvester>();
    }

    Vector3 GetSpawnPosition()
    {
        for (int i = 0; i < 8; i++)
        {
            Vector2 r = Random.insideUnitCircle * spawnRadius;
            Vector3 candidate = transform.position + new Vector3(r.x, 0.5f, r.y);

            if (obstacleMask.value != 0)
            {
                bool blocked = Physics.CheckSphere(candidate, slimeCheckRadius, obstacleMask);
                if (blocked) continue;
            }

            return candidate;
        }

        // 대체: 그냥 자기 위치 근처
        return transform.position + Vector3.up * 0.5f;
    }
}
