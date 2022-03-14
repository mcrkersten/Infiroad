Shader "AquariusMax/Leaves_Gradient_A"
{
    Properties
    {
        _Color ("BottomColor", Color) = (1,1,1,1)
        _Color2 ("TopColor", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}

    }
    SubShader
    {
        Tags { "RenderType"="TransparentCutout" }
       
        //LOD 200
        Cull off

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Lambert addshadow

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            //float4 screenPos;
            //float4 pos;
        };

        fixed4 _Color;
        fixed4 _Color2;

        void surf (Input IN, inout SurfaceOutput o)
        {
           // float heightGradient = (IN.pos.y + 1) * 0.5;
            //float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
            // Albedo comes from a texture tinted by color
            //lerpWeight = (screenUV.y - lowestVertex.y) / (highestVertex.y - lowestVertex.y)
            fixed4 gradColor = lerp(_Color, _Color2, IN.uv_MainTex.y);
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * gradColor;
            //fixed4 c = lerp(_Color, _Color2, lerpWeight)
            o.Albedo = c.rgb;
            o.Alpha = c.a;
            clip(o.Alpha - 0.5 * IN.uv_MainTex.y);
        }
        ENDCG
    }
    FallBack "VertexLit"
}
