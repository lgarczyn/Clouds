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
  override protected void OnUpdate()
  {
    float4x4 target = GameObject.FindGameObjectWithTag("Player").transform.localToWorldMatrix;
    float3 velocity = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>().velocity;

    Entities.ForEach((ref LocalToWorld transform, in FollowPlayer f) =>
    {
      transform.Value = target;
      transform.Value.c3 += new float4(velocity * f.timeExtrapolation, 0f);
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
    AddComponent(new FollowPlayer
    {
      timeExtrapolation = authoring.timeExtrapolation
    });
  }
}

