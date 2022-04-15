using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class OOPTestSetup : MonoBehaviour
{
    public GameObject HealthPrefab;
    public GameObject DamagerPrefab;

    public int HealthEntityCount = 500000;
    public float Spacing = 1f;
    public int DamagersPerHealths = 1;

    private void Start()
    {
        int spawnResolution = (int)math.ceil(math.sqrt(HealthEntityCount));

        int spawnCounter = 0;
        for (int x = 0; x < spawnResolution; x++)
        {
            for (int y = 0; y < spawnResolution; y++)
            {
                GameObject spawnedHealth = Instantiate(HealthPrefab);
                spawnedHealth.transform.position = new float3(x * Spacing, 0f, y * Spacing);

                for (int d = 0; d < DamagersPerHealths; d++)
                {
                    GameObject spawnedDamager = Instantiate(DamagerPrefab);
                    spawnedDamager.GetComponent<TestDamager>().Target = spawnedHealth.GetComponent<TestHealth>();
                }

                spawnCounter++;
                if (spawnCounter >= HealthEntityCount)
                {
                    break;
                }
            }

            if (spawnCounter >= HealthEntityCount)
            {
                break;
            }
        }
    }
}
