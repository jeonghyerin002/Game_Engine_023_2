using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public Sprite dirtBlockImage;
    public Sprite grassBlockImage;
    public Sprite diaBlockImage;
    public Sprite axeImage;
    public Sprite ShovelImage;
    public Sprite PickImage;

    public List<Transform> Slot = new List<Transform>();
    public GameObject SlotItem;
    List<GameObject> items = new List<GameObject>();

    public int selectedIndex = -1;
    
    public void UpdateInventory(Inventory myInven)
    {
        foreach(var slotItems in items)
        {
            Destroy(slotItems);
        }
        items.Clear();

        int idx = 0;
        foreach(var item in myInven.items)
        {
            var go = Instantiate(SlotItem, Slot[idx], transform);
            go.transform.localPosition = Vector3.zero;
            SlotItemPrefab sItem = go.GetComponent<SlotItemPrefab>();
            items.Add(go);

            switch (item.Key)
            {
                case ItemType.Dirt:
                    sItem.ItemSetting(dirtBlockImage, "x"+ item.Value.ToString(), item.Key);
                    break;

                case ItemType.Grass:
                    sItem.ItemSetting(grassBlockImage, "x" + item.Value.ToString(), item.Key);
                    break;
                case ItemType.Diamond:
                    sItem.ItemSetting(diaBlockImage, "x" + item.Value.ToString(), item.Key);
                    break;
                case ItemType.Axe:
                    sItem.ItemSetting(axeImage, "x" + item.Value.ToString(), item.Key);
                    break;
                case ItemType.Shovel:
                    sItem.ItemSetting(ShovelImage, "x" + item.Value.ToString(), item.Key);
                    break;
                        case ItemType.Pick:
                    sItem.ItemSetting(PickImage, "x" + item.Value.ToString(), item.Key);
                    break;
            }
            idx++;
            
        }
    }
    void Update()
    {
        for(int i = 0; i < Mathf.Min(9, Slot.Count); i++)
        {
            if(Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SetSelectedIndex(i);
            }
        }
    }
    public void SetSelectedIndex(int idx)
    {
        ResetSelection();
        if(selectedIndex == idx)
        {
            selectedIndex = -1;
        }
        else
        {
            if(idx >= items.Count)
            {
                selectedIndex = -1;
            }
            else
            {
                SetSelectedIndex(idx);
                selectedIndex = idx;
            }
        }
    }
    public void ResetSelection()
    {
        foreach (var slot in Slot)
        {
            slot.GetComponent<Image>().color = Color.white;
        }
    }
    void SetSelection(int _idx)
    {
        Slot[_idx].GetComponent<Image>().color = Color.yellow;
    }
    public ItemType GetInventorySlot()
    {
        return items[selectedIndex].GetComponent<SlotItemPrefab>().blockType;
    }
}
