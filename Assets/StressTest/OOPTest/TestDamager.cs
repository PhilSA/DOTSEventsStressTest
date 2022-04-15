using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDamager : MonoBehaviour
{
    public TestHealth Target;
    public float Damage;

    void Update()
    {
        Target.ApplyDamage(Damage);
    }
}
