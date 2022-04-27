using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
public struct PollDamageEventBuffersJob : IJobEntityBatch
{
    [ReadOnly]
    public EntityTypeHandle EntityType;
    public ComponentTypeHandle<Health> HealthType;
    [ReadOnly]
    public BufferTypeHandle<DamageEvent> DamageEventBufferType;

    public uint LastSystemVersion;

    public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
    {
        if (batchInChunk.DidChange(DamageEventBufferType, LastSystemVersion))
        {
            NativeArray<Entity> chunkEntity = batchInChunk.GetNativeArray(EntityType);
            NativeArray<Health> chunkHealth = batchInChunk.GetNativeArray(HealthType);
            BufferAccessor<DamageEvent> chunkDamageEventBuffer = batchInChunk.GetBufferAccessor(DamageEventBufferType);

            for (int i = 0; i < batchInChunk.Count; i++)
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