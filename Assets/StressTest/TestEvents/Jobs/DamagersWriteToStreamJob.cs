using System;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
public struct DamagersWriteToStreamJob : IJobChunk
{
    [ReadOnly]
    public EntityTypeHandle EntityType;
    [ReadOnly]
    public ComponentTypeHandle<Damager> DamagerType;
    [NativeDisableParallelForRestriction]
    public NativeStream.Writer StreamDamageEvents;

    public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, Boolean useEnabledMask, in v128 chunkEnabledMask)
    {
        NativeArray<Entity> chunkEntity = chunk.GetNativeArray(EntityType);
        NativeArray<Damager> chunkDamager = chunk.GetNativeArray(ref DamagerType);

        StreamDamageEvents.BeginForEachIndex(unfilteredChunkIndex);

        for (int i = 0; i < chunk.Count; i++)
        {
            Entity entity = chunkEntity[i];
            Damager damager = chunkDamager[i];
            StreamDamageEvents.Write(new StreamDamageEvent { Target = damager.Target, DamageEvent = new DamageEvent { Source = entity, Value = damager.Damage } });
        }

        StreamDamageEvents.EndForEachIndex();
    }
}
