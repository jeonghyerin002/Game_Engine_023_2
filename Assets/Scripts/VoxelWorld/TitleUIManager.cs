using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using System;

public class TitleUIManager : MonoBehaviour
{
    public Button startButton;
    public Button resetStartButton;
    public Button exitButton;

    public string gameSceneName = "TestSceneDev";
    public int startCoin = 5000;

    void Start()
    {
        startButton.onClick.AddListener(StartButton);
        resetStartButton.onClick.AddListener(ResetAndStartButton);
        exitButton.onClick.AddListener(ExitButton);
    }

    public void StartButton()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void ResetAndStartButton()
    {
        ResetAllSavesWithStartCoin(startCoin);
        SceneManager.LoadScene(gameSceneName);
    }

    public void ExitButton()
    {
        Application.Quit();
    }

   
    void ResetAllSavesWithStartCoin(int coin)
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        string root = Application.persistentDataPath;

        // 1) 青己 府胶飘 昏力
        DeleteFile(Path.Combine(root, "planet_save.json"));

        // 2) 老老 技捞宏(slime_save.json) 积己 (5000 内牢)
        WriteSlimeSave(Path.Combine(root, "slime_save.json"), coin);

        // 3) 青己喊 技捞宏 傈何 昏力 饶, 霉 青己侩 扁夯 技捞宏 积己
        try
        {
            var files = Directory.GetFiles(root, "planet_*.json");
            foreach (var f in files)
                DeleteFile(f);
        }
        catch { }

        Debug.Log($"[Title] Reset complete. StartCoin = {coin}");

        PlayerPrefs.SetInt("ResetStart", 1);
        PlayerPrefs.SetInt("ResetStartCoin", coin);
        PlayerPrefs.Save();
    }

    void DeleteFile(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }

    // SlimeDailyResetSaveManager侩 技捞宏 备炼
    void WriteSlimeSave(string path, int coin)
    {
        var data = new SlimeSaveData
        {
            coin = coin,
            soil = 0,
            copper = 0,
            silver = 0,
            gold = 0,
            metal = 0,
            mithril = 0,
            nextResetUtcTicks = DateTime.UtcNow.Ticks
        };

        File.WriteAllText(path, JsonUtility.ToJson(data, true));
    }

    [Serializable]
    class SlimeSaveData
    {
        public long coin, soil, copper, silver, gold, metal, mithril;
        public long nextResetUtcTicks;
    }
}
