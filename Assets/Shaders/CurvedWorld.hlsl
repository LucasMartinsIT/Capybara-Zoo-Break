#ifndef CURVED_WORLD_INCLUDED
#define CURVED_WORLD_INCLUDED

// Variáveis globais enviadas pela CPU para TODOS os shaders ao mesmo tempo
float _GlobalCurveX;
float _GlobalCurveY;
float _GlobalFlatZoneZ;

void ApplyCurvedWorld_float(float3 WorldPos, float CurvatureY, float CurvatureX, out float3 OutWorldPos)
{
    float3 absWorldPos = GetAbsolutePositionWS(WorldPos);
    float distZ = max(0.0, (absWorldPos.z - _WorldSpaceCameraPos.z) - _GlobalFlatZoneZ);
    float distSq = distZ * distZ;

    OutWorldPos = WorldPos;
    OutWorldPos.x += distSq * _GlobalCurveX;
    OutWorldPos.y -= distSq * _GlobalCurveY;
}

#endif