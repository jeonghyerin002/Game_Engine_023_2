using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SlimePlanetSaveManager : MonoBehaviour
{
    [Header("Refs")]
    public VoxelWorld voxelWorld;

    [Header("Parents (없으면 자동 생성)")]
    public Transform oreParent;        // OreObjects
    public Transform spawnerParent;    // SlimeSpawners
    public Transform totemParent;      // TotemTowers

    [Header("Prefabs (복원용, 없으면 Primitive로 생성)")]
    public GameObject copperPrefab;
    public GameObject silverPrefab;
    public GameObject goldPrefab;
    public GameObject metalPrefab;
    public GameObject mithrilPrefab;
    public GameObject slimeSpawnerPrefab;

    [Header("Totem Prefab (없으면 Cylinder)")]
    public GameObject totemPrefab;

    [Header("행성 이동 시 슬라임 처리")]
    public bool clearSlimesOnPlanetLoad = true;
    public bool burstSpawnOnPlanetLoad = true;
    public int burstSpawnCountPerSpawner = 5;

    [Header("Debug")]
    public bool logDebug = true;

    // =========================
    // Save Data
    // =========================

    [Serializable]
    class SaveData
    {
        public List<PlacedOre> ores = new();
        public List<PlacedSpawner> spawners = new();
        public List<PlacedTotem> totems = new();
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

    [Serializable]
    class PlacedTotem
    {
        public TotemType type;
        public Vector3 pos;
        public Quaternion rot;
    }

    // =========================
    // Unity Events
    // =========================

    void Awake()
    {
        if (voxelWorld == null)
            voxelWorld = FindObjectOfType<VoxelWorld>();

        EnsureParents();
    }

    void OnApplicationPause(bool pause)
    {
        if (pause) SaveNow();
    }

    void OnApplicationQuit()
    {
        SaveNow();
    }

    // =========================
    // Public API
    // =========================

    /// <summary>
    /// PlanetUIBinder에서 행성 이동 시 호출
    /// </summary>
    public void LoadCurrentPlanet()
    {
        EnsureParents();

        // 1) 월드 재생성 (Seed 기준)
        if (voxelWorld != null)
            voxelWorld.Regenerate();

        // 2) 데이터 로드
        SaveData data = LoadPlanetData();

        // 3) 기존 설치물 제거
        ClearChildren(oreParent);
        ClearChildren(spawnerParent);
        ClearChildren(totemParent);

        // 4) 설치물 복원
        for (int i = 0; i < data.ores.Count; i++)
        {
            var o = data.ores[i];
            SpawnOre(o.oreType, GetOrePrefab(o.oreType), o.pos, o.rot, oreParent);
        }

        for (int i = 0; i < data.spawners.Count; i++)
        {
            var s = data.spawners[i];
            SpawnSpawner(s.pos, s.rot, spawnerParent);
        }

        for (int i = 0; i < data.totems.Count; i++)
        {
            var t = data.totems[i];
            SpawnTotem(t.type, t.pos, t.rot, totemParent);
        }

        // 5) 슬라임 리셋 & 재스폰
        AfterPlanetLoaded_ResetSlimes();

        if (logDebug)
            Debug.Log($"[PlanetLoad] ores={data.ores.Count}, spawners={data.spawners.Count}, totems={data.totems.Count}");
    }

    public void SaveNow()
    {
        EnsureParents();

        var data = new SaveData();

        // 광석 저장
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

        // 토탬 저장
        if (totemParent != null)
        {
            for (int i = 0; i < totemParent.childCount; i++)
            {
                var tr = totemParent.GetChild(i);
                var marker = tr.GetComponent<PlacedTotemMarker>();
                if (marker == null) continue;

                data.totems.Add(new PlacedTotem
                {
                    type = marker.totemType,
                    pos = tr.position,
                    rot = tr.rotation
                });
            }
        }

        WritePlanetData(data);

        if (logDebug)
            Debug.Log($"[PlanetSave] ores={data.ores.Count}, spawners={data.spawners.Count}, totems={data.totems.Count}");
    }

    // =========================
    // Planet File
    // =========================

    string GetPlanetFilePath()
    {
        var pm = PlanetManager.Instance;
        var p = pm != null ? pm.CurrentPlanet : null;
        string id = (p != null) ? p.id : "no_planet";
        return Path.Combine(Application.persistentDataPath, $"planet_{id}.json");
    }

    SaveData LoadPlanetData()
    {
        var path = GetPlanetFilePath();

        if (File.Exists(path))
        {
            try
            {
                return JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
            }
            catch { }
        }

        var empty = new SaveData();
        WritePlanetData(empty);
        return empty;
    }

    void WritePlanetData(SaveData data)
    {
        var path = GetPlanetFilePath();
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, JsonUtility.ToJson(data, true));
    }

    // =========================
    // Slime Reset & Respawn
    // =========================

    void AfterPlanetLoaded_ResetSlimes()
    {
        if (clearSlimesOnPlanetLoad)
            ClearAllSlimes();

        if (burstSpawnOnPlanetLoad)
            BurstSpawnFromAllSpawners(burstSpawnCountPerSpawner);
    }

    void ClearAllSlimes()
    {
        var slimes = GameObject.FindGameObjectsWithTag("Slime");
        for (int i = 0; i < slimes.Length; i++)
            Destroy(slimes[i]);
    }

    void BurstSpawnFromAllSpawners(int countPerSpawner)
    {
        var spawners = FindObjectsOfType<SlimeSpawner>();
        for (int i = 0; i < spawners.Length; i++)
        {
            spawners[i].ResetTimer();
            spawners[i].SpawnBurst(countPerSpawner);
        }
    }

    // =========================
    // Spawn Helpers
    // =========================

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

            // 프리미어 머티리얼(있으면)
            OreVisualUtil.ApplyOreMaterial(ore, type);
        }

        ore.name = type + "_Ore";
        ore.tag = type.ToString();

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

            // 핑크 프리미어 머티리얼(있으면)
            OreVisualUtil.ApplySpawnerMaterial(spawnerObj);
        }

        spawnerObj.name = "SlimeSpawner";

        if (spawnerObj.GetComponent<SlimeSpawner>() == null)
            spawnerObj.AddComponent<SlimeSpawner>();

        return spawnerObj;
    }

    GameObject SpawnTotem(TotemType type, Vector3 pos, Quaternion rot, Transform parent)
    {
        GameObject obj;

        if (totemPrefab != null)
        {
            obj = Instantiate(totemPrefab, pos, rot, parent);
        }
        else
        {
            obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            obj.transform.SetParent(parent);
            obj.transform.SetPositionAndRotation(pos, rot);
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

        return obj;
    }

    // =========================
    // Utils
    // =========================

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

        if (totemParent == null)
        {
            var go = GameObject.Find("TotemTowers");
            if (go == null) go = new GameObject("TotemTowers");
            totemParent = go.transform;
        }
    }

    void ClearChildren(Transform parent)
    {
        if (parent == null) return;
        for (int i = parent.childCount - 1; i >= 0; i--)
            Destroy(parent.GetChild(i).gameObject);
    }
}
