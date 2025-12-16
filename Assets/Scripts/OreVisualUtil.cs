using UnityEngine;

public static class OreVisualUtil
{
    // 광석 프리미어 큐브
    public static void ApplyOreMaterial(GameObject obj, ResourceType type)
    {
        var r = obj.GetComponent<MeshRenderer>();
        if (r == null) return;

        var mat = new Material(Shader.Find("Standard"));
        Color baseColor = GetOreColor(type);

        mat.color = baseColor;

        // 프리미어/크리스탈 느낌
        mat.SetFloat("_Metallic", GetMetallic(type));
        mat.SetFloat("_Glossiness", GetSmoothness(type));

        // 은은한 발광
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", baseColor * GetEmission(type));

        r.material = mat;
    }

    // 스포너 핑크 프리미어 큐브
    public static void ApplySpawnerMaterial(GameObject obj)
    {
        var r = obj.GetComponent<MeshRenderer>();
        if (r == null) return;

        var mat = new Material(Shader.Find("Standard"));

        Color pink = new Color(1f, 0.35f, 0.85f);
        mat.color = pink;

        mat.SetFloat("_Metallic", 0.35f);
        mat.SetFloat("_Glossiness", 0.92f);

        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", new Color(1f, 0.18f, 0.6f) * 0.55f);

        r.material = mat;
    }

    static Color GetOreColor(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Copper: return new Color(0.784f, 0.459f, 0.20f);
            case ResourceType.Silver: return new Color(0.84f, 0.86f, 0.90f);
            case ResourceType.Gold: return new Color(0.949f, 0.788f, 0.298f);
            case ResourceType.Metal: return new Color(0.48f, 0.54f, 0.60f);
            case ResourceType.Mithril: return new Color(0.48f, 0.42f, 1.00f);
            default: return Color.gray;
        }
    }

    static float GetMetallic(ResourceType type)
    {
        switch (type)
        {
            // 금속 느낌 더 강하게
            case ResourceType.Gold:
            case ResourceType.Silver:
            case ResourceType.Metal: return 0.92f;

            // 동은 살짝 낮게
            case ResourceType.Copper: return 0.85f;

            // 미스릴은 크리스탈/마법광 느낌: 금속은 조금 낮추고 광택/발광으로
            case ResourceType.Mithril: return 0.65f;

            default: return 0.7f;
        }
    }

    static float GetSmoothness(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Mithril: return 0.95f;
            default: return 0.90f;
        }
    }

    static float GetEmission(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Mithril: return 0.70f; // 미스릴은 더 반짝
            case ResourceType.Gold: return 0.45f;
            case ResourceType.Silver: return 0.40f;
            default: return 0.30f;
        }
    }
}
