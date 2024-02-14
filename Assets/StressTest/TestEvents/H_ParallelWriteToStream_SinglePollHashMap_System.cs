using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public partial class H_ParallelWriteToStream_SinglePollHashMap_System : SystemBase
{
    public NativeStream PendingStream;
    public NativeParallelMultiHashMap<Entity, DamageEvent> DamageEventsMap;

    protected override void OnCreate()
    {
        base.OnCreate();
        DamageEventsMap = new NativeParallelMultiHashMap<Entity, DamageEvent>(500000, Allocator.Persistent);
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
        if (!SystemAPI.HasSingleton<EventStressTest>())
            return;

        if (SystemAPI.GetSingleton<EventStressTest>().EventType != EventType.H_ParallelWriteToStream_SinglePollHashMap)
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

        Dependency = new SingleWriteStreamEventsToHashMapJob
        {
            StreamDamageEvents = PendingStream.AsReader(),
            DamageEventsMap = DamageEventsMap,
        }.Schedule(Dependency);

        //Dependency = new EnsureHashMapCapacityJob
        //{
        //    StreamDamageEvents = PendingStream.AsReader(),
        //    DamageEventsMap = DamageEventsMap,
        //}.Schedule(Dependency);

        //Dependency = new ParallelWriteStreamEventsToHashMapJob
        //{
        //    StreamDamageEvents = PendingStream.AsReader(),
        //    DamageEventsMap = DamageEventsMap.AsParallelWriter(),
        //}.Schedule(damagersQuery.CalculateChunkCount(), 1, Dependency);

        Dependency = new SinglePollDamageEventHashMapJob
        {
            EntityType = GetEntityTypeHandle(),
            DamageEventsMap = DamageEventsMap,
            HealthFromEntity = GetComponentLookup<Health>(false),
        }.Schedule(Dependency);

        Dependency = new ClearDamageEventHashMapJob
        {
            DamageEventsMap = DamageEventsMap,
        }.Schedule(Dependency);
    }
}
