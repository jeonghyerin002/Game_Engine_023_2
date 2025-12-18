using System.IO;
using UnityEngine;

public static class GameDataResetter
{
    public static void ResetAllSavesAndPrefs(bool log = true)
    {
        // 1) PlayerPrefs 전체 초기화
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // 2) 파일 세이브 초기화
        string root = Application.persistentDataPath;

        DeleteFile(Path.Combine(root, "planet_save.json"), log);
        DeleteFile(Path.Combine(root, "slime_save.json"), log);

        // 3) 행성별 세이브 파일(planet_{id}.json) 전부 삭제
        try
        {
            var files = Directory.GetFiles(root, "planet_*.json");
            for (int i = 0; i < files.Length; i++)
                DeleteFile(files[i], log);
        }
        catch (System.Exception e)
        {
            if (log) Debug.LogWarning($"[Reset] planet_*.json scan failed: {e.Message}");
        }

        if (log) Debug.Log($"[Reset] Done. path={root}");
    }

    static void DeleteFile(string path, bool log)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                if (log) Debug.Log($"[Reset] Deleted: {path}");
            }
        }
        catch (System.Exception e)
        {
            if (log) Debug.LogWarning($"[Reset] Delete failed: {path} ({e.Message})");
        }
    }
}
