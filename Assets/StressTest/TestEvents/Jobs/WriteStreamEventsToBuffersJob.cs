using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;


[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
public struct WriteStreamEventsToBuffersJob : IJob
{
    public NativeStream.Reader StreamDamageEvents;
    public BufferLookup<DamageEvent> DamageEventBufferFromEntity;

    public void Execute()
    {
        for (int i = 0; i < StreamDamageEvents.ForEachCount; i++)
        {
            StreamDamageEvents.BeginForEachIndex(i);
            while (StreamDamageEvents.RemainingItemCount > 0)
            {
                StreamDamageEvent damageEvent = StreamDamageEvents.Read<StreamDamageEvent>();
                if (DamageEventBufferFromEntity.HasBuffer(damageEvent.Target))
                {
                    DynamicBuffer<DamageEvent> damageEventBuffer = DamageEventBufferFromEntity[damageEvent.Target];
                    damageEventBuffer.Add(damageEvent.DamageEvent);
                }
            }
            StreamDamageEvents.EndForEachIndex();
        }
    }
}
