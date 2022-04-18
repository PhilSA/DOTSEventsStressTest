using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class OOPTestSetup : MonoBehaviour
{
    public bool UseMonobehaviourUpdate = true;

    public GameObject HealthPrefabRegular;
    public GameObject DamagerPrefabRegular;
    public GameObject HealthPrefabManual;
    public GameObject DamagerPrefabManual;

    public int HealthEntityCount = 500000;
    public float Spacing = 1f;
    public int DamagersPerHealths = 1;

    private void Start()
    {
        int spawnResolution = (int)math.ceil(math.sqrt(HealthEntityCount));

        GameObject usedHealthPrefab = null;
        GameObject usedDamagerPrefab = null;
        if(UseMonobehaviourUpdate)
        {
            usedHealthPrefab = HealthPrefabRegular;
            usedDamagerPrefab = DamagerPrefabRegular;
        }
        else
        {
            usedHealthPrefab = HealthPrefabManual;
            usedDamagerPrefab = DamagerPrefabManual;
        }


        GameObject updateManagerGO = new GameObject("UpdateManager");
        ManualUpdateManager updateManager = updateManagerGO.AddComponent<ManualUpdateManager>();

        int spawnCounter = 0;
        for (int x = 0; x < spawnResolution; x++)
        {
            for (int y = 0; y < spawnResolution; y++)
            {
                GameObject spawnedHealth = Instantiate(usedHealthPrefab);
                spawnedHealth.transform.position = new float3(x * Spacing, 0f, y * Spacing);

                for (int d = 0; d < DamagersPerHealths; d++)
                {
                    GameObject spawnedDamager = Instantiate(usedDamagerPrefab);
                    if(UseMonobehaviourUpdate)
                    {
                        spawnedDamager.GetComponent<TestDamager>().Target = spawnedHealth.GetComponent<TestHealth>();
                    }
                    else
                    {
                        spawnedDamager.GetComponent<TestDamagerManual>().Target = spawnedHealth.GetComponent<TestHealthManual>();
                        updateManager.ManualDamagers.Add(spawnedDamager.GetComponent<TestDamagerManual>());
                    }
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
