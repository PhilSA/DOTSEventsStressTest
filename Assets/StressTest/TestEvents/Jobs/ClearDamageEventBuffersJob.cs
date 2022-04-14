using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public struct ClearDamageEventBuffersJob : IJobEntityBatch
{
    public BufferTypeHandle<DamageEvent> DamageEventBufferType;

    public uint LastSystemVersion;

    public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
    {
        if (batchInChunk.DidChange(DamageEventBufferType, LastSystemVersion))
        {
            BufferAccessor<DamageEvent> chunkDamageEventBuffer = batchInChunk.GetBufferAccessor(DamageEventBufferType);

            for (int i = 0; i < batchInChunk.Count; i++)
            {
                DynamicBuffer<DamageEvent> damageEventBuffer = chunkDamageEventBuffer[i]; 
                damageEventBuffer.Clear();
            }
        }
    }
}
