using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum SlimeBuildMode
{
    None,
    Copper,
    Silver,
    Gold,
    Metal,
    Mithril,
    SlimeSpawner
}

public class SlimeBuildUI : MonoBehaviour
{
    [Header("현재 선택")]
    public SlimeBuildMode currentMode = SlimeBuildMode.None;

    [Header("UI Buttons")]
    public Button noneButton;

    public Button copperButton;
    public Button silverButton;
    public Button goldButton;
    public Button metalButton;
    public Button mithrilButton;

    public Button spawnerButton;

    [Header("선택 표시(옵션)")]
    public Image noneHighlight;
    public Image copperHighlight;
    public Image silverHighlight;
    public Image goldHighlight;
    public Image metalHighlight;
    public Image mithrilHighlight;
    public Image spawnerHighlight;

    [Header("가격 표시 TMP_Text (버튼 위/아래 텍스트)")]
    public TMP_Text copperCostText;
    public TMP_Text silverCostText;
    public TMP_Text goldCostText;
    public TMP_Text metalCostText;
    public TMP_Text mithrilCostText;
    public TMP_Text spawnerCostText;

    [Header("비용")]
    public long copperCostCoin = 50;
    public long silverCostCoin = 150;
    public long goldCostCoin = 400;
    public long metalCostCoin = 800;
    public long mithrilCostCoin = 2000;
    public long spawnerCostCoin = 3000;

    [Header("선택 제한")]
    public bool blockSelectIfNoCoin = true;

    SlimeGameManager gm;

    void Awake()
    {
        gm = SlimeGameManager.Instance;
    }

    void Start()
    {
        if (noneButton != null) noneButton.onClick.AddListener(SetMode_None);

        if (copperButton != null) copperButton.onClick.AddListener(SetMode_Copper);
        if (silverButton != null) silverButton.onClick.AddListener(SetMode_Silver);
        if (goldButton != null) goldButton.onClick.AddListener(SetMode_Gold);
        if (metalButton != null) metalButton.onClick.AddListener(SetMode_Metal);
        if (mithrilButton != null) mithrilButton.onClick.AddListener(SetMode_Mithril);

        if (spawnerButton != null) spawnerButton.onClick.AddListener(SetMode_Spawner);

        RefreshCostLabels();
        RefreshHighlight();
        RefreshInteractable();
    }

    void Update()
    {
        RefreshInteractable();
    }

    // -------------------------
    // 모드 변경
    // -------------------------
    public void SetMode_None()
    {
        currentMode = SlimeBuildMode.None;
        RefreshHighlight();
    }

    public void SetMode_Copper() => TrySetMode(SlimeBuildMode.Copper, copperCostCoin);
    public void SetMode_Silver() => TrySetMode(SlimeBuildMode.Silver, silverCostCoin);
    public void SetMode_Gold() => TrySetMode(SlimeBuildMode.Gold, goldCostCoin);
    public void SetMode_Metal() => TrySetMode(SlimeBuildMode.Metal, metalCostCoin);
    public void SetMode_Mithril() => TrySetMode(SlimeBuildMode.Mithril, mithrilCostCoin);
    public void SetMode_Spawner() => TrySetMode(SlimeBuildMode.SlimeSpawner, spawnerCostCoin);

    void TrySetMode(SlimeBuildMode mode, long costCoin)
    {
        if (blockSelectIfNoCoin && gm != null)
        {
            if (!gm.CanSpendResource(ResourceType.Coin, costCoin))
                return;
        }

        currentMode = mode;
        RefreshHighlight();
    }

    // -------------------------
    // UI 갱신
    // -------------------------
    public void RefreshCostLabels()
    {
        if (copperCostText != null) copperCostText.text = FormatIdle(copperCostCoin);
        if (silverCostText != null) silverCostText.text = FormatIdle(silverCostCoin);
        if (goldCostText != null) goldCostText.text = FormatIdle(goldCostCoin);
        if (metalCostText != null) metalCostText.text = FormatIdle(metalCostCoin);
        if (mithrilCostText != null) mithrilCostText.text = FormatIdle(mithrilCostCoin);
        if (spawnerCostText != null) spawnerCostText.text = FormatIdle(spawnerCostCoin);
    }

    void RefreshInteractable()
    {
        if (gm == null) gm = SlimeGameManager.Instance;
        if (gm == null) return;

        if (!blockSelectIfNoCoin) return;

        long coin = gm.GetResource(ResourceType.Coin);

        if (copperButton != null) copperButton.interactable = coin >= copperCostCoin;
        if (silverButton != null) silverButton.interactable = coin >= silverCostCoin;
        if (goldButton != null) goldButton.interactable = coin >= goldCostCoin;
        if (metalButton != null) metalButton.interactable = coin >= metalCostCoin;
        if (mithrilButton != null) mithrilButton.interactable = coin >= mithrilCostCoin;
        if (spawnerButton != null) spawnerButton.interactable = coin >= spawnerCostCoin;
    }

    void RefreshHighlight()
    {
        SetHighlight(noneHighlight, currentMode == SlimeBuildMode.None);
        SetHighlight(copperHighlight, currentMode == SlimeBuildMode.Copper);
        SetHighlight(silverHighlight, currentMode == SlimeBuildMode.Silver);
        SetHighlight(goldHighlight, currentMode == SlimeBuildMode.Gold);
        SetHighlight(metalHighlight, currentMode == SlimeBuildMode.Metal);
        SetHighlight(mithrilHighlight, currentMode == SlimeBuildMode.Mithril);
        SetHighlight(spawnerHighlight, currentMode == SlimeBuildMode.SlimeSpawner);
    }

    void SetHighlight(Image img, bool on)
    {
        if (img != null) img.enabled = on;
    }

    // -------------------------
    // 표시 유틸 (방치형 표기)
    // -------------------------
    static string FormatIdle(long value)
    {
        if (value < 0) return "-" + FormatIdle(-value);
        if (value < 1000) return value.ToString();

        string[] units = { "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No" };
        double v = value;
        int unitIndex = -1;

        while (v >= 1000 && unitIndex < units.Length - 1)
        {
            v /= 1000.0;
            unitIndex++;
        }

        double rounded = Math.Floor(v * 10) / 10.0; // 소수 1자리 버림
        return rounded.ToString("0.#") + units[unitIndex];
    }

    public bool TryGetBuildRequest(out BuildRequest req)
    {
        req = default;

        switch (currentMode)
        {
            case SlimeBuildMode.Copper:
                req = BuildRequest.Ore(ResourceType.Copper, copperCostCoin);
                return true;

            case SlimeBuildMode.Silver:
                req = BuildRequest.Ore(ResourceType.Silver, silverCostCoin);
                return true;

            case SlimeBuildMode.Gold:
                req = BuildRequest.Ore(ResourceType.Gold, goldCostCoin);
                return true;

            case SlimeBuildMode.Metal:
                req = BuildRequest.Ore(ResourceType.Metal, metalCostCoin);
                return true;

            case SlimeBuildMode.Mithril:
                req = BuildRequest.Ore(ResourceType.Mithril, mithrilCostCoin);
                return true;

            case SlimeBuildMode.SlimeSpawner:
                req = BuildRequest.Spawner(spawnerCostCoin);
                return true;

            default:
                return false;
        }
    }

    [System.Serializable]
    public struct BuildRequest
    {
        public bool isSpawner;
        public ResourceType oreType; // isSpawner=false 일 때만 의미
        public long costCoin;

        public static BuildRequest Ore(ResourceType type, long cost)
        {
            return new BuildRequest
            {
                isSpawner = false,
                oreType = type,
                costCoin = cost
            };
        }

        public static BuildRequest Spawner(long cost)
        {
            return new BuildRequest
            {
                isSpawner = true,
                oreType = ResourceType.Soil,
                costCoin = cost
            };
        }
    }
}
