using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public partial class EventStressTestSystem : SystemBase
{
    protected override void OnUpdate()
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        Entities
            .WithNone<IsInitialized>()
            .ForEach((Entity entity, ref EventStressTest spawner) =>
            {
                Random random = Random.CreateFromIndex(1);
                int spawnResolution = (int)math.ceil(math.sqrt(spawner.EntityCount));

                int spawnCounter = 0;
                for (int x = 0; x < spawnResolution; x++)
                {
                    for (int y = 0; y < spawnResolution; y++)
                    {
                        Entity spawnedPrefab = ecb.Instantiate(spawner.Prefab);
                        ecb.SetComponent(spawnedPrefab, new Translation { Value = new float3(x * spawner.Spacing, 0f, y * spawner.Spacing) });

                        for (int d = 0; d < spawner.DamagersPerHealths; d++)
                        {
                            Entity damagerEntity = ecb.CreateEntity();
                            ecb.AddComponent(damagerEntity, new Damager { Target = spawnedPrefab, Damage = 0.1f });
                        }

                        spawnCounter++;
                        if (spawnCounter >= spawner.EntityCount)
                        {
                            break;
                        }
                    }

                    if (spawnCounter >= spawner.EntityCount)
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
