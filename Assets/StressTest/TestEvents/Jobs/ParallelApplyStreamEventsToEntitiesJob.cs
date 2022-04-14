using System;
using System.Runtime.InteropServices;
using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public unsafe struct ParallelApplyStreamEventsToEntitiesJob : IJobParallelFor
{
    public NativeStream.Reader StreamDamageEvents;
    public StorageInfoFromEntity StorageInfoFromEntity;
    public ComponentTypeHandle<Health> HealthType;

    public unsafe void Execute(int index)
    {
        StreamDamageEvents.BeginForEachIndex(index);
        while (StreamDamageEvents.RemainingItemCount > 0)
        {
            StreamDamageEvent damageEvent = StreamDamageEvents.Read<StreamDamageEvent>();
            if (StorageInfoFromEntity.Exists(damageEvent.Target))
            {
                EntityStorageInfo storageInfo = StorageInfoFromEntity[damageEvent.Target];
                ArchetypeChunk chunk = storageInfo.Chunk;
                if (chunk.Has(HealthType))
                {
                    NativeArray<Health> chunkHealth = chunk.GetNativeArray(HealthType);
                    int* healthIntPtr = (int*)chunkHealth.GetUnsafePtr();
                    healthIntPtr += storageInfo.IndexInChunk * UnsafeUtility.SizeOf<Health>();
                    Interlocked.Add(ref UnsafeUtility.AsRef<int>(healthIntPtr), -1);
                }
            }
        }
        StreamDamageEvents.EndForEachIndex();
    }
}