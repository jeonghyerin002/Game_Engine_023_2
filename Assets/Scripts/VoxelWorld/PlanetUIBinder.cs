using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlanetUIBinder : MonoBehaviour
{
    [Header("Refs")]
    public PlanetManager planetManager;
    public SlimePlanetSaveManager planetSave;

    [Header("Buttons")]
    public Button btnNew;
    public Button btnPrev;
    public Button btnNext;
    public Button btnDelete;

    [Header("UI")]
    public TMP_Text infoText;
    public TMP_Text createCostText; // 선택: 생성 비용 표시

    [Header("행성 생성 비용 사용")]
    public bool enableCreatePlanetCost = true;

    void Start()
    {
        if (planetManager == null) planetManager = PlanetManager.Instance;

        btnNew.onClick.AddListener(OnNewPlanet);
        btnPrev.onClick.AddListener(OnPrev);
        btnNext.onClick.AddListener(OnNext);
        btnDelete.onClick.AddListener(OnDelete);

        planetSave.LoadCurrentPlanet();
        Refresh();
    }

    void Update()
    {
        if (createCostText != null)
        {
            long cost = enableCreatePlanetCost ? GetNextPlanetCreateCost() : 0;
            createCostText.text = enableCreatePlanetCost ? FormatIdle(cost) : "-";
        }
    }

    void OnNewPlanet()
    {
        if (planetManager == null) return;

        if (enableCreatePlanetCost)
        {
            var gm = SlimeGameManager.Instance;
            if (gm == null) return;

            long cost = GetNextPlanetCreateCost();
            if (!gm.CanSpendResource(ResourceType.Coin, cost)) return;
            if (!gm.SpendResource(ResourceType.Coin, cost)) return;
        }

        planetManager.CreatePlanet();
        planetSave.LoadCurrentPlanet();
        Refresh();
    }

    void OnPrev()
    {
        planetManager.PrevPlanet();
        planetSave.LoadCurrentPlanet();
        Refresh();
    }

    void OnNext()
    {
        planetManager.NextPlanet();
        planetSave.LoadCurrentPlanet();
        Refresh();
    }

    void OnDelete()
    {
        planetManager.DeletePlanet();
        planetSave.LoadCurrentPlanet();
        Refresh();
    }

    // =========================
    // 비용 규칙
    // 1~4번째: 0원
    // 5번째: 10000원
    // 6번째: 100000원
    // 이후: *10
    // =========================
    long GetNextPlanetCreateCost()
    {
        int currentCount = (planetManager != null && planetManager.planets != null) ? planetManager.planets.Count : 1;
        int nextCount = currentCount + 1; // 새 행성을 만들면 총 몇 개인지

        if (nextCount <= 4) return 0;      // 2,3,4번째 생성은 0원
        if (nextCount == 5) return 10000;  // 5번째부터 비용 발생

        // nextCount=6 -> 100000
        // nextCount=7 -> 1000000 ...
        int power = nextCount - 5; // 6->1, 7->2 ...
        long cost = 10000;

        for (int i = 0; i < power; i++)
        {
            // overflow 방지: 너무 커지면 long 최대값으로 클램프
            if (cost > long.MaxValue / 10) return long.MaxValue;
            cost *= 10;
        }

        return cost;
    }

    void Refresh()
    {
        var p = planetManager.CurrentPlanet;
        if (p == null) return;

        infoText.text = $"{planetManager.currentIndex + 1}/{planetManager.planets.Count}  {p.name}\nSeed: {p.seed}";
    }

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

        double rounded = System.Math.Floor(v * 10) / 10.0;
        return rounded.ToString("0.#") + units[unitIndex];
    }
}
