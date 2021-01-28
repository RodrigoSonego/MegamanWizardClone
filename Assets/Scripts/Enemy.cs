﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class Enemy : MonoBehaviour
{
    [SerializeField] private int health;
    [SerializeField] private int damage;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    public void TakeDamage(int damage)
    {
        health -= damage;
        print("fantasminha apanhou");
        StartCoroutine(DamageBlink());

        if(health <= 0) { Die(); }
    }

    IEnumerator DamageBlink()
    {
        animator.SetBool("hit", true);
        yield return new WaitForSeconds(0.2f);
        animator.SetBool("hit", false);
    }

    public void DealDamage(Player player)
    {
        player.TakeDamage(damage);      
    }

    void Die()
    {
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        bool collisionWithPlayer = collision.CompareTag("Player");
        bool collisionWithBullet = collision.CompareTag("Projectile");

        if (!collisionWithPlayer && !collisionWithBullet) { return; }
        else if (collisionWithPlayer)
        {
            DealDamage(collision.GetComponent<Player>());
        } 
        else if (collisionWithBullet)
        {
            TakeDamage(collision.GetComponent<PlayerProjectile>().Damage);
        }
    }

}
