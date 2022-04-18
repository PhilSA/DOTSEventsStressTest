using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public partial class SingleDirectModificationSystem : SystemBase
{
    protected override void OnUpdate()
    {
        if (!HasSingleton<EventStressTest>())
            return;

        if (GetSingleton<EventStressTest>().EventType != EventType.J_SingleDirectModification)
            return;

        Entities.ForEach((in Damager damager) => 
        {
            if (HasComponent<Health>(damager.Target))
            {
                Health health = GetComponent<Health>(damager.Target);
                health.Value -= damager.Damage;
                SetComponent(damager.Target, health);
            }
        }).Schedule();
    }
}
