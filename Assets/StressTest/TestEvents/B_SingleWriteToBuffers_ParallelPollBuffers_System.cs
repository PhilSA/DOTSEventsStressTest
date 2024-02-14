using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public partial class B_SingleWriteToBuffers_ParallelPollBuffers_System : SystemBase
{
    protected override void OnUpdate()
    {
        if (!SystemAPI.HasSingleton<EventStressTest>())
            return;

        if (SystemAPI.GetSingleton<EventStressTest>().EventType != EventType.B_SingleWriteToBuffers_ParallelPollBuffers)
            return;

        EntityQuery healthsQuery = GetEntityQuery(typeof(Health), typeof(DamageEvent));
        EntityQuery damageBuffersQuery = GetEntityQuery(typeof(DamageEvent));

        BufferLookup<DamageEvent> damageEventFromEntity = GetBufferLookup<DamageEvent>();

        Dependency = Entities.ForEach((Entity entity, in Damager damager) =>
        {
            if(damageEventFromEntity.HasBuffer(damager.Target))
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
