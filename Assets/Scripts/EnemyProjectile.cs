using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private int damage;

    void DealDamage(Player player)
    {
        player.TakeDamage(damage);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) { return; }    
        else
        {
            DealDamage(collision.GetComponent<Player>());
        }
    }
}
