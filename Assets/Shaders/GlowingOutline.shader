Shader "Custom/GlowingOutline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Main Color", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1,0,0,1)
        _OutlineWidth ("Outline Width", Range(0.0, 10.0)) = 2.0
        _GlowStrength ("Glow Strength", Range(0.0, 5.0)) = 1.0
        _GlowSpeed ("Glow Pulse Speed", Range(0.0, 10.0)) = 2.0
        _GlowIntensity ("Glow Intensity", Range(0.0, 3.0)) = 1.0
        _FresnelPower ("Fresnel Power", Range(0.0, 10.0)) = 2.0
        _RimColor ("Rim Color", Color) = (0,1,1,1)
        _RimStrength ("Rim Strength", Range(0.0, 3.0)) = 1.0
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
        LOD 100
        
        // First pass: Render the outline
        Pass
        {
            Name "Outline"
            Cull Front
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
            };
            
            float4 _OutlineColor;
            float _OutlineWidth;
            float _GlowStrength;
            float _GlowSpeed;
            float _GlowIntensity;
            float _FresnelPower;
            float4 _RimColor;
            float _RimStrength;
            
            v2f vert (appdata v)
            {
                v2f o;
                float3 normal = normalize(v.normal);
                float3 outlineOffset = normal * (_OutlineWidth * 0.01);
                float3 position = v.vertex + outlineOffset;
                
                o.vertex = UnityObjectToClipPos(position);
                o.uv = v.uv;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Calculate fresnel effect
                float fresnel = pow(1.0 - saturate(dot(i.worldNormal, i.viewDir)), _FresnelPower);
                
                // Calculate pulsing glow
                float pulse = sin(_Time.y * _GlowSpeed) * 0.5 + 0.5;
                float glow = _GlowStrength * pulse * _GlowIntensity;
                
                // Combine outline color with glow and fresnel
                float4 outline = _OutlineColor;
                outline.rgb += _RimColor.rgb * fresnel * _RimStrength;
                outline.rgb *= (1.0 + glow);
                
                return outline;
            }
            ENDCG
        }
        
        // Second pass: Render the main object
        Pass
        {
            Name "Main"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _GlowStrength;
            float _GlowSpeed;
            float _GlowIntensity;
            float _FresnelPower;
            float4 _RimColor;
            float _RimStrength;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                
                // Calculate fresnel effect
                float fresnel = pow(1.0 - saturate(dot(i.worldNormal, i.viewDir)), _FresnelPower);
                
                // Calculate pulsing glow
                float pulse = sin(_Time.y * _GlowSpeed) * 0.5 + 0.5;
                float glow = _GlowStrength * pulse * _GlowIntensity;
                
                // Add rim lighting
                col.rgb += _RimColor.rgb * fresnel * _RimStrength;
                
                // Add glow to the main color
                col.rgb *= (1.0 + glow * 0.3);
                
                return col;
            }
            ENDCG
        }
    }
    
    Fallback "Diffuse"
} 