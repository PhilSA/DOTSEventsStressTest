using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public partial class ParallelWriteToBuffersECB_ParallelPollBuffers_System : SystemBase
{
    protected override void OnUpdate()
    {
        if (!HasSingleton<EventStressTest>())
            return;

        if (GetSingleton<EventStressTest>().EventType != EventType.ParallelWriteToBuffersECB_ParallelPollBuffers)
            return;

        EntityQuery healthsQuery = GetEntityQuery(typeof(Health), typeof(DamageEvent));
        EntityQuery damageBuffersQuery = GetEntityQuery(typeof(DamageEvent));

        EndSimulationEntityCommandBufferSystem ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        EntityCommandBuffer.ParallelWriter ecb = ecbSystem.CreateCommandBuffer().AsParallelWriter();

        Dependency = Entities.ForEach((Entity entity, int entityInQueryIndex, in Damager damager) =>
        {
            ecb.AppendToBuffer(entityInQueryIndex, damager.Target, new DamageEvent { Source = entity, Value = damager.Damage });
        }).ScheduleParallel(Dependency);

        Dependency = new PollDamageEventBuffersJob
        {
            EntityType = GetEntityTypeHandle(),
            HealthType = GetComponentTypeHandle<Health>(false),
            DamageEventBufferType = GetBufferTypeHandle<DamageEvent>(true),
            LastSystemVersion = this.LastSystemVersion,
        }.ScheduleParallel(healthsQuery, Dependency);

        Dependency = new ClearDamageEventBuffersJob
        {
            DamageEventBufferType = GetBufferTypeHandle<DamageEvent>(false),
            LastSystemVersion = this.LastSystemVersion,
        }.ScheduleParallel(damageBuffersQuery, Dependency);

        ecbSystem.AddJobHandleForProducer(Dependency);
    }
}
