Shader "Custom/DoubleSided"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off  // 이게 양면 렌더링!
        
        CGPROGRAM
        #pragma surface surf Lambert

        fixed4 _Color;

        struct Input
        {
            float3 worldPos;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            o.Albedo = _Color.rgb;
        }
        ENDCG
    }
}