using UnityEngine;

public class VoxelClickSpawner : MonoBehaviour
{
    [Header("필수 레퍼런스")]
    public VoxelWorld voxelWorld;
    public SlimeBuildUI buildUI;

    [Header("프리팹 (없으면 큐브로 생성)")]
    public GameObject copperPrefab;
    public GameObject silverPrefab;
    public GameObject goldPrefab;
    public GameObject metalPrefab;
    public GameObject mithrilPrefab;
    public GameObject slimeSpawnerPrefab;

    [Header("정리용 부모")]
    public Transform oreParent;
    public Transform spawnerParent;

    [Header("비용 사용")]
    public bool enableCost = true;

    void Start()
    {
        if (oreParent == null)
        {
            var o = new GameObject("OreObjects");
            oreParent = o.transform;
        }

        if (spawnerParent == null)
        {
            var s = new GameObject("SlimeSpawners");
            spawnerParent = s.transform;
        }
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        if (voxelWorld == null || voxelWorld.Picker == null) return;
        if (buildUI == null) return;

        // UI에게 "이번에 뭘 설치할지"만 물어봄 (모드/비용/타입은 UI가 책임)
        if (!buildUI.TryGetBuildRequest(out var req)) return;

        var cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        var placePos = voxelWorld.Picker.GetPlacementPosition(ray);
        if (!placePos.HasValue) return;

        Vector3Int blockPos = placePos.Value;
        Vector3 worldPos = new Vector3(blockPos.x, blockPos.y, blockPos.z);

        var gm = SlimeGameManager.Instance;

        // 비용 처리(여기 한 곳에서만)
        if (enableCost && gm != null)
        {
            if (!gm.CanSpendResource(ResourceType.Coin, req.costCoin)) return;
            if (!gm.SpendResource(ResourceType.Coin, req.costCoin)) return;
        }

        // 설치 실행
        if (req.isSpawner)
        {
            SpawnSpawner(worldPos);
        }
        else
        {
            GameObject prefab = GetOrePrefab(req.oreType);
            SpawnOre(req.oreType, prefab, worldPos);
        }
    }

    GameObject GetOrePrefab(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Copper: return copperPrefab;
            case ResourceType.Silver: return silverPrefab;
            case ResourceType.Gold: return goldPrefab;
            case ResourceType.Metal: return metalPrefab;
            case ResourceType.Mithril: return mithrilPrefab;
            default: return null;
        }
    }

    void SpawnSpawner(Vector3 worldPos)
    {
        GameObject spawnerObj;

        if (slimeSpawnerPrefab != null)
        {
            spawnerObj = Instantiate(
                slimeSpawnerPrefab,
                worldPos + Vector3.up * 0.5f,
                Quaternion.identity,
                spawnerParent
            );
        }
        else
        {
            spawnerObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spawnerObj.transform.position = worldPos + Vector3.up * 0.5f;
            spawnerObj.transform.localScale = Vector3.one * 0.85f;
            spawnerObj.transform.SetParent(spawnerParent);

            var renderer = spawnerObj.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(1f, 0.6f, 1f);
                mat.SetFloat("_Metallic", 0.2f);
                mat.SetFloat("_Glossiness", 0.7f);
                renderer.material = mat;
            }
        }

        spawnerObj.name = "SlimeSpawner";

        var spawnerComp = spawnerObj.GetComponent<SlimeSpawner>();
        if (spawnerComp == null)
            spawnerComp = spawnerObj.AddComponent<SlimeSpawner>();
    }

    void SpawnOre(ResourceType type, GameObject prefab, Vector3 worldPos)
    {
        GameObject ore;

        if (prefab != null)
        {
            ore = Instantiate(prefab, worldPos, Quaternion.identity, oreParent);
        }
        else
        {
            ore = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ore.transform.position = worldPos;
            ore.transform.localScale = Vector3.one * 0.9f;
            ore.transform.SetParent(oreParent);

            var renderer = ore.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Standard"));
                mat.color = GetOreColor(type);
                mat.SetFloat("_Metallic", 0.9f);
                mat.SetFloat("_Glossiness", 0.75f);
                renderer.material = mat;
            }
        }

        ore.name = type + "_Ore";
        ore.tag = type.ToString(); // CollectOreByTag가 이해하는 태그 (Copper/Silver/Gold/Metal/Mithril)
    }

    Color GetOreColor(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Copper: return new Color(0.784f, 0.459f, 0.2f);
            case ResourceType.Silver: return new Color(0.79f, 0.81f, 0.84f);
            case ResourceType.Gold: return new Color(0.949f, 0.788f, 0.298f);
            case ResourceType.Metal: return new Color(0.48f, 0.54f, 0.60f);
            case ResourceType.Mithril: return new Color(0.48f, 0.42f, 1.0f);
            default: return Color.gray;
        }
    }
}
