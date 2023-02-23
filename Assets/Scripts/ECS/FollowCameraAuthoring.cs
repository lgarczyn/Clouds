using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

public struct FollowCamera : IComponentData
{
}

[RequireMatchingQueriesForUpdate]
public partial class FollowCameraSystem : SystemBase
{
  private EntityQuery followerQuery;
  override protected void OnUpdate()
  {
    float3 pos = Camera.main.transform.position;

    Entities.ForEach((ref LocalToWorld transform, in FollowCamera f) =>
    {
      transform.Value = float4x4.Translate(pos);
    }).ScheduleParallel();
  }
}
public class FollowCameraAuthoring : MonoBehaviour
{
}

public class FollowCameraAuthoringBaker : Baker<FollowCameraAuthoring>
{
  public override void Bake(FollowCameraAuthoring authoring)
  {
    AddTransformUsageFlags(TransformUsageFlags.ManualOverride);
    AddComponent(new FollowCamera { });
    AddComponent(new LocalToWorld());
  }
}

