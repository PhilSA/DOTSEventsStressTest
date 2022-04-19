using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public partial class A_ParallelWriteToStream_ParallelPollBuffers_System : SystemBase
{
    public NativeStream PendingStream;

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (PendingStream.IsCreated)
        {
            PendingStream.Dispose();
        }
    }

    protected override void OnUpdate()
    {
        if (!HasSingleton<EventStressTest>())
            return;

        if (GetSingleton<EventStressTest>().EventType != EventType.A_ParallelWriteToStream_ParallelPollBuffers)
            return;

        EntityQuery damagersQuery = GetEntityQuery(typeof(Damager));
        EntityQuery healthsQuery = GetEntityQuery(typeof(Health), typeof(DamageEvent));
        EntityQuery damageBuffersQuery = GetEntityQuery(typeof(DamageEvent));

        if (PendingStream.IsCreated)
        {
            PendingStream.Dispose();
        }
        PendingStream = new NativeStream(damagersQuery.CalculateChunkCount(), Allocator.TempJob);

        Dependency = new DamagersWriteToStreamJob
        {
            EntityType = GetEntityTypeHandle(),
            DamagerType = GetComponentTypeHandle<Damager>(true),
            StreamDamageEvents = PendingStream.AsWriter(),
        }.ScheduleParallel(damagersQuery, Dependency);

        Dependency = new WriteStreamEventsToBuffersJob
        {
            StreamDamageEvents = PendingStream.AsReader(),
            DamageEventBufferFromEntity = GetBufferFromEntity<DamageEvent>(false),
        }.Schedule(Dependency);

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
