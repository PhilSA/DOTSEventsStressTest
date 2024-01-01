using System;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
public struct ClearDamageEventBuffersJob : IJobChunk
{
    public BufferTypeHandle<DamageEvent> DamageEventBufferType;

    public uint LastSystemVersion;

    public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, Boolean useEnabledMask, in v128 chunkEnabledMask)
    {
        if (chunk.DidChange(ref DamageEventBufferType, LastSystemVersion))
        {
            BufferAccessor<DamageEvent> chunkDamageEventBuffer = chunk.GetBufferAccessor(ref DamageEventBufferType);

            for (int i = 0; i < chunk.Count; i++)
            {
                DynamicBuffer<DamageEvent> damageEventBuffer = chunkDamageEventBuffer[i]; 
                damageEventBuffer.Clear();
            }
        }
    }
}
