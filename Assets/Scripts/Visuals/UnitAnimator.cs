using UnityEngine;

/// <summary>
/// Gère l'affichage d'une unité : animation de course / repos (découpée depuis les planches
/// Pixel Adventure), orientation vers l'ennemi, pieds posés au sol, ombre, coup d'attaque
/// (fente vers l'avant), flash rouge quand l'unité est touchée et petite apparition au spawn.
/// </summary>
public class UnitAnimator : MonoBehaviour
{
    SpriteRenderer body;
    Transform bodyT;
    Sprite[] run, idle, still;
    float fps, frameT, lunge, flash, spawnT;
    float facing = 1f;
    bool moving;

    public SpriteRenderer Body => body;

    public void Init(SpriteRenderer source, Texture2D runSheet, Texture2D idleSheet,
                     float height, float framesPerSecond, float facingDir)
    {
        fps = framesPerSecond;
        facing = facingDir;
        float ppu = 32f / height; // frames 32x32 → "height" unités monde

        run = PixelArt.SliceStrip(runSheet, 32, ppu);
        idle = PixelArt.SliceStrip(idleSheet, 32, ppu);

        // repli : le sprite d'origine du prefab, recadré avec un pivot aux pieds
        if (source != null && source.sprite != null)
        {
            var s = source.sprite;
            still = new[] { Sprite.Create(s.texture, s.textureRect, new Vector2(0.5f, 0f),
                s.textureRect.height / height, 0, SpriteMeshType.FullRect) };
        }

        var go = new GameObject("Body");
        go.transform.SetParent(transform, false);
        bodyT = go.transform;
        body = go.AddComponent<SpriteRenderer>();
        body.flipX = facing < 0;
        if (source != null)
        {
            body.sortingLayerID = source.sortingLayerID;
            body.sortingOrder = source.sortingOrder;
            source.enabled = false;
        }

        var shadow = new GameObject("Shadow").AddComponent<SpriteRenderer>();
        shadow.transform.SetParent(transform, false);
        shadow.transform.localPosition = new Vector3(0, 0.04f, 0);
        shadow.sprite = PixelArt.Shadow;
        shadow.sortingLayerID = body.sortingLayerID;
        shadow.sortingOrder = body.sortingOrder - 1;

        frameT = Random.value * 10f; // désynchronise les unités entre elles
        body.sprite = Pick();
    }

    public void SetMoving(bool value) => moving = value;
    public void Attack() => lunge = 1f;
    public void Hit() => flash = 1f;

    Sprite Pick()
    {
        var frames = moving ? (run ?? idle ?? still) : (idle ?? run ?? still);
        if (frames == null || frames.Length == 0) return body.sprite;
        return frames[(int)frameT % frames.Length];
    }

    void Update()
    {
        if (body == null) return;
        float dt = Time.deltaTime;

        frameT += dt * fps;
        body.sprite = Pick();

        // fente d'attaque : avance puis revient
        lunge = Mathf.Max(0, lunge - dt * 5f);
        float push = Mathf.Sin(lunge * Mathf.PI) * 0.28f * facing;

        // apparition : léger "pop" d'échelle
        spawnT = Mathf.Min(1f, spawnT + dt * 6f);
        float s = Mathf.Lerp(0.6f, 1f, 1f - (1f - spawnT) * (1f - spawnT));

        bodyT.localPosition = new Vector3(push, 0, 0);
        bodyT.localScale = new Vector3(s, s, 1);

        flash = Mathf.Max(0, flash - dt * 7f);
        body.color = Color.Lerp(Color.white, new Color(1f, 0.35f, 0.35f), flash);
    }
}
