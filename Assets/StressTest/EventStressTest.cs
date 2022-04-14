using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public enum EventType
{
    ParallelWriteToStream_ParallelPollBuffers,
    SingleWriteToBuffers_ParallelPollBuffers,
    ParallelWriteToBuffersECB_ParallelPollBuffers,

    ParallelWriteToStream_SingleApplyToEntities,
    ParallelWriteToStream_ParallelApplyToEntities,

    ParallelCreateEventEntities_SingleApplyToEntities,

    ParallelWriteToStream_SinglePollList,
    ParallelWriteToStream_SinglePollHashMap,
}

[Serializable]
[GenerateAuthoringComponent]
public struct EventStressTest : IComponentData
{
    public EventType EventType;
    public Entity Prefab;
    public int EntityCount;
    public float Spacing;

    public int DamagersPerHealths;
}


[Serializable]
public struct IsInitialized : IComponentData
{
}
