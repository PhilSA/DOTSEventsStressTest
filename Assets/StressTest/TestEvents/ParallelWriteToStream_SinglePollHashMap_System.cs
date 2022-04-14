using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public partial class ParallelWriteToStream_SinglePollHashMap_System : SystemBase
{
    public NativeStream PendingStream;
    public NativeMultiHashMap<Entity, DamageEvent> DamageEventsMap;

    protected override void OnCreate()
    {
        base.OnCreate();
        DamageEventsMap = new NativeMultiHashMap<Entity, DamageEvent>(500000, Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (PendingStream.IsCreated)
        {
            PendingStream.Dispose();
        }
        if (DamageEventsMap.IsCreated)
        {
            DamageEventsMap.Dispose();
        }
    }

    protected override void OnUpdate()
    {
        if (!HasSingleton<EventStressTest>())
            return;

        if (GetSingleton<EventStressTest>().EventType != EventType.ParallelWriteToStream_SinglePollHashMap)
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

        Dependency = new WriteStreamEventsToHashMapJob
        {
            StreamDamageEvents = PendingStream.AsReader(),
            DamageEventsMap = DamageEventsMap,
        }.Schedule(Dependency);

        Dependency = new SinglePollDamageEventHashMapJob
        {
            EntityType = GetEntityTypeHandle(),
            DamageEventsMap = DamageEventsMap,
            HealthFromEntity = GetComponentDataFromEntity<Health>(false),
        }.Schedule(Dependency);

        Dependency = new ClearDamageEventHashMapJob
        {
            DamageEventsMap = DamageEventsMap,
        }.Schedule(Dependency);
    }
}
