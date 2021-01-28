using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    [SerializeField] private int damage;

    public int Damage
    {
        get
        {
            return damage;
        }
    }

}
