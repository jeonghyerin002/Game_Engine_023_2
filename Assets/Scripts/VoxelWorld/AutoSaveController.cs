using UnityEngine;

public class AutoSaveController : MonoBehaviour
{
    public SlimePlanetSaveManager planetSave;
    public float autoSaveInterval = 5f;
    float t;

    void Awake()
    {
        if (planetSave == null) planetSave = FindObjectOfType<SlimePlanetSaveManager>();
    }

    void Update()
    {
        if (planetSave == null) return;

        t += Time.deltaTime;
        if (t >= autoSaveInterval)
        {
            t = 0f;
            planetSave.SaveNow();
        }
    }

    void OnApplicationPause(bool pause)
    {
        if (pause && planetSave != null) planetSave.SaveNow();
    }

    void OnApplicationQuit()
    {
        if (planetSave != null) planetSave.SaveNow();
    }
}