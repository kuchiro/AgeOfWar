using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public float damage = 10f;

    // pour ne pas toucher ses alliés
    public int team;
    public float lifetime = 3f;
    public Unit shooter; // pour créditer le tueur

    private void Start()
    {
        // auto-destruction après qq secondes
        Destroy(gameObject, lifetime); 
    }

    private void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Unit target = other.GetComponent<Unit>();
        BasePlayer basePlayer = other.GetComponent<BasePlayer>();

        // toucher unité adverse
        if (target != null && target.GetUnitTeam() != team)
        {
            target.TakeDamage(damage, shooter);
            Destroy(gameObject);
        }
        //toucher base adverse
        else if (basePlayer != null && basePlayer.team != team)
        {
            basePlayer.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
