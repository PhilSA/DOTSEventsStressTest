using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public partial class ParallelWriteToStream_SingleApplyToEntities_System : SystemBase
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

        if (GetSingleton<EventStressTest>().EventType != EventType.ParallelWriteToStream_SingleApplyToEntities)
            return;

        EntityQuery damagersQuery = GetEntityQuery(typeof(Damager));

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

        Dependency = new SingleApplyStreamEventsToEntitiesJob
        {
            StreamDamageEvents = PendingStream.AsReader(),
            HealthFromEntity = GetComponentDataFromEntity<Health>(false),
        }.Schedule(Dependency);
    }
}
