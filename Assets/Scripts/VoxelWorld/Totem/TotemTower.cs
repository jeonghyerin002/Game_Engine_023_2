using UnityEngine;

public class TotemTower : MonoBehaviour
{
    public TotemType type = TotemType.Swift;
    public float radius = 6f;

    public void ApplyData(TotemData data)
    {
        if (data == null) return;
        type = data.type;
        radius = data.radius;
    }
}