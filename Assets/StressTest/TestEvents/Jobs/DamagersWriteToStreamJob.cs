using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
public struct DamagersWriteToStreamJob : IJobEntityBatch
{
    [ReadOnly]
    public EntityTypeHandle EntityType;
    [ReadOnly]
    public ComponentTypeHandle<Damager> DamagerType;
    [NativeDisableParallelForRestriction]
    public NativeStream.Writer StreamDamageEvents;

    public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
    {
        NativeArray<Entity> chunkEntity = batchInChunk.GetNativeArray(EntityType);
        NativeArray<Damager> chunkDamager = batchInChunk.GetNativeArray(DamagerType);

        StreamDamageEvents.BeginForEachIndex(batchIndex);

        for (int i = 0; i < batchInChunk.Count; i++)
        {
            Entity entity = chunkEntity[i];
            Damager damager = chunkDamager[i];
            StreamDamageEvents.Write(new StreamDamageEvent { Target = damager.Target, DamageEvent = new DamageEvent { Source = entity, Value = damager.Damage } });
        }

        StreamDamageEvents.EndForEachIndex();
    }
}