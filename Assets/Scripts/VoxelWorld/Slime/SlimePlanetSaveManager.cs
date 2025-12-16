using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SlimePlanetSaveManager : MonoBehaviour
{
    [Header("Refs")]
    public SlimeGameManager game;
    public VoxelWorld voxelWorld;

    [Header("Parents (없으면 자동 찾기/생성)")]
    public Transform oreParent;      // "OreObjects"
    public Transform spawnerParent;  // "SlimeSpawners"

    [Header("Prefabs (복원용, 없으면 큐브로 복원)")]
    public GameObject copperPrefab;
    public GameObject silverPrefab;
    public GameObject goldPrefab;
    public GameObject metalPrefab;
    public GameObject mithrilPrefab;
    public GameObject slimeSpawnerPrefab;

    [Header("Debug")]
    public bool logDebug = true;

    [Serializable]
    class SaveData
    {
        // 자원
        public long coin, soil, copper, silver, gold, metal, mithril;

        // 설치물
        public List<PlacedOre> ores = new();
        public List<PlacedSpawner> spawners = new();
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

    string GetPlanetFilePath()
    {
        var pm = PlanetManager.Instance;
        var p = pm != null ? pm.CurrentPlanet : null;
        string id = (p != null) ? p.id : "no_planet";
        return Path.Combine(Application.persistentDataPath, $"planet_{id}.json");
    }

    void Awake()
    {
        if (game == null) game = SlimeGameManager.Instance;
        if (voxelWorld == null) voxelWorld = FindObjectOfType<VoxelWorld>();

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

    // 버튼으로 행성 바꾼 직후 호출해줘야 하는 함수
    public void LoadCurrentPlanet()
    {
        EnsureParents();

        // 1) 월드 먼저 재생성 (현재 행성 seed 기준)
        if (voxelWorld != null) voxelWorld.Regenerate();

        // 2) 데이터 로드
        var path = GetPlanetFilePath();
        SaveData data = null;

        if (File.Exists(path))
        {
            try { data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path)); }
            catch { data = null; }
        }

        if (data == null)
        {
            data = new SaveData();
            // 처음 행성은 빈 데이터로 시작
            Write(data);
        }

        // 3) 자원 적용
        if (game != null)
        {
            game.SetResource(ResourceType.Coin, data.coin);
            game.SetResource(ResourceType.Soil, data.soil);
            game.SetResource(ResourceType.Copper, data.copper);
            game.SetResource(ResourceType.Silver, data.silver);
            game.SetResource(ResourceType.Gold, data.gold);
            game.SetResource(ResourceType.Metal, data.metal);
            game.SetResource(ResourceType.Mithril, data.mithril);
        }

        // 4) 설치물 복원 (기존 삭제 후)
        ClearChildren(oreParent);
        ClearChildren(spawnerParent);

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

        if (logDebug) Debug.Log($"[PlanetLoad] path={path} ores={data.ores.Count} spawners={data.spawners.Count}");
    }

    public void SaveNow()
    {
        EnsureParents();

        var data = new SaveData();

        // 자원 저장
        if (game != null)
        {
            data.coin = game.GetResource(ResourceType.Coin);
            data.soil = game.GetResource(ResourceType.Soil);
            data.copper = game.GetResource(ResourceType.Copper);
            data.silver = game.GetResource(ResourceType.Silver);
            data.gold = game.GetResource(ResourceType.Gold);
            data.metal = game.GetResource(ResourceType.Metal);
            data.mithril = game.GetResource(ResourceType.Mithril);
        }

        // 광맥 저장 (PlacedOreMarker 기반)
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

        Write(data);

        if (logDebug) Debug.Log($"[PlanetSave] path={GetPlanetFilePath()} ores={data.ores.Count} spawners={data.spawners.Count}");
    }

    void Write(SaveData data)
    {
        var path = GetPlanetFilePath();
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, JsonUtility.ToJson(data, true));
    }

    // -------------------------
    // Parents / Helpers
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

    void ClearChildren(Transform parent)
    {
        if (parent == null) return;
        for (int i = parent.childCount - 1; i >= 0; i--)
            Destroy(parent.GetChild(i).gameObject);
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
}
