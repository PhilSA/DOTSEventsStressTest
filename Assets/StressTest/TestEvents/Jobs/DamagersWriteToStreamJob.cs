// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
public struct DamagersWriteToStreamJob : IJobChunk
{
	[ReadOnly] public EntityTypeHandle EntityType;
	[ReadOnly] public ComponentTypeHandle<Damager> DamagerType;
	[NativeDisableParallelForRestriction] public NativeStream.Writer StreamDamageEvents;

	public void Execute(in ArchetypeChunk chunk, Int32 unfilteredChunkIndex, Boolean useEnabledMask,
		in v128 chunkEnabledMask)
	{
		var chunkEntity = chunk.GetNativeArray(EntityType);
		var chunkDamager = chunk.GetNativeArray(ref DamagerType);

		StreamDamageEvents.BeginForEachIndex(unfilteredChunkIndex);

		var chunkCount = chunk.Count;
		for (var i = 0; i < chunkCount; i++)
			StreamDamageEvents.Write(new StreamDamageEvent
			{
				Target = chunkDamager[i].Target,
				DamageEvent = new DamageEvent
				{
					Source = chunkEntity[i],
					Value = chunkDamager[i].Damage,
				},
			});

		StreamDamageEvents.EndForEachIndex();
	}
}
