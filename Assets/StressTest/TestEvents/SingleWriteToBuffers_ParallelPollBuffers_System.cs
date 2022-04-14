using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public partial class SingleWriteToBuffers_ParallelPollBuffers_System : SystemBase
{
    protected override void OnUpdate()
    {
        if (!HasSingleton<EventStressTest>())
            return;

        if (GetSingleton<EventStressTest>().EventType != EventType.SingleWriteToBuffers_ParallelPollBuffers)
            return;

        EntityQuery healthsQuery = GetEntityQuery(typeof(Health), typeof(DamageEvent));
        EntityQuery damageBuffersQuery = GetEntityQuery(typeof(DamageEvent));

        BufferFromEntity<DamageEvent> damageEventFromEntity = GetBufferFromEntity<DamageEvent>();

        Dependency = Entities.ForEach((Entity entity, in Damager damager) =>
        {
            if(damageEventFromEntity.HasComponent(damager.Target))
            {
                DynamicBuffer<DamageEvent> damageEventBuffer = damageEventFromEntity[damager.Target];
                damageEventBuffer.Add(new DamageEvent { Source = entity, Value = damager.Damage });
            }
        }).Schedule(Dependency);

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
    }
}
