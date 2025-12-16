using UnityEngine;

public class SlimeSpawner : MonoBehaviour
{
    [Header("생성 설정")]
    public GameObject slimePrefab;
    public float spawnInterval = 2f;
    public float spawnRadius = 1.5f;
    public int maxSlimes = 10;

    float timer = 0f;

    [Header("추가 생성(업그레이드) 가격 테이블")]
    public long[] extraSpawnCosts = new long[] { 0, 500, 5000, 50000, 500000 };

    [Header("현재 추가 생성 구매 횟수(저장하려면 따로 세이브에 포함)")]
    public int extraSpawnPurchased = 0;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            TrySpawnSlime();
        }
    }

    public void SpawnBurst(int count)
    {
        for (int i = 0; i < count; i++)
            TrySpawnSlime();
    }

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
            slimeObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            slimeObj.transform.position = pos;
            slimeObj.transform.localScale = Vector3.one * 0.8f;

            var renderer = slimeObj.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(1f, 0.5f, 1f);
                mat.SetFloat("_Metallic", 0.1f);
                mat.SetFloat("_Glossiness", 0.8f);
                renderer.material = mat;
            }

            slimeObj.AddComponent<Rigidbody>();
        }

        slimeObj.tag = "Slime";

        // 기존처럼 GridWalker를 붙이고 싶으면 유지, 싫으면 제거
        if (slimeObj.GetComponent<SlimeGridWalker>() == null)
            slimeObj.AddComponent<SlimeGridWalker>();
    }

    // ============================
    // 추가 생성 구매 (가격: 0, 500, 5000, 50000, 500000 ...)
    // - 효과: maxSlimes +1, 즉시 1마리 더 뽑기
    // ============================
    public long GetNextExtraSpawnCost()
    {
        if (extraSpawnCosts == null || extraSpawnCosts.Length == 0) return 0;
        int idx = Mathf.Clamp(extraSpawnPurchased, 0, extraSpawnCosts.Length - 1);
        return extraSpawnCosts[idx];
    }

    public bool TryBuyExtraSpawn()
    {
        var gm = SlimeGameManager.Instance;
        if (gm == null) return false;

        long cost = GetNextExtraSpawnCost();
        if (!gm.CanSpendResource(ResourceType.Coin, cost)) return false;
        if (!gm.SpendResource(ResourceType.Coin, cost)) return false;

        extraSpawnPurchased++;
        maxSlimes += 1;

        // 즉시 1마리 더 생성 시도(슬롯 늘렸으니 대부분 바로 나옴)
        TrySpawnSlime();

        return true;
    }
}
