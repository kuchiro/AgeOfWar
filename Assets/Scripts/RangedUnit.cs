using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class RangedUnit : Unit
{
    [Header("Ranged Attack")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    // cadence de tir de l'unité à distance
    public float fireRate = 1f;

    // portée de l'attaque
    public float attackRange = 5f;

    private float fireTimer = 0f;

    protected override void Start()
    {
        base.Start();
    }

    private void Update()
    {
        fireTimer += Time.deltaTime;

        // chercher la cible ennemie la plus proche (unité adverse)
        Unit target = FindNearestEnemy();

        // S'il y a une unité ennemie proche -> attaquer
        if (target != null )
        {
            float distance = Vector2.Distance(transform.position, target.transform.position);
            if (distance > attackRange)
                MoveTowardsBase();
            else if (fireTimer >= 1f / fireRate)
            {
                FireProjectile(target);
                fireTimer = 0f;
            }
        }
        else
        {
            // Si aucun ennemi proche, on avance vers la base ennemie
            MoveTowardsBase();
        }
    }

    // gestion du projectile de l'unité qui tire
    private void FireProjectile(Unit target)
    {
        if (projectilePrefab == null) return;

        Vector3 dir = (target.transform.position - transform.position).normalized;
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + dir * 0.5f;

        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        proj.transform.right = dir;

        Projectile p = proj.GetComponent<Projectile>();
        if (p != null)
        {
            p.damage = attackDamage;
            p.team = unitTeam;
            p.shooter = this;
        }
    }

    // on cherche la cible ennemie la plus proche
    private Unit FindNearestEnemy()
    {
        Unit[] allUnits = FindObjectsOfType<Unit>();
        Unit nearest = null;
        float minDist = Mathf.Infinity;

        foreach (Unit u in allUnits)
        {
            if (u == this || u.unitTeam == this.unitTeam) continue;

            float dist = Vector2.Distance(transform.position, u.transform.position);
            if (dist < minDist && dist <= attackRange)
            {
                minDist = dist;
                nearest = u;
            }
        }

        return nearest;
    }
}
