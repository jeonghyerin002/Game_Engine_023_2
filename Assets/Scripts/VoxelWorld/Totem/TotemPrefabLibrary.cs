using UnityEngine;

public class TotemPrefabLibrary : MonoBehaviour
{
    [Header("Totem Prefabs by Type")]
    public GameObject swiftPrefab;
    public GameObject productionPrefab;
    public GameObject harvestPrefab;
    public GameObject expansionPrefab;
    public GameObject duplicationPrefab;
    public GameObject stabilityPrefab;

    public GameObject GetPrefab(TotemType type)
    {
        return type switch
        {
            TotemType.Swift => swiftPrefab,
            TotemType.Production => productionPrefab,
            TotemType.Harvest => harvestPrefab,
            TotemType.Expansion => expansionPrefab,
            TotemType.Duplication => duplicationPrefab,
            TotemType.Stability => stabilityPrefab,
            _ => null
        };
    }
}
