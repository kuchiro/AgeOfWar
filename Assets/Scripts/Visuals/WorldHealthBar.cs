using UnityEngine;

/// <summary>
/// Barre de vie dans le monde (SpriteRenderers), attachée au-dessus d'une unité ou d'une base.
/// Cadre sombre + traînée claire qui rattrape les dégâts + remplissage coloré par équipe.
/// Remplace l'ancienne barre UI (Slider) qui flottait en espace écran et n'était affichée que pour le joueur.
/// </summary>
public class WorldHealthBar : MonoBehaviour
{
    public static readonly Color AllyColor = PixelArt.Hex("#5fd06e");
    public static readonly Color EnemyColor = PixelArt.Hex("#e5484d");

    SpriteRenderer fill, trail;
    float width;
    float target = 1f, trailValue = 1f, trailDelay;
    Color baseColor;

    public static WorldHealthBar Create(Transform parent, Vector3 localPos, float width, float height,
                                        int team, int sortingLayerId, int sortingOrder)
    {
        var go = new GameObject("HealthBar");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        var bar = go.AddComponent<WorldHealthBar>();
        bar.Build(width, height, team == 0 ? AllyColor : EnemyColor, sortingLayerId, sortingOrder);
        return bar;
    }

    void Build(float w, float h, Color color, int layer, int order)
    {
        width = w;
        baseColor = color;
        float px = 1f / PixelArt.PPU;

        Rect("Frame", PixelArt.White, new Vector3(0, 0), new Vector2(w + 2 * px, h + 2 * px),
             PixelArt.Hex("#0b0d12"), layer, order);
        Rect("Back", PixelArt.WhiteLeft, new Vector3(-w / 2, 0), new Vector2(w, h),
             PixelArt.Hex("#3a1c20"), layer, order + 1);
        trail = Rect("Trail", PixelArt.WhiteLeft, new Vector3(-w / 2, 0), new Vector2(w, h),
             PixelArt.Hex("#f3dfae"), layer, order + 2);
        fill = Rect("Fill", PixelArt.WhiteLeft, new Vector3(-w / 2, 0), new Vector2(w, h),
             color, layer, order + 3);
        // reflet sur le haut de la barre
        var shine = Rect("Shine", PixelArt.WhiteLeft, new Vector3(-w / 2, h / 2 - px / 2), new Vector2(w, px),
             new Color(1, 1, 1, 0.25f), layer, order + 4);
        shine.transform.SetParent(fill.transform, true);
    }

    SpriteRenderer Rect(string name, Sprite sprite, Vector3 pos, Vector2 size, Color c, int layer, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = pos;
        go.transform.localScale = new Vector3(size.x, size.y, 1);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = c;
        sr.sortingLayerID = layer;
        sr.sortingOrder = order;
        return sr;
    }

    /// <summary>ratio = vie actuelle / vie max.</summary>
    public void Set(float ratio)
    {
        ratio = Mathf.Clamp01(ratio);
        if (ratio < target) trailDelay = 0.25f;
        target = ratio;
    }

    void LateUpdate()
    {
        if (fill == null) return;

        fill.transform.localScale = new Vector3(width * target, fill.transform.localScale.y, 1);
        // la vie basse fait virer la couleur vers l'orange pour attirer l'oeil
        fill.color = target < 0.35f ? Color.Lerp(PixelArt.Hex("#f0a53a"), baseColor, target / 0.35f) : baseColor;

        if (trailDelay > 0) trailDelay -= Time.deltaTime;
        else trailValue = Mathf.MoveTowards(trailValue, target, Time.deltaTime * 0.8f);
        if (trailValue < target) trailValue = target;
        trail.transform.localScale = new Vector3(width * trailValue, trail.transform.localScale.y, 1);
    }
}
