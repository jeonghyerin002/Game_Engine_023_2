using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class InventoryUI : MonoBehaviour
{
    public Sprite dirtBlockImage;
    public Sprite grassBlockImage;
    public Inventory inventory;

    public List<Transform> slot;
    public GameObject SlotItem;
    List<GameObject> items;

    void Update()
    {
        UpdateInventory(inventory);
    }

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
            var go = Instantiate(SlotItem, slot[idx], transform);
            go.transform.localPosition = Vector3.zero;
            SlotItemPrefab sItem = go.GetComponent<SlotItemPrefab>();
            items.Add(go);

            switch (item.Key)
            {
                case BlockType.Dirt:
                    sItem.ItemSetting(dirtBlockImage, item.Value.ToString());
                    break;

                case BlockType.Grass:
                    sItem.ItemSetting(grassBlockImage, item.Value.ToString());
                    break;
            }
            idx++;
        }
    }
}
