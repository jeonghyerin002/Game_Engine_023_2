using UnityEngine;

public class OreMaterialBank : MonoBehaviour
{
    [Header("Fallback Shader (drag a Shader asset if possible)")]
    [SerializeField] private Shader fallbackShader; // 가능하면 Inspector에서 Standard Shader를 드래그

    [Header("If fallbackShader is empty, this name will be used")]
    [SerializeField] private string fallbackShaderName = "Standard";

    [Header("Runtime Materials (auto created)")]
    public Material copperMat;
    public Material silverMat;
    public Material goldMat;
    public Material metalMat;
    public Material mithrilMat;

    void Awake()
    {
        var shader = fallbackShader != null ? fallbackShader : Shader.Find(fallbackShaderName);
        if (shader == null)
        {
            // 최후의 안전장치 (거의 안 옴)
            shader = Shader.Find("Legacy Shaders/Diffuse");
        }

        copperMat = Create(shader, new Color(0.784f, 0.459f, 0.2f), 0.9f, 0.75f);
        silverMat = Create(shader, new Color(0.79f, 0.81f, 0.84f), 0.9f, 0.75f);
        goldMat = Create(shader, new Color(0.949f, 0.788f, 0.298f), 0.9f, 0.75f);
        metalMat = Create(shader, new Color(0.48f, 0.54f, 0.60f), 0.9f, 0.75f);
        mithrilMat = Create(shader, new Color(0.48f, 0.42f, 1.0f), 0.9f, 0.75f);
    }

    Material Create(Shader shader, Color c, float metallic, float smoothness)
    {
        var m = new Material(shader);
        m.color = c;

        // Standard 계열이면 아래가 먹힘 (셰이더에 프로퍼티 없으면 무시됨)
        if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
        if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", smoothness);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);

        return m;
    }

    public Material Get(ResourceType type)
    {
        return type switch
        {
            ResourceType.Copper => copperMat,
            ResourceType.Silver => silverMat,
            ResourceType.Gold => goldMat,
            ResourceType.Metal => metalMat,
            ResourceType.Mithril => mithrilMat,
            _ => null
        };
    }
}
