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
    float4x4 target = Camera.main.transform.localToWorldMatrix;

    Entities.ForEach((ref LocalToWorld transform, in FollowCamera f) =>
    {
      transform.Value = target;
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
    AddComponent(new FollowCamera { });
  }
}

