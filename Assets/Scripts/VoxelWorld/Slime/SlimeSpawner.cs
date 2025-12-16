using UnityEngine;

public class SlimeSpawner : MonoBehaviour
{
    [Header("생성 설정")]
    public GameObject slimePrefab;   // 비워두면 기본 슬라임 생성
    public float spawnInterval = 2f;
    public float spawnRadius = 1.5f;
    public int maxSlimes = 10;

    float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            TrySpawnSlime();
        }
    }

    // 리셋 직후 즉시 채우기
    public void SpawnBurst(int count)
    {
        for (int i = 0; i < count; i++)
            TrySpawnSlime();
    }

    // 리셋 직후 타이머 초기화
    public void ResetTimer()
    {
        timer = 0f;
    }

    void TrySpawnSlime()
    {
        int currentSlimes = GameObject.FindGameObjectsWithTag("Slime").Length;
        if (currentSlimes >= maxSlimes) return;

        Vector2 r = Random.insideUnitCircle * spawnRadius;
        Vector3 pos = transform.position + new Vector3(r.x, 0.5f, r.y);

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

        if (slimeObj.GetComponent<SlimeGridWalker>() == null)
            slimeObj.AddComponent<SlimeGridWalker>();
    }
}
