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
        if (!SystemAPI.HasSingleton<EventStressTest>())
            return;

        if (SystemAPI.GetSingleton<EventStressTest>().EventType != EventType.J_SingleDirectModification)
            return;

        Entities.ForEach((in Damager damager) => 
        {
            if (SystemAPI.HasComponent<Health>(damager.Target))
            {
                Health health = SystemAPI.GetComponent<Health>(damager.Target);
                health.Value -= damager.Damage;
                SystemAPI.SetComponent(damager.Target, health);
            }
        }).Schedule();
    }
}
