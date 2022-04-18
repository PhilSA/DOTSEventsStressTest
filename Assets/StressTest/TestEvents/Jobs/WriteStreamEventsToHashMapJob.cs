using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;



[BurstCompile]
public struct EnsureHashMapCapacityJob : IJob
{
    public NativeStream.Reader StreamDamageEvents;
    public NativeMultiHashMap<Entity, DamageEvent> DamageEventsMap;

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

[BurstCompile]
public struct WriteStreamEventsToHashMapJob : IJobParallelFor
{
    public NativeStream.Reader StreamDamageEvents;
    public NativeMultiHashMap<Entity, DamageEvent>.ParallelWriter DamageEventsMap;

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