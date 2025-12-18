using UnityEngine;
using UnityEngine.EventSystems;

public class VoxelClickSpawner : MonoBehaviour
{
    [Header("필수 레퍼런스")]
    public VoxelWorld voxelWorld;
    public SlimePlanetSaveManager save;

    [Header("빌드 UI (광석 / 스포너)")]
    public SlimeBuildUI buildUI;

    [Header("토탬 UI")]
    public TotemBuildUI totemUI;

    [Header("토탬 프리팹 라이브러리 (6종)")]
    public TotemPrefabLibrary totemPrefabs;

    [Header("광석 프리팹")]
    public GameObject copperPrefab;
    public GameObject silverPrefab;
    public GameObject goldPrefab;
    public GameObject metalPrefab;
    public GameObject mithrilPrefab;

    [Header("스포너 프리팹")]
    public GameObject slimeSpawnerPrefab;

    [Header("부모 오브젝트")]
    public Transform oreParent;
    public Transform spawnerParent;
    public Transform totemParent;

    [Header("비용 사용")]
    public bool enableCost = true;

    [Header("UI 클릭 차단")]
    public bool blockWorldClickWhenPointerOverUI = true;

    [Header("스포너 설치 가격 증가")]
    public long spawnerBaseCost = 0;
    public long spawnerCostStep = 1000;

    public OreMaterialBank oreMats;

    void Start()
    {
        if (save == null) save = FindObjectOfType<SlimePlanetSaveManager>();
        if (totemPrefabs == null) totemPrefabs = FindObjectOfType<TotemPrefabLibrary>();
        if (oreMats == null) oreMats = FindObjectOfType<OreMaterialBank>();

        EnsureParent(ref oreParent, "OreObjects");
        EnsureParent(ref spawnerParent, "SlimeSpawners");
        EnsureParent(ref totemParent, "TotemTowers");
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        // UI 위 클릭이면 월드 설치 차단
        if (blockWorldClickWhenPointerOverUI && IsPointerOverUI())
            return;

        if (voxelWorld == null || voxelWorld.Picker == null) return;

        var cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        var placePos = voxelWorld.Picker.GetPlacementPosition(ray);
        if (!placePos.HasValue) return;

        Vector3Int p = placePos.Value;
        Vector3 worldPos = new Vector3(p.x + 0.5f, p.y + 0.5f, p.z + 0.5f);

        // =================================================
        // 1️토탬 설치 (최우선)
        // =================================================
        if (totemUI != null && totemUI.TryGetTotemRequest(out TotemType tType, out long tCost))
        {
            if (!TryPayCoin(tCost)) return;

            SpawnTotem(tType, worldPos);
            totemUI.ClearSelection();
            save?.SaveNow();
            return;
        }

        // =================================================
        // 2️광석 / 스포너 설치
        // =================================================
        if (buildUI == null) return;
        if (!buildUI.TryGetBuildRequest(out var req)) return;

        if (req.isSpawner)
        {
            int count = GetCurrentSpawnerCount();
            req.costCoin = spawnerBaseCost + count * spawnerCostStep;
        }

        if (!TryPayCoin(req.costCoin)) return;

        if (req.isSpawner)
        {
            SpawnSpawner(worldPos);
        }
        else
        {
            SpawnOre(req.oreType, GetOrePrefab(req.oreType), worldPos);
        }

        save?.SaveNow();
    }

    // =================================================
    // 비용 처리
    // =================================================
    bool TryPayCoin(long cost)
    {
        if (!enableCost) return true;

        var gm = SlimeGameManager.Instance;
        if (gm == null) return false;

        if (!gm.CanSpendResource(ResourceType.Coin, cost)) return false;
        if (!gm.SpendResource(ResourceType.Coin, cost)) return false;
        return true;
    }

    // =================================================
    // Spawn
    // =================================================
    void SpawnOre(ResourceType type, GameObject prefab, Vector3 pos)
    {
        GameObject ore;

        if (prefab != null)
            ore = Instantiate(prefab, pos, Quaternion.identity, oreParent);
        else
        {
            ore = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ore.transform.SetParent(oreParent);
            ore.transform.position = pos;
            ore.transform.localScale = Vector3.one * 0.9f;

            var r = ore.GetComponent<MeshRenderer>();
            if (r != null && oreMats != null)
            {
                var mat = oreMats.Get(type);
                if (mat != null) r.sharedMaterial = mat; // sharedMaterial로 메모리 절약
            }

            OreVisualUtil.ApplyOreMaterial(ore, type);
        }

        ore.name = type + "_Ore";

        var marker = ore.GetComponent<PlacedOreMarker>();
        if (marker == null) marker = ore.AddComponent<PlacedOreMarker>();
        marker.oreType = type;

        ore.tag = type.ToString();
    }

    void SpawnSpawner(Vector3 pos)
    {
        GameObject obj;

        if (slimeSpawnerPrefab != null)
            obj = Instantiate(slimeSpawnerPrefab, pos, Quaternion.identity, spawnerParent);
        else
        {
            obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.transform.SetParent(spawnerParent);
            obj.transform.position = pos;
            obj.transform.localScale = Vector3.one * 0.85f;
            OreVisualUtil.ApplySpawnerMaterial(obj);
        }

        obj.name = "SlimeSpawner";

        if (obj.GetComponent<SlimeSpawner>() == null)
            obj.AddComponent<SlimeSpawner>();
    }

    void SpawnTotem(TotemType type, Vector3 pos)
    {
        GameObject prefab = (totemPrefabs != null) ? totemPrefabs.GetPrefab(type) : null;
        GameObject obj;

        if (prefab != null)
            obj = Instantiate(prefab, pos, Quaternion.identity, totemParent);
        else
        {
            obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            obj.transform.SetParent(totemParent);
            obj.transform.position = pos;
            obj.transform.localScale = new Vector3(0.6f, 1.2f, 0.6f);
        }

        obj.name = "TotemTower";

        var marker = obj.GetComponent<PlacedTotemMarker>();
        if (marker == null) marker = obj.AddComponent<PlacedTotemMarker>();
        marker.totemType = type;

        var tower = obj.GetComponent<TotemTower>();
        if (tower == null) tower = obj.AddComponent<TotemTower>();

        var data = TotemDatabase.Instance != null ? TotemDatabase.Instance.Get(type) : null;
        tower.ApplyData(data);
    }

    // =================================================
    // Utils
    // =================================================
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

    int GetCurrentSpawnerCount()
    {
        if (spawnerParent != null) return spawnerParent.childCount;
        return FindObjectsOfType<SlimeSpawner>().Length;
    }

    bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        if (EventSystem.current.IsPointerOverGameObject())
            return true;

        if (Input.touchCount > 0)
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);

        return false;
    }

    void EnsureParent(ref Transform parent, string name)
    {
        if (parent != null) return;

        var go = GameObject.Find(name);
        if (go == null) go = new GameObject(name);
        parent = go.transform;
    }
}
