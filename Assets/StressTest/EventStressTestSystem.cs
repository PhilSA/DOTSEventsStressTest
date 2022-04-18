using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[AlwaysUpdateSystem]
[AlwaysSynchronizeSystem]
public partial class EventStressTestSystem : SystemBase
{
    protected override void OnUpdate()
    {
        EntityManager.CompleteAllJobs();

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        Entities
            .WithNone<IsInitialized>()
            .ForEach((Entity entity, ref EventStressTest spawner) =>
            {
                Random random = Random.CreateFromIndex(1);
                int spawnResolution = (int)math.ceil(math.sqrt(spawner.HealthEntityCount));

                int spawnCounter = 0;
                for (int x = 0; x < spawnResolution; x++)
                {
                    for (int y = 0; y < spawnResolution; y++)
                    {
                        Entity spawnedPrefab = ecb.Instantiate(spawner.HealthPrefab);
                        ecb.SetComponent(spawnedPrefab, new Translation { Value = new float3(x * spawner.Spacing, 0f, y * spawner.Spacing) });

                        for (int d = 0; d < spawner.DamagersPerHealths; d++)
                        {
                            Entity damagerEntity = ecb.CreateEntity();
                            ecb.AddComponent(damagerEntity, new Damager { Target = spawnedPrefab, Damage = 0.1f });
                        }

                        spawnCounter++;
                        if (spawnCounter >= spawner.HealthEntityCount)
                        {
                            break;
                        }
                    }

                    if (spawnCounter >= spawner.HealthEntityCount)
                    {
                        break;
                    }
                }

                ecb.AddComponent(entity, new IsInitialized());
            }).Run();

        ecb.Playback(EntityManager);
        ecb.Dispose();

    }
}
