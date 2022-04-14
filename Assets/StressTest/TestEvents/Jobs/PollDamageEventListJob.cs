using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public struct SinglePollDamageEventListJob : IJob
{
    [ReadOnly]
    public EntityTypeHandle EntityType;
    [ReadOnly]
    public NativeList<StreamDamageEvent> DamageEventsList;
    public ComponentDataFromEntity<Health> HealthFromEntity;

    public void Execute()
    {
        for (int i = 0; i < DamageEventsList.Length; i++)
        {
            StreamDamageEvent sde = DamageEventsList[i];
            if (HealthFromEntity.HasComponent(sde.Target))
            {
                Health health = HealthFromEntity[sde.Target];
                health.Value -= sde.DamageEvent.Value;
                HealthFromEntity[sde.Target] = health;
            }
        }
    }
}

[BurstCompile]
public struct ClearDamageEventListJob : IJob
{
    public NativeList<StreamDamageEvent> DamageEventsList;

    public void Execute()
    {
        DamageEventsList.Clear();
    }
}