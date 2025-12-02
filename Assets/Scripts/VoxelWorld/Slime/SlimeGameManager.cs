using UnityEngine;
using TMPro;

public class SlimeGameManager : MonoBehaviour
{
    public static SlimeGameManager Instance { get; private set; }

    [Header("월드 레퍼런스")]
    public VoxelWorld voxelWorld;

    [Header("자원 카운트")]
    public int ore1, ore2, ore3, ore4, ore5;
    public int grassCount, dirtCount;

    [Header("UI")]
    public TMP_Text oreText;
    public TMP_Text blockText;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void CollectOreByTag(string tag)
    {
        switch (tag)
        {
            case "Ore1": ore1++; break;
            case "Ore2": ore2++; break;
            case "Ore3": ore3++; break;
            case "Ore4": ore4++; break;
            case "Ore5": ore5++; break;
        }
        RefreshUI();
    }

    public void CollectBlock(VoxelData.BlockType blockType)
    {
        if (blockType == VoxelData.BlockType.Grass) grassCount++;
        else if (blockType == VoxelData.BlockType.Dirt) dirtCount++;

        RefreshUI();
    }

    void RefreshUI()
    {
        if (oreText != null)
        {
            oreText.text = $"Ore1:{ore1}  Ore2:{ore2}  Ore3:{ore3}  Ore4:{ore4}  Ore5:{ore5}";
        }

        if (blockText != null)
        {
            blockText.text = $"Grass:{grassCount}  Dirt:{dirtCount}";
        }
    }
}
