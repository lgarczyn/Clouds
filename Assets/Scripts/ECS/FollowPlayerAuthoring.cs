using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

[System.Serializable]
public struct FollowPlayer : IComponentData
{
  public float timeExtrapolation;
}

[RequireMatchingQueriesForUpdate]
public partial class FollowPlayerSystem : SystemBase
{
  protected override void OnUpdate()
  {
    float3 targetPos = PlayerManager.instance.playerTransform.position;
    float3 velocity = PlayerManager.instance.playerRigidbody.velocity;

    Entities.ForEach((ref LocalToWorld pos, in FollowPlayer f) =>
    {
      pos.Value = float4x4.Translate(targetPos + velocity * f.timeExtrapolation);
    }).ScheduleParallel();
  }
}

public class FollowPlayerAuthoring : MonoBehaviour
{
  public float timeExtrapolation;
}

public class FollowPlayerAuthoringBaker : Baker<FollowPlayerAuthoring>
{
  public override void Bake(FollowPlayerAuthoring authoring)
  {
    AddTransformUsageFlags(TransformUsageFlags.ManualOverride);
    AddComponent(new FollowPlayer
    {
      timeExtrapolation = authoring.timeExtrapolation
    });
    AddComponent(new LocalToWorld());
  }
}

