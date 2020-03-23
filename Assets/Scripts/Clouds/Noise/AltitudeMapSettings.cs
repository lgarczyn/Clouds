using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class AltitudeMapSettings : ScriptableObject {

    [Range(0.5f,1.5f)]
    public float highestLayerAltitude = 0.9f;
    [Range(-0.5f, 0.5f)]
    public float lowestLayerAltitude = 0.1f;

    [Range(-10f, 10f)]
    public float linearOffsetStart = 0.2f;
    [Range(-10f, 10f)]
    public float linearOffsetFactor = -1f;

    [Range(0f, 10f)]
    public float powerOffsetPower = 0.5f;
    [Range(0f, 1f)]
    public float powerOffsetRatio = 0.75f;

    [Range(0f, 10f)]
    public float powerOffsetPowerTop = 0.5f;
    [Range(0f, 10f)]
    public float powerOffsetRatioTop = 0;

    [Range(0f, 10f)]
    public float initialPowerFactor = 2f;
    [Range(0f, 1f)]
    public float iterativePowerFactor = 0.8f;
    [Range(0, 10)]
    public int iterativePowerCount = 3;

}