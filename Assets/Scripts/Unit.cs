using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] protected float speed = 2f;
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float attackDamage = 10f;
    [SerializeField] protected int goldGive = 10;

    [Header("Team")]
    public int unitTeam;          // 0 = joueur, 1 = ennemi
    public BasePlayer baseP;      // la base qui possède cette unité (team correspondante)
    public Transform targetBase;

    [Header("UI")]
    public GameObject healthBarPrefab;
    protected HealthBar healthBar;
    protected float currentHealth;

    protected virtual void Start()
    {
        currentHealth = maxHealth;

        // --- Barre de vie ---
        if (healthBarPrefab != null)
        {
            GameObject hb = Instantiate(healthBarPrefab);
            healthBar = hb.GetComponent<HealthBar>();

            healthBar.transform.SetParent(GameObject.Find("GameCanvas").transform, false);
            healthBar.target = transform;
            healthBar.offset = new Vector3(0, 1.5f, 0);
            healthBar.SetMaxHealth(maxHealth);

            // on cache la barre des ennemis
            healthBar.gameObject.SetActive(unitTeam == 0);
        }
    }

    protected virtual void Update()
    {
        MoveTowardsBase();
    }

    protected void MoveTowardsBase()
    {
        if (targetBase == null) return;

        Vector3 dir = (targetBase.position - transform.position).normalized;
        transform.Translate(dir * speed * Time.deltaTime);
    }

    //--- Collisions melee ---
    private void OnCollisionStay2D(Collision2D other)
    {
        Unit enemy = other.gameObject.GetComponent<Unit>();

        if (enemy != null && enemy.unitTeam != this.unitTeam)
        {
            enemy.TakeDamage(attackDamage * Time.deltaTime, this);
            TakeDamage(enemy.attackDamage * Time.deltaTime, enemy);
        }
    }

    // --- Damage ---
    public virtual void TakeDamage(float dmg, Unit attacker)
    {
        currentHealth -= dmg;
        healthBar?.SetHealth(currentHealth);

        if (currentHealth <= 0)
            Die(attacker);
    }

    // --- Death ---
    protected virtual void Die(Unit killer)
    {
        if (killer != null && killer.baseP != null)
        {
            killer.baseP.goldPocket += goldGive;
            Debug.Log($"TEAM {killer.unitTeam} gagne {goldGive} gold → total {killer.baseP.goldPocket}");
        }

        Destroy(healthBar?.gameObject);
        Destroy(gameObject);
    }

    public float GetAttackDamage() => attackDamage;
    public int GetUnitTeam() => unitTeam;
}
