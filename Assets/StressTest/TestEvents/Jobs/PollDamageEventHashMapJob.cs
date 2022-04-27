using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
public struct SinglePollDamageEventHashMapJob : IJob
{
    [ReadOnly]
    public EntityTypeHandle EntityType;
    [ReadOnly]
    public NativeMultiHashMap<Entity, DamageEvent> DamageEventsMap;
    [NativeDisableParallelForRestriction]
    public ComponentDataFromEntity<Health> HealthFromEntity;

    public void Execute()
    {
        if (DamageEventsMap.Count() > 0)
        {
            var enumerator = DamageEventsMap.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Entity targetEntity = enumerator.Current.Key;
                DamageEvent damageEvent = enumerator.Current.Value;
                if (HealthFromEntity.HasComponent(targetEntity))
                {
                    Health health = HealthFromEntity[targetEntity];
                    health.Value -= damageEvent.Value;
                    HealthFromEntity[targetEntity] = health;
                }
            }
        }
    }
}


[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
public struct ParallelPollDamageEventHashMapJob : IJobNativeMultiHashMapVisitKeyValue<Entity, DamageEvent>
{
    [NativeDisableParallelForRestriction]
    public ComponentDataFromEntity<Health> HealthFromEntity;

    public void ExecuteNext(Entity targetEntity, DamageEvent damageEvent)
    {
        if (HealthFromEntity.HasComponent(targetEntity))
        {
            Health health = HealthFromEntity[targetEntity];
            health.Value -= damageEvent.Value;
            HealthFromEntity[targetEntity] = health;
        }
    }
}

[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
public struct ClearDamageEventHashMapJob : IJob
{
    public NativeMultiHashMap<Entity, DamageEvent> DamageEventsMap;

    public void Execute()
    {
        if (DamageEventsMap.Count() > 0)
        {
            DamageEventsMap.Clear();
        }
    }
}