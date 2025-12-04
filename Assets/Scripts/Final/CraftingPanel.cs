using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using System.Text;

public class CraftingPanel : MonoBehaviour
{
    public Inventory inventory;
    public List<CraftingRecipe> recipeList;
    public GameObject root;
    public Text plannedText;
    public Button craftButton;
    public Button clearButton;
    public Text hintText;

    readonly Dictionary<ItemType, int> planned = new();

    bool isOpen;
    void Start()
    {
        SetOpen(false);
        craftButton.onClick.AddListener(DoCraft);
        clearButton.onClick.AddListener(ClearPlanned);
        RefreshPlannedUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
            SetOpen(!isOpen);
    }
    public void SetOpen(bool open)
    {
        Cursor.lockState = CursorLockMode.None;
        isOpen = open;
        if (root)
            root.SetActive(open);

        if (!open)
            ClearPlanned();
    }
    public void AddPlanned(ItemType type, int count = 1)
    {
        if (!planned.ContainsKey(type))
            planned[type] = 0;
        planned[type] += count;

        RefreshPlannedUI();
        SetHint($"{type} x {count} 추가 완료");
    }
    public void ClearPlanned()
    {
        planned.Clear();
        RefreshPlannedUI();
        SetHint("초기화 완료");
    }
    void RefreshPlannedUI()
    {
        if (!plannedText)
            return;
        if (planned.Count == 0)
        {
            plannedText.text = "우클릭으로 재료를 추가하세요";
            return;
        }
        var sb = new StringBuilder();

        foreach (var item in planned)
            sb.AppendLine($"{item.Key} x {item.Value}");
        plannedText.text = sb.ToString();
    }
    void SetHint(string msg)
    {
        if (hintText)
            hintText.text = msg;
    }
    void DoCraft()
    {
        if (planned.Count == 0)
        {
            SetHint("재료가 부족합니다");
            return;
        }

        foreach (var plannedItem in planned)
        {
            if (inventory.GetCount(plannedItem.Key) < plannedItem.Value)
            {
                SetHint($"{plannedItem.Key} 가 부족합니다.");
                return;
            }
        }
        var matchedProduct = FindMatch(planned);
        if (matchedProduct == null)
        {
            SetHint("알맞는 레시피가 없습니다.");
            return;
        }
        foreach (var itemforconsume in planned)
            inventory.Consume(itemforconsume.Key, itemforconsume.Value);
        foreach (var p in matchedProduct.outputs)
            inventory.Add(p.type, p.count);

        ClearPlanned();

        SetHint($"조합 완료 : {matchedProduct.displayName}");
    }
    CraftingRecipe FindMatch(Dictionary<ItemType, int> planned)
    {
        foreach (var recipe in recipeList)
        {
            bool ok = true;
            foreach (var ing in recipe.inputs)
            {
                if (!planned.TryGetValue(ing.type, out int have) || have != ing.count)
                {
                    ok = false;
                    break;
                }
            }
            if (ok)
                return recipe;
        }
        return null;
    }
        
}
