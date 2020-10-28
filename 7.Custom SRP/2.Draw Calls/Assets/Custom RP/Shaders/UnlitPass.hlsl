#ifndef CUSTOM_UNLIT_PASS_INCLUDED
#define CUSTOM_UNLIT_PASS_INCLUDED

float4 UnlitPassVertex(float 3 positionOS : POSITION) : SV_POSITION
{
    return float4(positionOS, 1.0);
}

float4 UnlitPassFragment() : SV_TARGET
{
    return float4(0, 0, 0, 1);
}

#endif