using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;


[BurstCompile]
public struct WriteStreamEventsToListJob : IJob
{
    public NativeStream.Reader StreamDamageEvents;
    public NativeList<StreamDamageEvent> DamageEventsList;

    public void Execute()
    {
        for (int i = 0; i < StreamDamageEvents.ForEachCount; i++)
        {
            StreamDamageEvents.BeginForEachIndex(i);
            while (StreamDamageEvents.RemainingItemCount > 0)
            {
                StreamDamageEvent damageEvent = StreamDamageEvents.Read<StreamDamageEvent>();
                DamageEventsList.Add(damageEvent);
            }
            StreamDamageEvents.EndForEachIndex();
        }
    }
}