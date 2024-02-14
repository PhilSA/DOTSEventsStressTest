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

public partial class F_ParallelCreateEventEntities_SingleApplyToEntities_System : SystemBase
{
    protected override void OnUpdate()
    {
        if (!SystemAPI.HasSingleton<EventStressTest>())
            return;

        if (SystemAPI.GetSingleton<EventStressTest>().EventType != EventType.F_ParallelCreateEventEntities_SingleApplyToEntities)
            return;

        EntityArchetype eventArchetype = EntityManager.CreateArchetype(typeof(DamageEventComp));

        // FIXME: change this to NOT use the 'Managed' version of the method:
        EndSimulationEntityCommandBufferSystem ecbSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
        EntityCommandBuffer.ParallelWriter createEventsECB = ecbSystem.CreateCommandBuffer().AsParallelWriter();

        Dependency = Entities.ForEach((Entity entity, int entityInQueryIndex, in Damager damager) =>
        {
            Entity damageEventEntity = createEventsECB.CreateEntity(entityInQueryIndex, eventArchetype);
            createEventsECB.SetComponent(entityInQueryIndex, damageEventEntity, new DamageEventComp { Source = entity, Target = damager.Target, Damage = damager.Damage });
        }).ScheduleParallel(Dependency);

        Dependency = Entities.ForEach((Entity entity, in DamageEventComp damageEvent) =>
        {
            if(SystemAPI.HasComponent<Health>(damageEvent.Target))
            {
                Health health = SystemAPI.GetComponent<Health>(damageEvent.Target);
                health.Value -= damageEvent.Damage;
                SystemAPI.SetComponent(damageEvent.Target, health);
            }
        }).Schedule(Dependency);

        ecbSystem.AddJobHandleForProducer(Dependency);
    }
}

[UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
[UpdateAfter(typeof(EndSimulationEntityCommandBufferSystem))]
public partial struct DestroyEventEntitiesSystem : ISystem
{
    EntityQuery eventsQuery;
    public void OnCreate(ref SystemState state)
    {
        eventsQuery = state.GetEntityQuery(typeof(DamageEventComp));
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        state.EntityManager.DestroyEntity(eventsQuery);
    }
}
