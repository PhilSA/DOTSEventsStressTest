using System;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
public struct PollDamageEventBuffersJob : IJobChunk
{
    [ReadOnly]
    public EntityTypeHandle EntityType;
    public ComponentTypeHandle<Health> HealthType;
    [ReadOnly]
    public BufferTypeHandle<DamageEvent> DamageEventBufferType;

    public uint LastSystemVersion;

    public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, Boolean useEnabledMask, in v128 chunkEnabledMask)
    {
        if (chunk.DidChange(ref DamageEventBufferType, LastSystemVersion))
        {
            NativeArray<Entity> chunkEntity = chunk.GetNativeArray(EntityType);
            NativeArray<Health> chunkHealth = chunk.GetNativeArray(ref HealthType);
            BufferAccessor<DamageEvent> chunkDamageEventBuffer = chunk.GetBufferAccessor(ref DamageEventBufferType);

            for (int i = 0; i < chunk.Count; i++)
            {
                Entity entity = chunkEntity[i];
                Health health = chunkHealth[i];
                DynamicBuffer<DamageEvent> damageEventBuffer = chunkDamageEventBuffer[i];

                for (int d = 0; d < damageEventBuffer.Length; d++)
                {
                    health.Value -= damageEventBuffer[d].Value;
                }

                chunkHealth[i] = health;
            }
        }
    }
}
