using UnityEngine;

/// <summary>Silhouette qui s'efface en montant à la mort d'une unité.</summary>
public class DeathFade : MonoBehaviour
{
    SpriteRenderer sr;
    float t;
    const float Duration = 0.45f;

    public static void Spawn(SpriteRenderer from)
    {
        if (from == null || from.sprite == null) return;
        var go = new GameObject("DeathFade");
        go.transform.position = from.transform.position;
        go.transform.localScale = from.transform.lossyScale;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = from.sprite;
        sr.flipX = from.flipX;
        sr.sortingLayerID = from.sortingLayerID;
        sr.sortingOrder = from.sortingOrder;
        go.AddComponent<DeathFade>().sr = sr;
    }

    void Update()
    {
        t += Time.deltaTime / Duration;
        if (t >= 1f) { Destroy(gameObject); return; }
        transform.position += Vector3.up * Time.deltaTime * 0.9f;
        transform.localScale = new Vector3(1f + t * 0.3f, 1f - t * 0.4f, 1f);
        sr.color = new Color(1f, 1f - t * 0.5f, 1f - t * 0.5f, 1f - t);
    }
}
