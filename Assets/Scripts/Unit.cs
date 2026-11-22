using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Unité de corps à corps. Les unités avancent sur une seule ligne au sol vers la base adverse,
/// font la queue derrière leurs alliés (comme dans Age of War), s'arrêtent au contact d'un ennemi
/// et frappent à intervalle régulier, puis attaquent la base quand elles l'atteignent.
/// </summary>
public class Unit : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] protected float speed = 2f;
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float attackDamage = 10f;   // dégâts par coup
    [SerializeField] protected float attackCooldown = 1f;  // secondes entre deux coups
    [SerializeField] protected int goldGive = 10;

    [Header("Team")]
    public int unitTeam;          // 0 = joueur, 1 = ennemi
    public BasePlayer baseP;      // la base qui possède cette unité (team correspondante)
    public Transform targetBase;

    [Header("Visuals")]
    public Texture2D runSheet;    // planche "Run (32x32)"
    public Texture2D idleSheet;   // planche "Idle (32x32)"
    public float spriteHeight = 2f;
    public float animationFps = 18f;

    [Header("UI (ancien système, non utilisé)")]
    public GameObject healthBarPrefab;

    protected float currentHealth;
    protected float attackTimer;
    protected bool isDead;
    protected float facing = 1f;
    protected UnitAnimator anim;
    protected WorldHealthBar healthBar;
    protected BasePlayer enemyBase;
    protected Collider2D enemyBaseCollider;

    public const float BodyHalfWidth = 0.42f;
    const float MeleeGap = 0.12f;   // distance entre deux corps pour pouvoir frapper
    const float QueueGap = 0.25f;   // espace laissé derrière un allié

    static readonly List<Unit> all = new List<Unit>();

    public bool IsDead => isDead;
    public Vector3 AimPoint => transform.position + Vector3.up * spriteHeight * 0.4f;

    protected virtual void OnEnable() => all.Add(this);
    protected virtual void OnDisable() => all.Remove(this);

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        attackTimer = attackCooldown * 0.5f;

        if (targetBase != null)
        {
            enemyBase = targetBase.GetComponent<BasePlayer>();
            enemyBaseCollider = targetBase.GetComponent<Collider2D>();
            facing = targetBase.position.x >= transform.position.x ? 1f : -1f;
        }
        else
        {
            facing = unitTeam == 0 ? 1f : -1f;
        }

        SetupBody();
    }

    /// <summary>Pose l'unité au sol, normalise sa taille et branche animation, ombre et barre de vie.</summary>
    void SetupBody()
    {
        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.identity;
        transform.position = new Vector3(transform.position.x, Battlefield.GroundY, 0f);

        // Plus de physique "molle" : le déplacement est géré ici, la physique ne sert qu'aux projectiles.
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.velocity = Vector2.zero;
            rb.freezeRotation = true;
        }
        var box = GetComponent<BoxCollider2D>();
        if (box != null)
        {
            box.size = new Vector2(BodyHalfWidth * 2f, spriteHeight * 0.75f);
            box.offset = new Vector2(0f, spriteHeight * 0.375f);
        }

        var sr = GetComponent<SpriteRenderer>();
        anim = gameObject.AddComponent<UnitAnimator>();
        anim.Init(sr, runSheet, idleSheet, spriteHeight, animationFps, facing);

        int layer = sr != null ? sr.sortingLayerID : 0;
        healthBar = WorldHealthBar.Create(transform, new Vector3(0f, spriteHeight + 0.12f, 0f),
                                          1.1f, 0.14f, unitTeam, layer, 100);
    }

    protected virtual void Update()
    {
        if (isDead) return;
        attackTimer += Time.deltaTime;
        Think();
    }

    /// <summary>Décision de l'unité de mêlée à chaque frame.</summary>
    protected virtual void Think()
    {
        Unit enemy = ClosestEnemyAhead(out float gap);
        if (enemy != null && gap <= MeleeGap)
        {
            anim.SetMoving(false);
            if (attackTimer >= attackCooldown)
            {
                attackTimer = 0f;
                anim.Attack();
                enemy.TakeDamage(attackDamage, this);
            }
            return;
        }

        if (BaseGap() <= MeleeGap)
        {
            anim.SetMoving(false);
            if (attackTimer >= attackCooldown)
            {
                attackTimer = 0f;
                anim.Attack();
                enemyBase.TakeDamage(attackDamage);
            }
            return;
        }

        if (IsBlockedByAlly()) { anim.SetMoving(false); return; }

        MoveTowardsBase();
    }

    protected void MoveTowardsBase()
    {
        anim.SetMoving(true);
        transform.position += Vector3.right * facing * speed * Time.deltaTime;
    }

    // ---------- Perception ----------

    /// <summary>Ennemi le plus proche devant soi ; gap = espace libre entre les deux corps.</summary>
    protected Unit ClosestEnemyAhead(out float gap)
    {
        Unit best = null;
        gap = float.MaxValue;
        float x = transform.position.x;
        foreach (var u in all)
        {
            if (u == this || u.isDead || u.unitTeam == unitTeam) continue;
            float ahead = (u.transform.position.x - x) * facing;
            if (ahead < -BodyHalfWidth) continue; // derrière nous
            float g = Mathf.Abs(ahead) - 2f * BodyHalfWidth;
            if (g < gap) { gap = g; best = u; }
        }
        return best;
    }

    /// <summary>Espace libre entre l'unité et la façade de la base ennemie.</summary>
    protected float BaseGap()
    {
        if (enemyBase == null || enemyBaseCollider == null) return float.MaxValue;
        Bounds b = enemyBaseCollider.bounds;
        float front = facing > 0 ? b.min.x : b.max.x;
        return (front - transform.position.x) * facing - BodyHalfWidth;
    }

    /// <summary>Vrai si un allié est juste devant : on attend pour garder une file propre.</summary>
    protected bool IsBlockedByAlly()
    {
        float x = transform.position.x;
        foreach (var u in all)
        {
            if (u == this || u.isDead || u.unitTeam != unitTeam) continue;
            float ahead = (u.transform.position.x - x) * facing;
            bool inFront = ahead > 0.001f || (Mathf.Abs(ahead) <= 0.001f && u.GetInstanceID() < GetInstanceID());
            if (inFront && ahead < 2f * BodyHalfWidth + QueueGap) return true;
        }
        return false;
    }

    // ---------- Dégâts ----------

    public void TakeDamage(float dmg, Unit attacker) => TakeDamage(dmg, attacker, null);

    public virtual void TakeDamage(float dmg, Unit attacker, BasePlayer rewardTo)
    {
        if (isDead) return;
        currentHealth -= dmg;
        if (healthBar != null) healthBar.Set(currentHealth / maxHealth);
        if (anim != null) anim.Hit();

        if (currentHealth <= 0)
        {
            if (rewardTo == null && attacker != null) rewardTo = attacker.baseP;
            Die(rewardTo);
        }
    }

    protected virtual void Die(BasePlayer rewardTo)
    {
        isDead = true;

        if (rewardTo != null)
        {
            rewardTo.goldPocket += goldGive;
            if (rewardTo.team == 0)
                FloatingText.Spawn(transform.position + Vector3.up * (spriteHeight + 0.6f),
                                   "+" + goldGive + " or", PixelArt.Hex("#f2c94c"),
                                   anim != null ? anim.Body.sortingLayerID : 0);
        }

        if (anim != null) DeathFade.Spawn(anim.Body);
        Destroy(gameObject);
    }

    public float GetAttackDamage() => attackDamage;
    public int GetUnitTeam() => unitTeam;
}
