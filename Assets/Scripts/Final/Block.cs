using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum ItemType
{
    Dirt,
    Grass, 
    Water,
    Diamond,
    Axe,
    Shovel,
    Pick
}

public class Block : MonoBehaviour
{
    [Header("Block Stat")]
    public ItemType type = ItemType.Dirt;
    public int maxHP = 3;
    [HideInInspector] public int hp;

    public int dropCount = 1;
    public bool mineable = true;

    private void Awake()
    {
        hp = maxHP;
        if (GetComponent<Collider>() == null) gameObject.AddComponent<BoxCollider>();
        if (string.IsNullOrEmpty(gameObject.tag) || gameObject.tag == "Untagged")
            gameObject.tag = "Block";
    }
    public void Hit(int damage, Inventory inven)
    {
        if (!mineable) return;

        hp -= damage;

        if(hp <= 0)
        {
            if (inven != null && dropCount > 0)
                inven.Add(type, dropCount);
            Destroy(gameObject);
        }
    }
}
