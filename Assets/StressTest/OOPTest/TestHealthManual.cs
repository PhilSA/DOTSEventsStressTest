using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestHealthManual : MonoBehaviour
{
    public float Health;

    public void ApplyDamage(float dmg)
    {
        Health -= dmg;
    }
}
