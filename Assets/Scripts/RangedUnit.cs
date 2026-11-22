using UnityEngine;

/// <summary>
/// Unité à distance : s'arrête dès qu'un ennemi (ou la base adverse) est à portée et tire,
/// sinon avance en restant dans la file derrière ses alliés.
/// </summary>
public class RangedUnit : Unit
{
    [Header("Ranged Attack")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    // cadence de tir de l'unité à distance (tirs par seconde)
    public float fireRate = 1f;

    // portée de l'attaque
    public float attackRange = 5f;

    protected override void Think()
    {
        float cooldown = 1f / Mathf.Max(0.01f, fireRate);

        // 1) une unité ennemie à portée → on tire
        Unit target = ClosestEnemyAhead(out float gap);
        if (target != null && gap + 2f * BodyHalfWidth <= attackRange)
        {
            anim.SetMoving(false);
            if (attackTimer >= cooldown)
            {
                attackTimer = 0f;
                FireProjectile(target.AimPoint);
            }
            return;
        }

        // 2) la base adverse à portée → on la bombarde
        float baseGap = BaseGap();
        if (baseGap <= attackRange * 0.6f)
        {
            anim.SetMoving(false);
            if (attackTimer >= cooldown)
            {
                attackTimer = 0f;
                Bounds b = enemyBaseCollider.bounds;
                float front = facing > 0 ? b.min.x + 0.4f : b.max.x - 0.4f;
                FireProjectile(new Vector3(front, AimPoint.y, 0f));
            }
            return;
        }

        // 3) sinon on avance (en respectant la file)
        if (IsBlockedByAlly()) { anim.SetMoving(false); return; }
        MoveTowardsBase();
    }

    // gestion du projectile de l'unité qui tire
    private void FireProjectile(Vector3 aim)
    {
        if (projectilePrefab == null) return;

        Vector3 spawnPos = firePoint != null
            ? firePoint.position
            : transform.position + new Vector3(facing * 0.45f, spriteHeight * 0.4f, 0f);
        Vector3 dir = aim - spawnPos;
        dir.z = 0f;
        if (dir.sqrMagnitude < 0.0001f) dir = Vector3.right * facing;
        dir.Normalize();

        anim.Attack();

        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        proj.transform.right = dir;

        Projectile p = proj.GetComponent<Projectile>();
        if (p != null)
        {
            p.damage = attackDamage;
            p.team = unitTeam;
            p.shooter = this;
            p.rewardBase = baseP;
        }
    }
}
