#if UNITY_EDITOR

using System;
using Samples.Boids;
using Unity.Entities;
using UnityEngine;

public class BoidTargetAuthoring : MonoBehaviour {}

public class BoidTargetAuthoringBaker : Baker<BoidTargetAuthoring>
{
    public override void Bake(BoidTargetAuthoring authoring)
    {
        AddComponent( new BoidTarget());
    }
}

#endif
