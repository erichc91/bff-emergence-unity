Shader "Hidden/BFFBloom"
{
    Properties { _MainTex ("", 2D) = "white" {} }

    CGINCLUDE
    #include "UnityCG.cginc"

    sampler2D _MainTex;
    float4    _MainTex_TexelSize;
    sampler2D _BloomTex;

    float _Threshold;
    float _Intensity;
    float _BlurSpread;

    struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

    v2f vert(appdata_img v)
    {
        v2f o;
        o.pos = UnityObjectToClipPos(v.vertex);
        o.uv  = v.texcoord;
        return o;
    }

    // Pass 0: Extract pixels above brightness threshold
    fixed4 fragBrightPass(v2f i) : SV_Target
    {
        fixed4 col = tex2D(_MainTex, i.uv);
        float  lum = dot(col.rgb, float3(0.2126, 0.7152, 0.0722));
        float  w   = max(0.0, lum - _Threshold) / max(lum, 0.0001);
        return col * w;
    }

    // Pass 1: Horizontal 9-tap Gaussian blur
    fixed4 fragBlurH(v2f i) : SV_Target
    {
        float2 d = float2(_MainTex_TexelSize.x * _BlurSpread, 0);
        fixed4 s = tex2D(_MainTex, i.uv) * 0.2270270270;
        s += (tex2D(_MainTex, i.uv + d*1) + tex2D(_MainTex, i.uv - d*1)) * 0.1945945946;
        s += (tex2D(_MainTex, i.uv + d*2) + tex2D(_MainTex, i.uv - d*2)) * 0.1216216216;
        s += (tex2D(_MainTex, i.uv + d*3) + tex2D(_MainTex, i.uv - d*3)) * 0.0540540541;
        s += (tex2D(_MainTex, i.uv + d*4) + tex2D(_MainTex, i.uv - d*4)) * 0.0162162162;
        return s;
    }

    // Pass 2: Vertical 9-tap Gaussian blur
    fixed4 fragBlurV(v2f i) : SV_Target
    {
        float2 d = float2(0, _MainTex_TexelSize.y * _BlurSpread);
        fixed4 s = tex2D(_MainTex, i.uv) * 0.2270270270;
        s += (tex2D(_MainTex, i.uv + d*1) + tex2D(_MainTex, i.uv - d*1)) * 0.1945945946;
        s += (tex2D(_MainTex, i.uv + d*2) + tex2D(_MainTex, i.uv - d*2)) * 0.1216216216;
        s += (tex2D(_MainTex, i.uv + d*3) + tex2D(_MainTex, i.uv - d*3)) * 0.0540540541;
        s += (tex2D(_MainTex, i.uv + d*4) + tex2D(_MainTex, i.uv - d*4)) * 0.0162162162;
        return s;
    }

    // Pass 3: Composite — additive blend bloom onto original
    fixed4 fragComposite(v2f i) : SV_Target
    {
        fixed4 original = tex2D(_MainTex, i.uv);
        fixed4 bloom    = tex2D(_BloomTex, i.uv) * _Intensity;
        return original + bloom;
    }
    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass { CGPROGRAM #pragma vertex vert #pragma fragment fragBrightPass ENDCG }  // 0
        Pass { CGPROGRAM #pragma vertex vert #pragma fragment fragBlurH       ENDCG }  // 1
        Pass { CGPROGRAM #pragma vertex vert #pragma fragment fragBlurV       ENDCG }  // 2
        Pass { CGPROGRAM #pragma vertex vert #pragma fragment fragComposite   ENDCG }  // 3
    }
}
