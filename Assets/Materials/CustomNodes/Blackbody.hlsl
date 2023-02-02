#ifndef BLACKBODY_HDR_INCLUDED
#define BLACKBODY_HDR_INCLUDED

// Magic numbers from
//https://docs.unity3d.com/Packages/com.unity.shadergraph@8.0/manual/Blackbody-Node.html
void Blackbody_Hdr_float(float Temperature, out float3 Out)
{
    float tempPow = pow(Temperature,(-3.0 / 2.0));
    float tempLog = log(max(Temperature,0));
    float3 color;
    color.x = 220000 * tempPow + 0.580392;

    if (Temperature > 6500.0) color.y = 138039.215686 * tempPow + 0.721569;
    else color.y = 0.392314 * tempLog - 2.44549;

    color.z = 0.76149 * tempLog - 5.680784;

    color = clamp(color, 0.0, 1);
    color *= Temperature/1000.0;
    Out = color;
}

void Blackbody_Hdr_half(half Temperature, out half3 Out)
{
    half tempPow = pow(Temperature,(-3.0 / 2.0));
    half tempLog = log(max(Temperature,0));
    half3 color;
    color.x = 220000 * tempPow + 0.580392;

    if (Temperature > 6500.0) color.y = 138039.215686 * tempPow + 0.721569;
    else color.y = 0.392314 * tempLog - 2.44549;

    color.z = 0.76149 * tempLog - 5.680784;

    color = clamp(color, 0.0, 1);
    color *= Temperature/1000.0;
    Out = color;
}

#endif //BLACKBODY_HDR_INCLUDED
