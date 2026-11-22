using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public float damage = 10f;

    // pour ne pas toucher ses alliés
    public int team;
    public float lifetime = 3f;
    public Unit shooter;           // pour créditer le tueur
    public BasePlayer rewardBase;  // reste valable même si le tireur meurt avant l'impact

    private bool consumed;

    private void Start()
    {
        // auto-destruction après qq secondes
        Destroy(gameObject, lifetime);

        // le projectile vole en ligne droite : pas de gravité
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.velocity = Vector2.zero;
        }

        // visuel : trait lumineux à la couleur de l'équipe, devant les personnages
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = PixelArt.Bolt;
            sr.color = team == 0 ? PixelArt.Hex("#a8e6ff") : PixelArt.Hex("#ffb36b");
            sr.sortingLayerName = "Personnage";
            sr.sortingOrder = 50;
        }
    }

    private void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (consumed) return;

        Unit target = other.GetComponent<Unit>();
        BasePlayer basePlayer = other.GetComponent<BasePlayer>();

        // toucher unité adverse
        if (target != null && target.GetUnitTeam() != team && !target.IsDead)
        {
            consumed = true;
            target.TakeDamage(damage, shooter, rewardBase);
            Destroy(gameObject);
        }
        //toucher base adverse
        else if (basePlayer != null && basePlayer.team != team)
        {
            consumed = true;
            basePlayer.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
