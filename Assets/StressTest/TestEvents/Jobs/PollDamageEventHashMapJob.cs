using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public struct SinglePollDamageEventHashMapJob : IJob
{
    [ReadOnly]
    public EntityTypeHandle EntityType;
    [ReadOnly]
    public NativeMultiHashMap<Entity, DamageEvent> DamageEventsMap;
    [NativeDisableParallelForRestriction]
    public ComponentDataFromEntity<Health> HealthFromEntity;

    public void Execute()
    {
        var enumerator = DamageEventsMap.GetEnumerator();
        while(enumerator.MoveNext())
        {
            Entity targetEntity = enumerator.Current.Key;
            DamageEvent damageEvent = enumerator.Current.Value;
            if (HealthFromEntity.HasComponent(targetEntity))
            {
                Health health = HealthFromEntity[targetEntity];
                health.Value -= damageEvent.Value;
                HealthFromEntity[targetEntity] = health;
            }
        }
    }
}


[BurstCompile]
public struct ParallelPollDamageEventHashMapJob : IJobParallelFor
{
    [ReadOnly]
    public NativeMultiHashMap<Entity, DamageEvent> DamageEventsMap;
    [NativeDisableParallelForRestriction]
    public ComponentDataFromEntity<Health> HealthFromEntity;

    public int ThreadCount;

    public unsafe void Execute(int index)
    {
        UnsafeHashMapBucketData bucketData = DamageEventsMap.GetUnsafeBucketData();
        int* buckets = (int*)bucketData.buckets;
        int* nextPtrs = (int*)bucketData.next;
        byte* keys = bucketData.keys;
        byte* values = bucketData.values;

        int elementsCountInMap = DamageEventsMap.Count();
        int startIndex = (index * ThreadCount);
        int countPerThread = (int)math.ceil((float)elementsCountInMap / (float)ThreadCount);

        for (int i = startIndex; i < math.min(elementsCountInMap, startIndex + countPerThread); i++)
        {
            int entryIndex = buckets[i];
            while (entryIndex != -1)
            {
                Entity targetEntity = UnsafeUtility.ReadArrayElement<Entity>(keys, entryIndex);
                DamageEvent damageEvent = UnsafeUtility.ReadArrayElement<DamageEvent>(values, entryIndex);

                if (HealthFromEntity.HasComponent(targetEntity))
                {
                    Health health = HealthFromEntity[targetEntity];
                    health.Value -= damageEvent.Value;
                    HealthFromEntity[targetEntity] = health;
                }

                entryIndex = nextPtrs[entryIndex];
            }
        }
    }
}

[BurstCompile]
public struct ClearDamageEventHashMapJob : IJob
{
    public NativeMultiHashMap<Entity, DamageEvent> DamageEventsMap;

    public void Execute()
    {
        DamageEventsMap.Clear();
    }
}