using System.Collections.Generic;
using UnityEngine;

public class TotemDatabase : MonoBehaviour
{
    public static TotemDatabase Instance { get; private set; }

    [Header("Totems")]
    public List<TotemData> totems = new List<TotemData>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (totems == null) totems = new List<TotemData>();
        if (totems.Count == 0) CreateDefaults();
    }

    public TotemData Get(TotemType type)
    {
        for (int i = 0; i < totems.Count; i++)
            if (totems[i].type == type) return totems[i];
        return null;
    }

    void CreateDefaults()
    {
        // 1) 속도
        totems.Add(new TotemData
        {
            type = TotemType.Swift,
            displayName = "속도 토탬",
            desc = "슬라임 이동 속도 +25%",
            buildCostCoin = 2000,
            radius = 6f,
            v1 = 1.25f, // speed multiplier
            stackRule = TotemStackRule.MaxOnly
        });

        // 2) 생산
        totems.Add(new TotemData
        {
            type = TotemType.Production,
            displayName = "생산 토탬",
            desc = "스포너 생성 속도 +30%",
            buildCostCoin = 4000,
            radius = 6f,
            v1 = 1.30f, // productivity multiplier
            stackRule = TotemStackRule.Multiply
        });

        // 3) 수확
        totems.Add(new TotemData
        {
            type = TotemType.Harvest,
            displayName = "수확 토탬",
            desc = "자원 획득량 +20%",
            buildCostCoin = 8000,
            radius = 6f,
            v1 = 1.20f, // harvest multiplier
            stackRule = TotemStackRule.MaxOnly
        });

        // 4) 범위(메타 토탬)
        totems.Add(new TotemData
        {
            type = TotemType.Expansion,
            displayName = "범위 토탬",
            desc = "주변 토탬 반경 +50%",
            buildCostCoin = 12000,
            radius = 6f,
            v1 = 1.50f, // radius multiplier to other totems
            stackRule = TotemStackRule.Multiply
        });

        // 5) 증식
        totems.Add(new TotemData
        {
            type = TotemType.Duplication,
            displayName = "증식 토탬",
            desc = "추가 생성 확률 +10%",
            buildCostCoin = 20000,
            radius = 6f,
            v1 = 0.10f, // extra spawn chance
            stackRule = TotemStackRule.MaxOnly
        });

        // 6) 안정
        totems.Add(new TotemData
        {
            type = TotemType.Stability,
            displayName = "안정 토탬",
            desc = "생산 안정성 +15%",
            buildCostCoin = 25000,
            radius = 6f,
            v1 = 1.15f, // stability multiplier
            stackRule = TotemStackRule.MaxOnly
        });
    }
}
