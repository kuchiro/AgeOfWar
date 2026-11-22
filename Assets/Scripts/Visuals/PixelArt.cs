using UnityEngine;

/// <summary>
/// Petits utilitaires pour générer des textures / sprites pixel-art à l'exécution
/// (barres de vie, décor, châteaux, UI). Tout est en 16 pixels par unité monde
/// pour rester cohérent avec les personnages Pixel Adventure (32 px = 2 unités).
/// </summary>
public static class PixelArt
{
    public const float PPU = 16f;

    // ---------- Textures / sprites de base ----------

    public static Texture2D NewTexture(int w, int h, FilterMode filter = FilterMode.Point)
    {
        var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
        t.filterMode = filter;
        t.wrapMode = TextureWrapMode.Clamp;
        return t;
    }

    public static Sprite FromPixels(Color32[] px, int w, int h, Vector2 pivot, float ppu = PPU)
    {
        var t = NewTexture(w, h);
        t.SetPixels32(px);
        t.Apply(false, true);
        return Sprite.Create(t, new Rect(0, 0, w, h), pivot, ppu, 0, SpriteMeshType.FullRect);
    }

    static Sprite _white, _whiteLeft, _shadow, _bolt, _uiPanel, _uiButton;

    /// <summary>Carré blanc de 1x1 unité, pivot centre.</summary>
    public static Sprite White => _white != null ? _white : (_white = Solid(new Vector2(0.5f, 0.5f)));

    /// <summary>Carré blanc de 1x1 unité, pivot à gauche (pour les remplissages de barres).</summary>
    public static Sprite WhiteLeft => _whiteLeft != null ? _whiteLeft : (_whiteLeft = Solid(new Vector2(0f, 0.5f)));

    static Sprite Solid(Vector2 pivot)
    {
        var px = new Color32[16];
        for (int i = 0; i < px.Length; i++) px[i] = new Color32(255, 255, 255, 255);
        return FromPixels(px, 4, 4, pivot, 4f);
    }

    /// <summary>Ombre elliptique douce posée sous les pieds.</summary>
    public static Sprite Shadow
    {
        get
        {
            if (_shadow != null) return _shadow;
            const int w = 20, h = 6;
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float dx = (x + 0.5f - w / 2f) / (w / 2f);
                    float dy = (y + 0.5f - h / 2f) / (h / 2f);
                    float d = dx * dx + dy * dy;
                    byte a = d < 0.55f ? (byte)110 : d < 1f ? (byte)60 : (byte)0;
                    px[y * w + x] = new Color32(0, 0, 0, a);
                }
            return _shadow = FromPixels(px, w, h, new Vector2(0.5f, 0.5f));
        }
    }

    /// <summary>Projectile : trait lumineux (blanc, teinté par l'équipe via SpriteRenderer.color).</summary>
    public static Sprite Bolt
    {
        get
        {
            if (_bolt != null) return _bolt;
            const int w = 10, h = 3;
            var px = new Color32[w * h];
            for (int x = 0; x < w; x++)
            {
                byte tail = (byte)Mathf.Lerp(40, 255, x / (float)(w - 1));
                px[0 * w + x] = new Color32(255, 255, 255, (byte)(tail / 3));
                px[1 * w + x] = new Color32(255, 255, 255, tail);
                px[2 * w + x] = new Color32(255, 255, 255, (byte)(tail / 3));
            }
            return _bolt = FromPixels(px, w, h, new Vector2(0.8f, 0.5f));
        }
    }

    /// <summary>Découpe une bande horizontale de frames carrées (ex : Run 384x32 → 12 frames).</summary>
    public static Sprite[] SliceStrip(Texture2D tex, int frame, float ppu)
    {
        if (tex == null) return null;
        int n = Mathf.Max(1, tex.width / frame);
        var frames = new Sprite[n];
        for (int i = 0; i < n; i++)
            frames[i] = Sprite.Create(tex, new Rect(i * frame, 0, frame, Mathf.Min(frame, tex.height)),
                new Vector2(0.5f, 0f), ppu, 0, SpriteMeshType.FullRect);
        return frames;
    }

    // ---------- Bruit déterministe ----------

    public static float Hash(int x, int y, int seed = 0)
    {
        unchecked
        {
            uint h = (uint)(x * 374761393 + y * 668265263 + seed * 1442695041);
            h = (h ^ (h >> 13)) * 1274126177u;
            h ^= h >> 16;
            return (h & 0xFFFFFF) / (float)0x1000000;
        }
    }

    public static float Noise1D(float x, int seed)
    {
        int i = Mathf.FloorToInt(x);
        float f = x - i;
        f = f * f * (3f - 2f * f);
        return Mathf.Lerp(Hash(i, 0, seed), Hash(i + 1, 0, seed), f);
    }

    public static float Fbm(float x, int seed)
    {
        float sum = 0f, amp = 0.55f, norm = 0f;
        for (int o = 0; o < 4; o++)
        {
            sum += Noise1D(x, seed + o * 31) * amp;
            norm += amp;
            x *= 2.03f;
            amp *= 0.5f;
        }
        return sum / norm;
    }

    static readonly float[] Bayer4 =
    {
         0f/16,  8f/16,  2f/16, 10f/16,
        12f/16,  4f/16, 14f/16,  6f/16,
         3f/16, 11f/16,  1f/16,  9f/16,
        15f/16,  7f/16, 13f/16,  5f/16,
    };

    /// <summary>Quantifie t (0..1) en "steps" paliers avec un tramage ordonné → dégradé pixel-art.</summary>
    public static float Dither(float t, int px, int py, int steps)
    {
        float v = t * steps;
        float fl = Mathf.Floor(v);
        float th = Bayer4[(py & 3) * 4 + (px & 3)];
        return Mathf.Clamp01(((v - fl) > th ? fl + 1 : fl) / steps);
    }

    public static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out var c);
        return c;
    }

    // ---------- UI ----------

    /// <summary>Panneau UI arrondi semi-transparent (9-slice).</summary>
    public static Sprite UIPanel => _uiPanel != null ? _uiPanel
        : (_uiPanel = RoundedRect(Hex("#11151dE6"), Hex("#11151dE6"), Hex("#c9a45c"), 1.5f));

    /// <summary>Bouton UI arrondi : fond ardoise en dégradé + liseré doré (9-slice).</summary>
    public static Sprite UIButton => _uiButton != null ? _uiButton
        : (_uiButton = RoundedRect(Hex("#3a4356"), Hex("#1d222d"), Hex("#d7b26a"), 2f));

    static Sprite RoundedRect(Color top, Color bottom, Color border, float borderPx)
    {
        const int s = 48;
        const float r = 12f;
        var px = new Color32[s * s];
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                // distance signée à un rectangle arrondi
                float qx = Mathf.Abs(x + 0.5f - s / 2f) - (s / 2f - r);
                float qy = Mathf.Abs(y + 0.5f - s / 2f) - (s / 2f - r);
                float outside = new Vector2(Mathf.Max(qx, 0), Mathf.Max(qy, 0)).magnitude;
                float d = outside + Mathf.Min(Mathf.Max(qx, qy), 0) - r;

                float alpha = Mathf.Clamp01(0.5f - d);
                Color fill = Color.Lerp(bottom, top, y / (float)(s - 1));
                // fine ligne claire en haut pour le relief
                if (y >= s - 4 && d < -borderPx) fill = Color.Lerp(fill, Color.white, 0.08f);
                float b = Mathf.Clamp01(d + borderPx + 0.5f);
                Color c = Color.Lerp(fill, border, b);
                c.a *= alpha;
                px[y * s + x] = c;
            }
        var t = NewTexture(s, s, FilterMode.Bilinear);
        t.SetPixels32(px);
        t.Apply(false, true);
        return Sprite.Create(t, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 100f, 0,
            SpriteMeshType.FullRect, new Vector4(r + 2, r + 2, r + 2, r + 2));
    }
}
