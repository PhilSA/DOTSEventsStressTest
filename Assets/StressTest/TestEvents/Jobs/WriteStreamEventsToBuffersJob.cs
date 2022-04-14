using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;


[BurstCompile]
public struct WriteStreamEventsToBuffersJob : IJob
{
    public NativeStream.Reader StreamDamageEvents;
    public BufferFromEntity<DamageEvent> DamageEventBufferFromEntity;

    public void Execute()
    {
        for (int i = 0; i < StreamDamageEvents.ForEachCount; i++)
        {
            StreamDamageEvents.BeginForEachIndex(i);
            while (StreamDamageEvents.RemainingItemCount > 0)
            {
                StreamDamageEvent damageEvent = StreamDamageEvents.Read<StreamDamageEvent>();
                if (DamageEventBufferFromEntity.HasComponent(damageEvent.Target))
                {
                    DynamicBuffer<DamageEvent> damageEventBuffer = DamageEventBufferFromEntity[damageEvent.Target];
                    damageEventBuffer.Add(damageEvent.DamageEvent);
                }
            }
            StreamDamageEvents.EndForEachIndex();
        }
    }
}