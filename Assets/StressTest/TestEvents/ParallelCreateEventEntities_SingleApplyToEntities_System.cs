using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public struct DamageEventComp : IComponentData
{
    public Entity Source;
    public Entity Target;
    public float Damage;
}

public partial class ParallelCreateEventEntities_SingleApplyToEntities_System : SystemBase
{
    protected override void OnUpdate()
    {
        if (!HasSingleton<EventStressTest>())
            return;

        if (GetSingleton<EventStressTest>().EventType != EventType.ParallelCreateEventEntities_SingleApplyToEntities)
            return;

        EndSimulationEntityCommandBufferSystem ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        EntityCommandBuffer.ParallelWriter createEventsECB = ecbSystem.CreateCommandBuffer().AsParallelWriter();
        Dependency = Entities.ForEach((Entity entity, int entityInQueryIndex, in Damager damager) =>
        {
            Entity damageEventEntity = createEventsECB.CreateEntity(entityInQueryIndex);
            createEventsECB.AddComponent(entityInQueryIndex, damageEventEntity, new DamageEventComp { Source = entity, Target = damager.Target, Damage = damager.Damage });
        }).ScheduleParallel(Dependency);

        EntityCommandBuffer destroyEventsECB = ecbSystem.CreateCommandBuffer();
        Dependency = Entities.ForEach((Entity entity, in DamageEventComp damageEvent) =>
        {
            if(HasComponent<Health>(damageEvent.Target))
            {
                Health health = GetComponent<Health>(damageEvent.Target);
                health.Value -= damageEvent.Damage;
                SetComponent(damageEvent.Target, health);
            }
            destroyEventsECB.DestroyEntity(entity);
        }).Schedule(Dependency);

        ecbSystem.AddJobHandleForProducer(Dependency);
    }
}
