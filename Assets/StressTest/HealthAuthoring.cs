using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
public class HealthAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public float HealthValue = 10f;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new Health { Value = HealthValue });
        dstManager.AddBuffer<DamageEvent>(entity);
    }
}
