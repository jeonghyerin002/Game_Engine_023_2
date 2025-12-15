using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum ResourceType
{
    Coin,
    Soil,
    Copper,
    Silver,
    Gold,
    Metal,
    Mithril
}

[Serializable]
public class ResourceCountEntry
{
    public ResourceType type;
    public long count;
}

[Serializable]
public class SellPriceEntry
{
    public ResourceType type;
    public long coinPerUnit;
}

[Serializable]
public class BlockSoilYield
{
    public VoxelData.BlockType blockType;
    public int soilYield; // 블록당 soil 지급량(작으니 int)
}

public class SlimeGameManager : MonoBehaviour
{
    public static SlimeGameManager Instance { get; private set; }

    [Header("월드 레퍼런스 (SlimeGridWalker/Harvester가 사용)")]
    public VoxelWorld voxelWorld;

    [Header("자원 보유량(Inspector 표시용)")]
    public List<ResourceCountEntry> resourceCounts = new List<ResourceCountEntry>()
    {
        new ResourceCountEntry{ type = ResourceType.Coin,    count = 0 },
        new ResourceCountEntry{ type = ResourceType.Soil,    count = 0 },
        new ResourceCountEntry{ type = ResourceType.Copper,  count = 0 },
        new ResourceCountEntry{ type = ResourceType.Silver,  count = 0 },
        new ResourceCountEntry{ type = ResourceType.Gold,    count = 0 },
        new ResourceCountEntry{ type = ResourceType.Metal,   count = 0 },
        new ResourceCountEntry{ type = ResourceType.Mithril, count = 0 },
    };

    [Header("Soil 환산표 (BlockType -> Soil)")]
    public List<BlockSoilYield> soilYieldTable = new List<BlockSoilYield>()
    {
        new BlockSoilYield{ blockType = VoxelData.BlockType.Grass, soilYield = 1 },
        new BlockSoilYield{ blockType = VoxelData.BlockType.Dirt,  soilYield = 1 },
    };

    [Header("판매 가격표 (자원 1개당 Coin)")]
    public List<SellPriceEntry> sellPrices = new List<SellPriceEntry>()
    {
        new SellPriceEntry{ type = ResourceType.Soil,    coinPerUnit = 1 },
        new SellPriceEntry{ type = ResourceType.Copper,  coinPerUnit = 3 },
        new SellPriceEntry{ type = ResourceType.Silver,  coinPerUnit = 8 },
        new SellPriceEntry{ type = ResourceType.Gold,    coinPerUnit = 20 },
        new SellPriceEntry{ type = ResourceType.Metal,   coinPerUnit = 40 },
        new SellPriceEntry{ type = ResourceType.Mithril, coinPerUnit = 100 },
    };

    [Header("UI (방치형 표기)")]
    public TMP_Text coinText;
    public TMP_Text soilText;
    public TMP_Text copperText;
    public TMP_Text silverText;
    public TMP_Text goldText;
    public TMP_Text metalText;
    public TMP_Text mithrilText;

    Dictionary<ResourceType, long> resourceMap;
    Dictionary<VoxelData.BlockType, int> soilYieldMap;
    Dictionary<ResourceType, long> sellPriceMap;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        BuildResourceMapFromList();
        EnsureAllResourceTypesExist();
        SyncResourceMapToList();

        BuildSoilYieldMap();
        BuildSellPriceMap();

        RefreshResourceUI();
    }

    // -------------------------
    // Resource 기본
    // -------------------------
    void BuildResourceMapFromList()
    {
        resourceMap = new Dictionary<ResourceType, long>();
        if (resourceCounts == null) resourceCounts = new List<ResourceCountEntry>();

        for (int i = 0; i < resourceCounts.Count; i++)
        {
            var e = resourceCounts[i];
            resourceMap[e.type] = Math.Max(0, e.count);
        }
    }

    void EnsureAllResourceTypesExist()
    {
        foreach (ResourceType t in Enum.GetValues(typeof(ResourceType)))
            if (!resourceMap.ContainsKey(t))
                resourceMap[t] = 0;
    }

    void SyncResourceMapToList()
    {
        if (resourceCounts == null) return;
        for (int i = 0; i < resourceCounts.Count; i++)
        {
            resourceCounts[i].count = GetResource(resourceCounts[i].type);
        }
    }

    public long GetResource(ResourceType type)
    {
        if (resourceMap == null)
        {
            BuildResourceMapFromList();
            EnsureAllResourceTypesExist();
        }
        return resourceMap.TryGetValue(type, out long v) ? v : 0L;
    }

    public bool CanSpendResource(ResourceType type, long amount)
    {
        if (amount <= 0) return true;
        return GetResource(type) >= amount;
    }

    public void AddResource(ResourceType type, long amount)
    {
        if (amount <= 0) return;

        if (resourceMap == null)
        {
            BuildResourceMapFromList();
            EnsureAllResourceTypesExist();
        }

        resourceMap[type] = GetResource(type) + amount;
        SyncResourceMapToList();
        RefreshResourceUI();
    }

    public bool SpendResource(ResourceType type, long amount)
    {
        if (amount <= 0) return true;
        if (!CanSpendResource(type, amount)) return false;

        resourceMap[type] = GetResource(type) - amount;
        SyncResourceMapToList();
        RefreshResourceUI();
        return true;
    }

    public void SetResource(ResourceType type, long value)
    {
        if (resourceMap == null)
        {
            BuildResourceMapFromList();
            EnsureAllResourceTypesExist();
        }

        resourceMap[type] = Math.Max(0, value);
        SyncResourceMapToList();
        RefreshResourceUI();
    }

    // -------------------------
    // Harvest API (기존 스크립트 호환)
    // -------------------------
    public void CollectOreByTag(string tag)
    {
        // 태그를 Resource로 매핑
        switch (tag)
        {
            case "Copper":
            case "Ore1":
                AddResource(ResourceType.Copper, 1);
                break;

            case "Silver":
            case "Ore2":
                AddResource(ResourceType.Silver, 1);
                break;

            case "Gold":
            case "Ore3":
                AddResource(ResourceType.Gold, 1);
                break;

            case "Metal":
            case "Ore4":
                AddResource(ResourceType.Metal, 1);
                break;

            case "Mithril":
            case "Ore5":
                AddResource(ResourceType.Mithril, 1);
                break;

            case "Soil":
                AddResource(ResourceType.Soil, 1);
                break;
        }
    }

    void BuildSoilYieldMap()
    {
        soilYieldMap = new Dictionary<VoxelData.BlockType, int>();
        if (soilYieldTable == null) soilYieldTable = new List<BlockSoilYield>();

        for (int i = 0; i < soilYieldTable.Count; i++)
        {
            var e = soilYieldTable[i];
            soilYieldMap[e.blockType] = Mathf.Max(0, e.soilYield);
        }
    }

    public void CollectBlock(VoxelData.BlockType blockType)
    {
        if (soilYieldMap == null) BuildSoilYieldMap();

        if (soilYieldMap.TryGetValue(blockType, out int yield) && yield > 0)
        {
            AddResource(ResourceType.Soil, yield);
        }
    }

    // -------------------------
    // 판매 시스템
    // -------------------------
    void BuildSellPriceMap()
    {
        sellPriceMap = new Dictionary<ResourceType, long>();
        if (sellPrices == null) sellPrices = new List<SellPriceEntry>();

        for (int i = 0; i < sellPrices.Count; i++)
        {
            var e = sellPrices[i];
            sellPriceMap[e.type] = Math.Max(0, e.coinPerUnit);
        }
    }

    public long SellAll(ResourceType type)
    {
        if (type == ResourceType.Coin) return 0;
        if (sellPriceMap == null) BuildSellPriceMap();
        if (!sellPriceMap.TryGetValue(type, out long price) || price <= 0) return 0;

        long have = GetResource(type);
        if (have <= 0) return 0;

        long gain = have * price;

        // 자원 0으로 만들고, 코인 추가
        SetResource(type, 0);
        AddResource(ResourceType.Coin, gain);

        return gain;
    }

    // 버튼 1개: Coin 제외 전부 판매
    public long SellAllResources()
    {
        long totalGain = 0;

        foreach (ResourceType t in Enum.GetValues(typeof(ResourceType)))
        {
            if (t == ResourceType.Coin) continue;
            totalGain += SellAll(t);
        }

        return totalGain;
    }

    // -------------------------
    // UI
    // -------------------------
    public void RefreshResourceUI()
    {
        if (coinText != null) coinText.text = FormatIdle(GetResource(ResourceType.Coin));
        if (soilText != null) soilText.text = FormatIdle(GetResource(ResourceType.Soil));
        if (copperText != null) copperText.text = FormatIdle(GetResource(ResourceType.Copper));
        if (silverText != null) silverText.text = FormatIdle(GetResource(ResourceType.Silver));
        if (goldText != null) goldText.text = FormatIdle(GetResource(ResourceType.Gold));
        if (metalText != null) metalText.text = FormatIdle(GetResource(ResourceType.Metal));
        if (mithrilText != null) mithrilText.text = FormatIdle(GetResource(ResourceType.Mithril));
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

        double rounded = Math.Floor(v * 10) / 10.0;
        return rounded.ToString("0.#") + units[unitIndex];
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!Application.isPlaying) return;
        BuildSoilYieldMap();
        BuildSellPriceMap();
    }
#endif
}
