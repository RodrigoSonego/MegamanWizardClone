using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    [SerializeField] private int damage;
    [SerializeField] private bool destroyOnCollision;

    public int Damage
    {
        get
        {
            return damage;
        }
    }

    public void CheckDestroyOnCollision()
    {
        if (!destroyOnCollision) { return; }
        else { Destroy(gameObject); }
    }
}
