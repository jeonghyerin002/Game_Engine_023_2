using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TotemBuildUI : MonoBehaviour
{
    [System.Serializable]
    public class TotemButtonEntry
    {
        public TotemType type;
        public Button button;
        public TMP_Text costText;   // 선택: 가격 표시
    }

    [Header("Buttons")]
    public TotemButtonEntry[] entries;

    [Header("Info Text (선택)")]
    public TMP_Text titleText;
    public TMP_Text descText;

    [Header("선택 제한")]
    public bool blockSelectIfNoCoin = true;

    [Header("선택 하이라이트 색상")]
    public Color selectedColor = new Color(1f, 0.9f, 0.2f);   // 노랑
    public Color normalColor = Color.white;

    bool selected;
    TotemType selectedType;

    void Start()
    {
        if (entries != null)
        {
            for (int i = 0; i < entries.Length; i++)
            {
                var e = entries[i];
                if (e == null || e.button == null) continue;

                TotemType captured = e.type;
                e.button.onClick.AddListener(() => TrySelect(captured));
            }
        }

        RefreshAll();
    }

    void Update()
    {
        RefreshAll();
    }

    void TrySelect(TotemType type)
    {
        var data = TotemDatabase.Instance != null ? TotemDatabase.Instance.Get(type) : null;
        long cost = data != null ? data.buildCostCoin : 0;

        if (blockSelectIfNoCoin)
        {
            var gm = SlimeGameManager.Instance;
            if (gm != null && !gm.CanSpendResource(ResourceType.Coin, cost))
                return;
        }

        selected = true;
        selectedType = type;

        RefreshSelectionVisual();
        RefreshInfo();
    }

    public void ClearSelection()
    {
        selected = false;
        RefreshSelectionVisual();
        RefreshInfo();
    }

    public bool TryGetTotemRequest(out TotemType type, out long costCoin)
    {
        type = default;
        costCoin = 0;

        if (!selected) return false;

        var data = TotemDatabase.Instance != null ? TotemDatabase.Instance.Get(selectedType) : null;
        type = selectedType;
        costCoin = data != null ? data.buildCostCoin : 0;
        return true;
    }

    void RefreshAll()
    {
        var gm = SlimeGameManager.Instance;
        long coin = (gm != null) ? gm.GetResource(ResourceType.Coin) : 0;

        if (entries != null)
        {
            for (int i = 0; i < entries.Length; i++)
            {
                var e = entries[i];
                if (e == null) continue;

                var data = TotemDatabase.Instance != null ? TotemDatabase.Instance.Get(e.type) : null;
                long cost = data != null ? data.buildCostCoin : 0;

                // 가격 표시
                if (e.costText != null)
                    e.costText.text = cost.ToString();

                // 버튼 활성/비활성
                if (e.button != null && blockSelectIfNoCoin)
                    e.button.interactable = coin >= cost;
            }
        }

        // 선택 중인데 돈이 부족해졌으면 자동 해제
        if (selected && blockSelectIfNoCoin)
        {
            var data = TotemDatabase.Instance != null ? TotemDatabase.Instance.Get(selectedType) : null;
            long cost = data != null ? data.buildCostCoin : 0;

            if (coin < cost)
                ClearSelection();
        }

        RefreshSelectionVisual();
    }

    void RefreshSelectionVisual()
    {
        if (entries == null) return;

        for (int i = 0; i < entries.Length; i++)
        {
            var e = entries[i];
            if (e == null || e.button == null) continue;

            bool isSelected = selected && e.type == selectedType;
            SetButtonColor(e.button, isSelected ? selectedColor : normalColor);
        }
    }

    void SetButtonColor(Button btn, Color color)
    {
        var colors = btn.colors;
        colors.normalColor = color;
        colors.selectedColor = color;
        btn.colors = colors;
    }

    void RefreshInfo()
    {
        if (!selected)
        {
            if (titleText != null) titleText.text = "토탬 선택";
            if (descText != null) descText.text = "";
            return;
        }

        var data = TotemDatabase.Instance != null ? TotemDatabase.Instance.Get(selectedType) : null;
        if (data == null) return;

        if (titleText != null) titleText.text = data.displayName;
        if (descText != null) descText.text = data.desc;
    }
}
