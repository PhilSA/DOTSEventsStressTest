using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public partial class E_ParallelWriteToStream_ParallelApplyToEntities_System : SystemBase
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

        if (GetSingleton<EventStressTest>().EventType != EventType.E_ParallelWriteToStream_ParallelApplyToEntities)
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

        Dependency = new ParallelApplyStreamEventsToEntitiesJob
        {
            StreamDamageEvents = PendingStream.AsReader(),
            StorageInfoFromEntity = GetStorageInfoFromEntity(),
            HealthType = GetComponentTypeHandle<Health>(false),
        }.Schedule(damagersQuery.CalculateChunkCount(), 1, Dependency);
    }
}
