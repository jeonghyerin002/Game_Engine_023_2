using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SlimeDailyResetSaveManager : MonoBehaviour
{
    [Header("연결")]
    public SlimeGameManager game;

    [Header("부모(없으면 자동 찾기/생성)")]
    public Transform oreParent;          // OreObjects
    public Transform spawnerParent;      // SlimeSpawners

    [Header("프리팹 (복원용, 없으면 큐브로 복원)")]
    public GameObject copperPrefab;
    public GameObject silverPrefab;
    public GameObject goldPrefab;
    public GameObject metalPrefab;
    public GameObject mithrilPrefab;
    public GameObject slimeSpawnerPrefab;

    [Header("리셋 주기(초) - 테스트용 240초")]
    public bool enableReset = true;
    public int resetIntervalSeconds = 240;

    [Header("리셋 시 즉시 재스폰(선택)")]
    public bool burstSpawnAfterReset = true;
    public int burstSpawnCountPerSpawner = 3;

    [Header("저장 파일명")]
    public string saveFileName = "slime_save.json";

    string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);
    SaveData data;

    public OreMaterialBank oreMats;

    void Awake()
    {
        if (game == null) game = SlimeGameManager.Instance;

        EnsureParents();

        LoadOrCreate();
        ApplyLoadedResources();
        RestorePlacedObjects(); // ✅ 설치물 복원

        if (enableReset)
            CheckAndDoResetIfNeeded();
    }

    void Start()
    {
        if (oreMats == null) oreMats = FindObjectOfType<OreMaterialBank>();
    }

    void Update()
    {
        if (enableReset && data != null)
            CheckAndDoResetIfNeeded();
    }

    void OnApplicationPause(bool pause)
    {
        if (pause) SaveNow();
    }

    void OnApplicationQuit()
    {
        SaveNow();
    }

    // -------------------------
    // Parents
    // -------------------------
    void EnsureParents()
    {
        if (oreParent == null)
        {
            var go = GameObject.Find("OreObjects");
            if (go == null) go = new GameObject("OreObjects");
            oreParent = go.transform;
        }

        if (spawnerParent == null)
        {
            var go = GameObject.Find("SlimeSpawners");
            if (go == null) go = new GameObject("SlimeSpawners");
            spawnerParent = go.transform;
        }
    }

    // -------------------------
    // Reset
    // -------------------------
    void CheckAndDoResetIfNeeded()
    {
        long now = DateTime.UtcNow.Ticks;
        if (now < data.nextResetUtcTicks) return;

        DoReset();

        data.nextResetUtcTicks = DateTime.UtcNow.AddSeconds(resetIntervalSeconds).Ticks;
        SaveNow();
    }

    void DoReset()
    {
        // 슬라임 전부 제거
        var slimes = GameObject.FindGameObjectsWithTag("Slime");
        for (int i = 0; i < slimes.Length; i++)
            Destroy(slimes[i]);

        // 스포너에서 재스폰
        var spawners = FindObjectsOfType<SlimeSpawner>();
        for (int i = 0; i < spawners.Length; i++)
        {
            spawners[i].ResetTimer();
            if (burstSpawnAfterReset)
                spawners[i].SpawnBurst(burstSpawnCountPerSpawner);
        }
    }

    // -------------------------
    // Save / Load
    // -------------------------
    public void SaveNow()
    {
        EnsureParents();

        if (data == null) data = new SaveData();

        // 자원 저장
        data.coin = game != null ? game.GetResource(ResourceType.Coin) : 0;
        data.soil = game != null ? game.GetResource(ResourceType.Soil) : 0;
        data.copper = game != null ? game.GetResource(ResourceType.Copper) : 0;
        data.silver = game != null ? game.GetResource(ResourceType.Silver) : 0;
        data.gold = game != null ? game.GetResource(ResourceType.Gold) : 0;
        data.metal = game != null ? game.GetResource(ResourceType.Metal) : 0;
        data.mithril = game != null ? game.GetResource(ResourceType.Mithril) : 0;

        // 광맥 저장 (Marker 기반)
        data.ores = new List<PlacedOre>();
        if (oreParent != null)
        {
            for (int i = 0; i < oreParent.childCount; i++)
            {
                var tr = oreParent.GetChild(i);
                var marker = tr.GetComponent<PlacedOreMarker>();
                if (marker == null) continue;

                data.ores.Add(new PlacedOre
                {
                    oreType = marker.oreType,
                    pos = tr.position,
                    rot = tr.rotation
                });
            }
        }

        // 스포너 저장
        data.spawners = new List<PlacedSpawner>();
        if (spawnerParent != null)
        {
            for (int i = 0; i < spawnerParent.childCount; i++)
            {
                var tr = spawnerParent.GetChild(i);
                data.spawners.Add(new PlacedSpawner
                {
                    pos = tr.position,
                    rot = tr.rotation
                });
            }
        }

        // 저장
        Directory.CreateDirectory(Path.GetDirectoryName(SavePath));
        File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));

        Debug.Log($"[SAVE] ores={data.ores.Count}, spawners={data.spawners.Count} path={SavePath}");
    }

    void LoadOrCreate()
    {
        if (File.Exists(SavePath))
        {
            try
            {
                data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
            }
            catch
            {
                data = null;
            }
        }

        if (data == null)
        {
            data = new SaveData();
            data.nextResetUtcTicks = DateTime.UtcNow.AddSeconds(resetIntervalSeconds).Ticks;
            SaveNow();
        }

        // 구버전/비정상 세이브 보정
        if (data.nextResetUtcTicks <= 0)
        {
            data.nextResetUtcTicks = DateTime.UtcNow.AddSeconds(resetIntervalSeconds).Ticks;
            SaveNow();
        }
    }

    void ApplyLoadedResources()
    {
        if (game == null || data == null) return;

        game.SetResource(ResourceType.Coin, data.coin);
        game.SetResource(ResourceType.Soil, data.soil);
        game.SetResource(ResourceType.Copper, data.copper);
        game.SetResource(ResourceType.Silver, data.silver);
        game.SetResource(ResourceType.Gold, data.gold);
        game.SetResource(ResourceType.Metal, data.metal);
        game.SetResource(ResourceType.Mithril, data.mithril);
    }

    // -------------------------
    // Restore
    // -------------------------
    void RestorePlacedObjects()
    {
        if (data == null) return;

        EnsureParents();

        ClearChildren(oreParent);
        ClearChildren(spawnerParent);

        // 광맥 복원
        if (data.ores != null)
        {
            for (int i = 0; i < data.ores.Count; i++)
            {
                var o = data.ores[i];
                var prefab = GetOrePrefab(o.oreType);
                SpawnOre(o.oreType, prefab, o.pos, o.rot, oreParent);
            }
        }

        // 스포너 복원
        if (data.spawners != null)
        {
            for (int i = 0; i < data.spawners.Count; i++)
            {
                var s = data.spawners[i];
                SpawnSpawner(s.pos, s.rot, spawnerParent);
            }
        }

        Debug.Log($"[RESTORE] ores={(data.ores?.Count ?? 0)}, spawners={(data.spawners?.Count ?? 0)}");
    }

    void ClearChildren(Transform parent)
    {
        if (parent == null) return;
        for (int i = parent.childCount - 1; i >= 0; i--)
            Destroy(parent.GetChild(i).gameObject);
    }

    // -------------------------
    // Spawn helpers
    // -------------------------
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

    GameObject SpawnOre(ResourceType type, GameObject prefab, Vector3 pos, Quaternion rot, Transform parent)
    {
        GameObject ore;

        if (prefab != null)
        {
            ore = Instantiate(prefab, pos, rot, parent);
        }
        else
        {
            ore = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ore.transform.SetParent(parent);
            ore.transform.SetPositionAndRotation(pos, rot);
            ore.transform.localScale = Vector3.one * 0.9f;

            var r = ore.GetComponent<MeshRenderer>();
            if (r != null && oreMats != null)
            {
                var mat = oreMats.Get(type);
                if (mat != null) r.sharedMaterial = mat; // sharedMaterial로 메모리 절약
            }
        }

        ore.name = type + "_Ore";

        var marker = ore.GetComponent<PlacedOreMarker>();
        if (marker == null) marker = ore.AddComponent<PlacedOreMarker>();
        marker.oreType = type;

        return ore;
    }

    GameObject SpawnSpawner(Vector3 pos, Quaternion rot, Transform parent)
    {
        GameObject spawnerObj;

        if (slimeSpawnerPrefab != null)
        {
            spawnerObj = Instantiate(slimeSpawnerPrefab, pos, rot, parent);
        }
        else
        {
            spawnerObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spawnerObj.transform.SetParent(parent);
            spawnerObj.transform.SetPositionAndRotation(pos, rot);
            spawnerObj.transform.localScale = Vector3.one * 0.85f;
        }

        spawnerObj.name = "SlimeSpawner";

        var spawnerComp = spawnerObj.GetComponent<SlimeSpawner>();
        if (spawnerComp == null) spawnerComp = spawnerObj.AddComponent<SlimeSpawner>();

        return spawnerObj;
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

    // -------------------------
    // Save Data
    // -------------------------
    [Serializable]
    class SaveData
    {
        public long coin, soil, copper, silver, gold, metal, mithril;
        public long nextResetUtcTicks;

        public List<PlacedOre> ores;
        public List<PlacedSpawner> spawners;
    }

    [Serializable]
    class PlacedOre
    {
        public ResourceType oreType;
        public Vector3 pos;
        public Quaternion rot;
    }

    [Serializable]
    class PlacedSpawner
    {
        public Vector3 pos;
        public Quaternion rot;
    }
}
