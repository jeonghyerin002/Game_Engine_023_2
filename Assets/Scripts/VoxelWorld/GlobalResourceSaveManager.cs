using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GlobalResourceSaveManager : MonoBehaviour
{
    [Header("Refs")]
    public SlimeGameManager game;

    [Header("저장 파일명(전역)")]
    public string saveFileName = "global_resources.json";

    [Header("Debug")]
    public bool logDebug = false;

    string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);

    [Serializable]
    class SaveData
    {
        public List<ResourceCountEntry> resources = new();
    }

    void Awake()
    {
        if (game == null) game = SlimeGameManager.Instance;
        LoadNow();
    }

    void OnApplicationPause(bool pause)
    {
        if (pause) SaveNow();
    }

    void OnApplicationQuit()
    {
        SaveNow();
    }

    public void SaveNow()
    {
        if (game == null) return;

        var data = new SaveData();

        // SlimeGameManager가 inspector용 리스트(resourceCounts)를 유지하므로
        // 그 리스트를 그대로 저장하면 됨
        if (game.resourceCounts != null)
        {
            for (int i = 0; i < game.resourceCounts.Count; i++)
            {
                data.resources.Add(new ResourceCountEntry
                {
                    type = game.resourceCounts[i].type,
                    count = game.resourceCounts[i].count
                });
            }
        }

        Directory.CreateDirectory(Path.GetDirectoryName(SavePath));
        File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));

        if (logDebug) Debug.Log($"[GlobalResourceSave] path={SavePath} entries={data.resources.Count}");
    }

    public void LoadNow()
    {
        if (game == null) game = SlimeGameManager.Instance;
        if (game == null) return;

        SaveData data = null;

        if (File.Exists(SavePath))
        {
            try { data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath)); }
            catch { data = null; }
        }

        if (data == null || data.resources == null)
        {
            // 파일 없으면 현재값을 초기값으로 저장
            SaveNow();
            return;
        }

        // 전역 자원 적용
        for (int i = 0; i < data.resources.Count; i++)
        {
            var e = data.resources[i];
            game.SetResource(e.type, e.count);
        }

        if (logDebug) Debug.Log($"[GlobalResourceLoad] path={SavePath} entries={data.resources.Count}");
    }
}
