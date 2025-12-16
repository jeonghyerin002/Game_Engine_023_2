using UnityEngine;

public class SellAllButton : MonoBehaviour
{
    public SlimePlanetSaveManager planetSave;

    void Awake()
    {
        if (planetSave == null) planetSave = FindObjectOfType<SlimePlanetSaveManager>();
    }

    public void SellAll()
    {
        var gm = SlimeGameManager.Instance;
        if (gm == null) return;

        long gained = gm.SellAllResources();
        Debug.Log($"[SELL] All resources sold. +{gained} Coin");

        planetSave?.SaveNow();
    }
}