#if UNITY_EDITOR

using System;
using Samples.Boids;
using Unity.Entities;
using UnityEngine;

public class BoidObstacleAuthoring : MonoBehaviour {}

public class BoidObstacleAuthoringBaker : Baker<BoidObstacleAuthoring>
{
    public override void Bake(BoidObstacleAuthoring authoring)
    {
        AddComponent(new BoidObstacle());
    }
}

#endif
