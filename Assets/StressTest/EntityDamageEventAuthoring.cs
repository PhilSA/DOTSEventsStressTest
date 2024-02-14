// Copyright (C) 2021-2023 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class EntityDamageEventAuthoring : MonoBehaviour {}

public class EntityDamageBaker : Baker<EntityDamageEventAuthoring>
{
	public override void Bake(EntityDamageEventAuthoring authoring) => AddComponent(GetEntity(TransformUsageFlags.None), new DamageEventComp());
}
