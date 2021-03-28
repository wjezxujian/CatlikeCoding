#ifndef CUSTOM_LIT_PASS_INCLUDED
#define CUSTOM_LIT_PASS_INCLUDED

#include "../ShaderLibrary/Surface.hlsl"
#include "../ShaderLibrary/Shadows.hlsl"
#include "../ShaderLibrary/Light.hlsl"
#include "../ShaderLibrary/BRDF.hlsl"
#include "../ShaderLibrary/GI.hlsl"
#include "../ShaderLibrary/Lighting.hlsl"

struct Attributes {
    float3 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float2 baseUV : TEXCOORD0;    
    GI_ATTRIBUTE_DATA
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings {
    float4 positionCS : SV_POSITION;
    float3 positionWS : VAR_POSITION;
    float3 normalWS : VAR_NORMAL;
#if defined(_NORMAL_MAP)
    float4 tangentWS : VAR_TANAGENT;
#endif
    float2 baseUV : VAR_BASE_UV;
#if defined(_DETAIL_MAP)    
    float2 detailUV : VAR_DETAIL_UV;
#endif
    GI_VARYINGS_DATA
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings LitPassVertex(Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    TRANSFER_GI_DATA(input, output);
    output.positionWS = TransformObjectToWorld(input.positionOS);
    output.positionCS = TransformWorldToHClip(output.positionWS);
    output.normalWS = TransformObjectToWorldNormal(input.normalOS);
#if defined(_NORMAL_MAP)
    output.tangentWS = float4(TransformObjectToWorldDir(input.tangentOS.xyz), input.tangentOS.w);
#endif

    // float4 baseST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseMap_ST);
    // output.baseUV = input.baseUV * baseST.xy + baseST.zw;
    output.baseUV = TransformBaseUV(input.baseUV);
#if defined(_DETAIL_MAP)
    output.detailUV = TransformDetailUV(input.baseUV);
#endif
    return output;
}

float4 LitPassFragment(Varyings input) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
// #if defined(LOD_FADE_CROSSFADE)
//     return -unity_LODFade.x;
// #endif
    ClipLOD(input.positionCS.xy, unity_LODFade.x);
    
    // float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.baseUV);
    // float4 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
    // float4 base = baseMap * baseColor;
    InputConfig config = GetInputConfig(input.baseUV);
#if defined(_MASK_MAP)
    config.useMask = true;
#endif
#if defined(_DETAIL_MAP)
    config.detailUV = input.detailUV;
    config.useDetail = true;
#endif
    float4 base = GetBase(config);
#if defined(_CLIPPING)
    // clip(base.a - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Cutoff));
    clip(base.a - GetCutoff(config));
#endif

    // base.rgb = abs(length(input.normalWS) - 1.0) * 10;
    // base.rgb = normalize(input.normalWS);
    // return base;

    Surface surface;
    surface.position = input.positionWS;
#if defined(_NORMAL_MAP)
    surface.normal = NormalTangentToWorld(GetNormalTS(config), input.normalWS, input.tangentWS);
#else
    surface.normal = normalize(input.normalWS);
#endif
    surface.interpolatedNormal = surface.normal;
    surface.viewDirection = normalize(_WorldSpaceCameraPos - input.positionWS);
    surface.depth = -TransformWorldToView(input.positionWS).z;
    surface.renderingLayerMask = asuint(unity_RenderingLayer.x);
    surface.color = base.rgb;
    surface.alpha = base.a;
    // surface.metallic = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Metallic);
    // surface.smoothness = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Smoothness);
    surface.metallic = GetMetallic(config);
    surface.occlusion = GetOcclusion(config);
    surface.smoothness = GetSmoothness(config);
    surface.fresnelStrength = GetFresnel(config);
    surface.dither = InterleavedGradientNoise(input.positionCS.xy, 0);
#if defined(_PREMULTIPLY_ALPHA)
    BRDF brdf = GetBRDF(surface, true);
#else
    BRDF brdf = GetBRDF(surface);
#endif
    GI gi = GetGI(GI_FRAGMENT_DATA(input), surface, brdf);
    float3 color = GetLighting(surface, brdf, gi);
    color += GetEmission(config);
    return float4(color, GetFinalAlpha(surface.alpha));
}

#endif