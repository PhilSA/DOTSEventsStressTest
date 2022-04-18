using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDamagerManual : MonoBehaviour
{
    public TestHealthManual Target;
    public float Damage;

    public void ManualUpdate()
    {
        Target.ApplyDamage(Damage);
    }
}
