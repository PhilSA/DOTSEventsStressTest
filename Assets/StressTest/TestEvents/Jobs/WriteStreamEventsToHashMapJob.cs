using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;


[BurstCompile]
public struct WriteStreamEventsToHashMapJob : IJob
{
    public NativeStream.Reader StreamDamageEvents;
    public NativeMultiHashMap<Entity, DamageEvent> DamageEventsMap;

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