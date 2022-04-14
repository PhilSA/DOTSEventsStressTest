using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public partial class ParallelWriteToStream_SinglelPollList_System : SystemBase
{
    public NativeStream PendingStream;
    public NativeList<StreamDamageEvent> DamageEventsList;

    protected override void OnCreate()
    {
        base.OnCreate();
        DamageEventsList = new NativeList<StreamDamageEvent>(500000, Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (PendingStream.IsCreated)
        {
            PendingStream.Dispose();
        }
        if (DamageEventsList.IsCreated)
        {
            DamageEventsList.Dispose();
        }
    }

    protected override void OnUpdate()
    {
        if (!HasSingleton<EventStressTest>())
            return;

        if (GetSingleton<EventStressTest>().EventType != EventType.ParallelWriteToStream_SinglePollList)
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

        Dependency = new WriteStreamEventsToListJob
        {
            StreamDamageEvents = PendingStream.AsReader(),
            DamageEventsList = DamageEventsList,
        }.Schedule(Dependency);

        Dependency = new SinglePollDamageEventListJob
        {
            EntityType = GetEntityTypeHandle(),
            DamageEventsList = DamageEventsList,
            HealthFromEntity = GetComponentDataFromEntity<Health>(false),
        }.Schedule(Dependency);

        Dependency = new ClearDamageEventListJob
        {
            DamageEventsList = DamageEventsList,
        }.Schedule(Dependency);
    }
}

