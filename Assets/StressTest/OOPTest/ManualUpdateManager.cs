using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualUpdateManager : MonoBehaviour
{
    public List<TestDamagerManual> ManualDamagers = new List<TestDamagerManual>();

    public void Update()
    {
        for (int i = 0; i < ManualDamagers.Count; i++)
        {
            ManualDamagers[i].ManualUpdate();
        }
    }
}
