using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;



[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
public struct EnsureHashMapCapacityJob : IJob
{
    public NativeStream.Reader StreamDamageEvents;
    public NativeParallelMultiHashMap<Entity, DamageEvent> DamageEventsMap;

    public void Execute()
    {
        int totalCount = 0;
        for (int i = 0; i < StreamDamageEvents.ForEachCount; i++)
        {
            StreamDamageEvents.BeginForEachIndex(i);
            totalCount += StreamDamageEvents.RemainingItemCount;
        }

        if (totalCount > DamageEventsMap.Capacity)
        {
            DamageEventsMap.Capacity = totalCount;
        }
    }
}

[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
public struct SingleWriteStreamEventsToHashMapJob : IJob
{
    public NativeStream.Reader StreamDamageEvents;
    public NativeParallelMultiHashMap<Entity, DamageEvent> DamageEventsMap;

    public void Execute()
    {
        for (int i = 0; i < StreamDamageEvents.ForEachCount; i++)
        {
            StreamDamageEvents.BeginForEachIndex(i);
            while (StreamDamageEvents.RemainingItemCount > 0)
            {
                StreamDamageEvent damageEvent = StreamDamageEvents.Read<StreamDamageEvent>();
                DamageEventsMap.Add(damageEvent.Target, damageEvent.DamageEvent);
            }
            StreamDamageEvents.EndForEachIndex();
        }
    }
}

[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
public struct ParallelWriteStreamEventsToHashMapJob : IJobParallelFor
{
    public NativeStream.Reader StreamDamageEvents;
    public NativeParallelMultiHashMap<Entity, DamageEvent>.ParallelWriter DamageEventsMap;

    public void Execute(int index)
    {
        StreamDamageEvents.BeginForEachIndex(index);
        while (StreamDamageEvents.RemainingItemCount > 0)
        {
            StreamDamageEvent damageEvent = StreamDamageEvents.Read<StreamDamageEvent>();
            DamageEventsMap.Add(damageEvent.Target, damageEvent.DamageEvent);
        }
        StreamDamageEvents.EndForEachIndex();
    }
}
