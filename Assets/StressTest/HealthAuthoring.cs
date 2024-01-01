// Copyright (C) 2021-2023 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using System;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class HealthAuthoring : MonoBehaviour
{
	public Single HealthValue = 10f;
}

public class HealthBaker : Baker<HealthAuthoring>
{
	public override void Bake(HealthAuthoring authoring)
	{
		AddComponent(GetEntity(TransformUsageFlags.None), new Health { Value = authoring.HealthValue });
		AddBuffer<DamageEvent>(GetEntity(TransformUsageFlags.None));
	}
}
