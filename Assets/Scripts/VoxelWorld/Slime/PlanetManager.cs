using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlanetManager : MonoBehaviour
{
    public static PlanetManager Instance;

    [Serializable]
    public class PlanetInfo
    {
        public string id;
        public string name;
        public int seed;
        public long createdUtcTicks;
    }

    [Serializable]
    class SaveData
    {
        public int currentIndex;
        public List<PlanetInfo> planets;
    }

    [Header("저장 파일명")]
    public string saveFileName = "planet_save.json";

    [Header("디버그")]
    public bool logDebug = true;

    // UIBinder에서 currentIndex / planets 접근하니까 public으로 유지
    [SerializeField] public List<PlanetInfo> planets = new List<PlanetInfo>();
    [SerializeField] public int currentIndex = 0;

    string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);

    public PlanetInfo CurrentPlanet
    {
        get
        {
            if (planets == null || planets.Count == 0) return null;
            currentIndex = Mathf.Clamp(currentIndex, 0, planets.Count - 1);
            return planets[currentIndex];
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadOrCreate();
    }

    // -----------------------------
    // Public API (UIBinder가 호출하는 이름 그대로)
    // -----------------------------

    // UIBinder가 CreatePlanet() 호출할 수도 있으니 유지
    public void CreatePlanet(string planetName = null)
    {
        if (planets == null) planets = new List<PlanetInfo>();

        var p = new PlanetInfo
        {
            id = Guid.NewGuid().ToString("N"),
            seed = UnityEngine.Random.Range(1, int.MaxValue),
            createdUtcTicks = DateTime.UtcNow.Ticks,
            name = string.IsNullOrWhiteSpace(planetName) ? $"Planet {planets.Count + 1}" : planetName
        };

        planets.Add(p);
        currentIndex = planets.Count - 1;

        SaveNow();

        if (logDebug)
            Debug.Log($"[PlanetManager] CreatePlanet idx={currentIndex} name={p.name} seed={p.seed}");
    }

    // UIBinder가 찾는 함수명: PrevPlanet / NextPlanet
    public void PrevPlanet()
    {
        if (planets == null || planets.Count == 0) return;

        currentIndex = (currentIndex - 1 + planets.Count) % planets.Count;
        SaveNow();

        if (logDebug)
            Debug.Log($"[PlanetManager] PrevPlanet idx={currentIndex} name={CurrentPlanet?.name} seed={CurrentPlanet?.seed}");
    }

    public void NextPlanet()
    {
        if (planets == null || planets.Count == 0) return;

        currentIndex = (currentIndex + 1) % planets.Count;
        SaveNow();

        if (logDebug)
            Debug.Log($"[PlanetManager] NextPlanet idx={currentIndex} name={CurrentPlanet?.name} seed={CurrentPlanet?.seed}");
    }

    // UIBinder가 찾는 함수명: DeletePlanet
    public void DeletePlanet()
    {
        DeleteCurrentPlanet();
    }

    // 기존 내부 구현 (삭제 로직)
    public void DeleteCurrentPlanet()
    {
        if (planets == null || planets.Count == 0) return;

        var deleted = CurrentPlanet;
        planets.RemoveAt(currentIndex);

        if (planets.Count == 0)
        {
            // 하나도 없으면 자동으로 1개 생성
            CreatePlanet("Planet 1");
            return;
        }

        currentIndex = Mathf.Clamp(currentIndex, 0, planets.Count - 1);
        SaveNow();

        if (logDebug)
            Debug.Log($"[PlanetManager] DeletePlanet deleted={deleted?.name} nowIdx={currentIndex} nowName={CurrentPlanet?.name}");
    }

    public void RenameCurrentPlanet(string newName)
    {
        if (CurrentPlanet == null) return;
        if (string.IsNullOrWhiteSpace(newName)) return;

        CurrentPlanet.name = newName.Trim();
        SaveNow();

        if (logDebug)
            Debug.Log($"[PlanetManager] RenamePlanet idx={currentIndex} name={CurrentPlanet.name}");
    }

    // -----------------------------
    // Save / Load
    // -----------------------------

    public void SaveNow()
    {
        var data = new SaveData
        {
            currentIndex = currentIndex,
            planets = planets
        };

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SavePath));
            File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlanetManager] Save failed: {e}");
        }
    }

    void LoadOrCreate()
    {
        SaveData data = null;

        if (File.Exists(SavePath))
        {
            try
            {
                data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[PlanetManager] Load parse failed: {e}");
            }
        }

        if (data == null || data.planets == null || data.planets.Count == 0)
        {
            planets = new List<PlanetInfo>();
            currentIndex = 0;
            CreatePlanet("Planet 1");
            return;
        }

        planets = data.planets;
        currentIndex = Mathf.Clamp(data.currentIndex, 0, planets.Count - 1);

        if (logDebug && CurrentPlanet != null)
            Debug.Log($"[PlanetManager] Loaded planets={planets.Count}, currentIndex={currentIndex}, currentSeed={CurrentPlanet.seed}");
    }
}
