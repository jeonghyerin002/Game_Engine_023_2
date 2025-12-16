using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("가격 표시 TMP_Text (선택)")]
    public TMP_Text copperCostText;
    public TMP_Text silverCostText;
    public TMP_Text goldCostText;
    public TMP_Text metalCostText;
    public TMP_Text mithrilCostText;
    public TMP_Text spawnerCostText;

    [Header("비용(광석은 고정)")]
    public long copperCostCoin = 50;
    public long silverCostCoin = 150;
    public long goldCostCoin = 400;
    public long metalCostCoin = 800;
    public long mithrilCostCoin = 2000;

    [Header("스포너 설치 비용(동적)")]
    public long spawnerBaseCost = 0;
    public long spawnerCostStep = 1000;

    [Header("선택 제한")]
    public bool blockSelectIfNoCoin = true;

    [Header("스포너 개수 카운트 기준(없으면 자동 탐색)")]
    public Transform spawnerParent; // "SlimeSpawners" 연결 추천

    [Header("선택 하이라이트 색상")]
    public Color selectedColor = new Color(1f, 0.9f, 0.2f);   // 노랑
    public Color normalColor = Color.white;

    SlimeGameManager gm;

    void Awake()
    {
        gm = SlimeGameManager.Instance;

        if (spawnerParent == null)
        {
            var found = GameObject.Find("SlimeSpawners");
            if (found != null) spawnerParent = found.transform;
        }
    }

    void Start()
    {
        if (noneButton != null) noneButton.onClick.AddListener(SetMode_None);

        if (copperButton != null) copperButton.onClick.AddListener(() => TrySetMode(SlimeBuildMode.Copper, copperCostCoin));
        if (silverButton != null) silverButton.onClick.AddListener(() => TrySetMode(SlimeBuildMode.Silver, silverCostCoin));
        if (goldButton != null) goldButton.onClick.AddListener(() => TrySetMode(SlimeBuildMode.Gold, goldCostCoin));
        if (metalButton != null) metalButton.onClick.AddListener(() => TrySetMode(SlimeBuildMode.Metal, metalCostCoin));
        if (mithrilButton != null) mithrilButton.onClick.AddListener(() => TrySetMode(SlimeBuildMode.Mithril, mithrilCostCoin));

        if (spawnerButton != null) spawnerButton.onClick.AddListener(() => TrySetMode(SlimeBuildMode.SlimeSpawner, GetSpawnerInstallCost()));

        RefreshAll();
    }

    void Update()
    {
        // 스포너 가격이 동적이라 매 프레임 갱신
        RefreshAll();
    }

    // -------------------------
    // 모드 변경
    // -------------------------
    public void SetMode_None()
    {
        currentMode = SlimeBuildMode.None;
        RefreshSelectionVisual();
    }

    void TrySetMode(SlimeBuildMode mode, long costCoin)
    {
        if (blockSelectIfNoCoin)
        {
            if (gm == null) gm = SlimeGameManager.Instance;
            if (gm != null && !gm.CanSpendResource(ResourceType.Coin, costCoin))
                return;
        }

        currentMode = mode;
        RefreshSelectionVisual();
    }

    // -------------------------
    // UI 갱신
    // -------------------------
    void RefreshAll()
    {
        RefreshCostLabels();
        RefreshInteractable();
        RefreshSelectionVisual();
    }

    void RefreshCostLabels()
    {
        if (copperCostText != null) copperCostText.text = FormatIdle(copperCostCoin);
        if (silverCostText != null) silverCostText.text = FormatIdle(silverCostCoin);
        if (goldCostText != null) goldCostText.text = FormatIdle(goldCostCoin);
        if (metalCostText != null) metalCostText.text = FormatIdle(metalCostCoin);
        if (mithrilCostText != null) mithrilCostText.text = FormatIdle(mithrilCostCoin);

        if (spawnerCostText != null) spawnerCostText.text = FormatIdle(GetSpawnerInstallCost());
    }

    void RefreshInteractable()
    {
        if (!blockSelectIfNoCoin) return;

        if (gm == null) gm = SlimeGameManager.Instance;
        if (gm == null) return;

        long coin = gm.GetResource(ResourceType.Coin);

        if (copperButton != null) copperButton.interactable = coin >= copperCostCoin;
        if (silverButton != null) silverButton.interactable = coin >= silverCostCoin;
        if (goldButton != null) goldButton.interactable = coin >= goldCostCoin;
        if (metalButton != null) metalButton.interactable = coin >= metalCostCoin;
        if (mithrilButton != null) mithrilButton.interactable = coin >= mithrilCostCoin;

        long spawnerCost = GetSpawnerInstallCost();
        if (spawnerButton != null) spawnerButton.interactable = coin >= spawnerCost;

        // 선택 중인데 돈이 부족해졌으면 자동으로 None으로
        if (currentMode == SlimeBuildMode.Copper && coin < copperCostCoin) currentMode = SlimeBuildMode.None;
        if (currentMode == SlimeBuildMode.Silver && coin < silverCostCoin) currentMode = SlimeBuildMode.None;
        if (currentMode == SlimeBuildMode.Gold && coin < goldCostCoin) currentMode = SlimeBuildMode.None;
        if (currentMode == SlimeBuildMode.Metal && coin < metalCostCoin) currentMode = SlimeBuildMode.None;
        if (currentMode == SlimeBuildMode.Mithril && coin < mithrilCostCoin) currentMode = SlimeBuildMode.None;
        if (currentMode == SlimeBuildMode.SlimeSpawner && coin < spawnerCost) currentMode = SlimeBuildMode.None;
    }

    void RefreshSelectionVisual()
    {
        // 선택된 버튼만 노랑
        SetButtonColor(noneButton, currentMode == SlimeBuildMode.None);
        SetButtonColor(copperButton, currentMode == SlimeBuildMode.Copper);
        SetButtonColor(silverButton, currentMode == SlimeBuildMode.Silver);
        SetButtonColor(goldButton, currentMode == SlimeBuildMode.Gold);
        SetButtonColor(metalButton, currentMode == SlimeBuildMode.Metal);
        SetButtonColor(mithrilButton, currentMode == SlimeBuildMode.Mithril);
        SetButtonColor(spawnerButton, currentMode == SlimeBuildMode.SlimeSpawner);
    }

    void SetButtonColor(Button btn, bool isSelected)
    {
        if (btn == null) return;

        var colors = btn.colors;
        Color c = isSelected ? selectedColor : normalColor;

        colors.normalColor = c;
        colors.selectedColor = c;
        btn.colors = colors;
    }

    // -------------------------
    // 스포너 설치 비용(동적)
    // -------------------------
    long GetSpawnerInstallCost()
    {
        int count = 0;

        if (spawnerParent != null) count = spawnerParent.childCount;
        else count = FindObjectsOfType<SlimeSpawner>().Length;

        return spawnerBaseCost + (long)count * spawnerCostStep;
    }

    // -------------------------
    // BuildRequest 반환
    // -------------------------
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
                req = BuildRequest.Spawner(GetSpawnerInstallCost());
                return true;

            default:
                return false;
        }
    }

    [Serializable]
    public struct BuildRequest
    {
        public bool isSpawner;
        public ResourceType oreType;
        public long costCoin;

        public static BuildRequest Ore(ResourceType type, long cost)
        {
            return new BuildRequest { isSpawner = false, oreType = type, costCoin = cost };
        }

        public static BuildRequest Spawner(long cost)
        {
            return new BuildRequest { isSpawner = true, oreType = ResourceType.Soil, costCoin = cost };
        }
    }

    // -------------------------
    // 표시 유틸
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

        double rounded = Math.Floor(v * 10) / 10.0;
        return rounded.ToString("0.#") + units[unitIndex];
    }
}
