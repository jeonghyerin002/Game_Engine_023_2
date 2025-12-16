using UnityEngine;

public class VoxelClickSpawner : MonoBehaviour
{
    [Header("필수 레퍼런스")]
    public VoxelWorld voxelWorld;
    public SlimeBuildUI buildUI;
    public SlimePlanetSaveManager save; // 행성별 저장매니저

    [Header("프리팹 (없으면 큐브로 생성)")]
    public GameObject copperPrefab;
    public GameObject silverPrefab;
    public GameObject goldPrefab;
    public GameObject metalPrefab;
    public GameObject mithrilPrefab;
    public GameObject slimeSpawnerPrefab;

    [Header("정리용 부모(없으면 자동 찾기/생성)")]
    public Transform oreParent;
    public Transform spawnerParent;

    [Header("비용 사용")]
    public bool enableCost = true;

    void Start()
    {
        if (save == null) save = FindObjectOfType<SlimePlanetSaveManager>();

        if (oreParent == null)
        {
            var found = GameObject.Find("OreObjects");
            if (found == null) found = new GameObject("OreObjects");
            oreParent = found.transform;
        }

        if (spawnerParent == null)
        {
            var found = GameObject.Find("SlimeSpawners");
            if (found == null) found = new GameObject("SlimeSpawners");
            spawnerParent = found.transform;
        }
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        if (voxelWorld == null || voxelWorld.Picker == null) return;
        if (buildUI == null) return;

        if (!buildUI.TryGetBuildRequest(out var req)) return;

        var cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        var placePos = voxelWorld.Picker.GetPlacementPosition(ray);
        if (!placePos.HasValue) return;

        Vector3Int p = placePos.Value;
        Vector3 worldPos = new Vector3(p.x + 0.5f, p.y + 0.5f, p.z + 0.5f);

        var gm = SlimeGameManager.Instance;

        if (enableCost && gm != null)
        {
            if (!gm.CanSpendResource(ResourceType.Coin, req.costCoin)) return;
            if (!gm.SpendResource(ResourceType.Coin, req.costCoin)) return;
        }

        if (req.isSpawner)
        {
            SpawnSpawner(worldPos);
            save?.SaveNow(); // 설치 직후 행성별 저장
        }
        else
        {
            SpawnOre(req.oreType, GetOrePrefab(req.oreType), worldPos);
            save?.SaveNow(); // 설치 직후 행성별 저장
        }
    }

    GameObject GetOrePrefab(ResourceType type)
    {
        return type switch
        {
            ResourceType.Copper => copperPrefab,
            ResourceType.Silver => silverPrefab,
            ResourceType.Gold => goldPrefab,
            ResourceType.Metal => metalPrefab,
            ResourceType.Mithril => mithrilPrefab,
            _ => null
        };
    }

    void SpawnSpawner(Vector3 worldPos)
    {
        GameObject spawnerObj;

        if (slimeSpawnerPrefab != null)
            spawnerObj = Instantiate(slimeSpawnerPrefab, worldPos, Quaternion.identity, spawnerParent);
        else
        {
            spawnerObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spawnerObj.transform.SetParent(spawnerParent);
            spawnerObj.transform.position = worldPos;
            spawnerObj.transform.localScale = Vector3.one * 0.85f;
        }

        spawnerObj.name = "SlimeSpawner";

        var comp = spawnerObj.GetComponent<SlimeSpawner>();
        if (comp == null) spawnerObj.AddComponent<SlimeSpawner>();
    }

    void SpawnOre(ResourceType type, GameObject prefab, Vector3 worldPos)
    {
        GameObject ore;

        if (prefab != null)
            ore = Instantiate(prefab, worldPos, Quaternion.identity, oreParent);
        else
        {
            ore = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ore.transform.SetParent(oreParent);
            ore.transform.position = worldPos;
            ore.transform.localScale = Vector3.one * 0.9f;
        }

        ore.name = type + "_Ore";

        var marker = ore.GetComponent<PlacedOreMarker>();
        if (marker == null) marker = ore.AddComponent<PlacedOreMarker>();
        marker.oreType = type;
    }
}
