using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.Profiling;

namespace Samples.Boids
{
    public struct BoidSchool : IComponentData
    {
        public Entity Prefab;
        public float InitialRadius;
        public int Count;
        public float BaseScale;
        public float ScaleVariation;
    }

    [RequireMatchingQueriesForUpdate]
    public partial class BoidSchoolSpawnSystem : SystemBase
    {
        public ComponentLookup<LocalToWorld> LocalToWorldFromEntity;

        [BurstCompile]
        struct SetBoidLocalToWorld : IJobParallelFor
        {
            [NativeDisableContainerSafetyRestriction]
            [NativeDisableParallelForRestriction]
            public ComponentLookup<LocalToWorld> LocalToWorldFromEntity;

            public NativeArray<Entity> Entities;
            public float3 Center;
            public float Radius;
            public float BaseScale;
            public float ScaleVariation;

            public void Execute(int i)
            {
                var entity = Entities[i];
                var random = new Random(((uint)(entity.Index + i + 1) * 0x9F6ABC1));
                var dir = math.normalizesafe(random.NextFloat3() - new float3(0.5f, 0.5f, 0.5f));
                var scale = BaseScale + random.NextFloat() * ScaleVariation;
                var pos = Center + (dir * Radius);
                var localToWorld = new LocalToWorld
                {
                    Value = float4x4.TRS(
                      pos,
                      quaternion.LookRotationSafe(dir, math.up()),
                      new float3(scale, scale, scale))
                };
                LocalToWorldFromEntity[entity] = localToWorld;
            }
        }

        protected override void OnCreate()
        {
             LocalToWorldFromEntity = GetComponentLookup<LocalToWorld>();
        }

        protected override void OnUpdate()
        {
            Entities.WithStructuralChanges().ForEach((Entity entity, int entityInQueryIndex, in BoidSchool boidSchool, in LocalToWorld boidSchoolLocalToWorld) =>
            {
                var world = World.Unmanaged;
                var boidEntities = CollectionHelper.CreateNativeArray<Entity, RewindableAllocator>(boidSchool.Count, ref world.UpdateAllocator);

                Profiler.BeginSample("Instantiate");
                EntityManager.Instantiate(boidSchool.Prefab, boidEntities);
                Profiler.EndSample();

                LocalToWorldFromEntity.Update(this);
                var setBoidLocalToWorldJob = new SetBoidLocalToWorld
                {
                    LocalToWorldFromEntity = LocalToWorldFromEntity,
                    Entities = boidEntities,
                    Center = boidSchoolLocalToWorld.Position,
                    Radius = boidSchool.InitialRadius,
                    BaseScale = boidSchool.BaseScale,
                    ScaleVariation = boidSchool.ScaleVariation,
                };
                Dependency = setBoidLocalToWorldJob.Schedule(boidSchool.Count, 64, Dependency);

                EntityManager.DestroyEntity(entity);
            }).Run();
        }
    }
}
