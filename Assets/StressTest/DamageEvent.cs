using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct DamageEvent : IBufferElementData
{
    public Entity Source;
    public float Value;
}

public struct StreamDamageEvent : IBufferElementData
{
    public Entity Target;
    public DamageEvent DamageEvent;
}
