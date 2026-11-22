using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Met en scène le champ de bataille au chargement de la scène de jeu, sans rien modifier
/// dans l'éditeur : ciel crépusculaire, montagnes et forêt en parallaxe fixe, sol en pixel-art,
/// nuages qui dérivent, châteaux dessinés pour chaque camp, alignement de tout sur une même
/// ligne de sol, et restylage du HUD.
/// </summary>
public static class Battlefield
{
    /// <summary>Ordonnée du sol : les pieds des unités et le pied des châteaux sont posés ici.</summary>
    public static float GroundY = -4f;

    const float PPU = PixelArt.PPU;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Register()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HUDStyler.StyleScene();

        var bases = Object.FindObjectsOfType<BasePlayer>();
        if (bases.Length > 0) Build(bases);
    }

    /// <summary>
    /// Décor commun (ciel, montagnes, sol, nuages). groundFraction = position du sol sous le centre
    /// de l'écran, en fraction de la demi-hauteur. Renvoie l'abscisse (absolue) des châteaux.
    /// </summary>
    public static float BuildScenery(Camera cam, float groundFraction)
    {
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        Vector3 c = cam.transform.position;

        GroundY = c.y - halfH * groundFraction;

        // anciens décors (tuiles étirées) → masqués
        foreach (var r in Object.FindObjectsOfType<SpriteRenderer>())
            if (r.name.StartsWith("Terrain (16x16)") || r.name == "SolGame") r.gameObject.SetActive(false);

        cam.backgroundColor = SkyTop;

        BuildBackdrop(c, halfW, halfH);
        BuildClouds(c, halfW, halfH);

        return Mathf.Min(14f, halfW - 2.6f);
    }

    public static readonly Color PlayerColor = PixelArt.Hex("#3d6fd1");
    public static readonly Color EnemyColor = PixelArt.Hex("#c23b3b");

    static void Build(BasePlayer[] bases)
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        Vector3 c = cam.transform.position;

        float baseX = BuildScenery(cam, 0.42f);

        // bases aux deux extrémités visibles, posées au sol
        foreach (var b in bases)
        {
            float side = b.team == 0 ? -1f : 1f;
            b.transform.position = new Vector3(c.x + side * baseX, GroundY - 0.1f, 0f);
            b.ApplyFortress(BuildFortress(b.team == 0 ? PlayerColor : EnemyColor),
                            new Vector2(3.0f, 4.4f));
        }

        // les points d'apparition juste devant chaque château
        foreach (var sp in Object.FindObjectsOfType<Spawner>())
        {
            float side = sp.team == 0 ? -1f : 1f;
            sp.transform.position = new Vector3(c.x + side * (baseX - 2.2f), GroundY, 0f);
        }
    }

    // ---------------------------------------------------------------- Décor

    static readonly Color SkyTop = PixelArt.Hex("#151a2c");
    static readonly Color SkyMid = PixelArt.Hex("#3b4466");
    static readonly Color SkyLow = PixelArt.Hex("#d0805a");
    static readonly Color SunCol = PixelArt.Hex("#ffd9a0");
    static readonly Color FarCol = PixelArt.Hex("#5a5f80");
    static readonly Color MidCol = PixelArt.Hex("#2d3349");
    static readonly Color HillCol = PixelArt.Hex("#161c24");
    static readonly Color HazeCol = PixelArt.Hex("#e09a6c");
    static readonly Color GrassTop = PixelArt.Hex("#8fae5a");
    static readonly Color Grass = PixelArt.Hex("#5a7f3a");
    static readonly Color GrassDark = PixelArt.Hex("#3b5a2b");
    static readonly Color Dirt = PixelArt.Hex("#5e4532");
    static readonly Color DirtDeep = PixelArt.Hex("#1f1612");
    static readonly Color Pebble = PixelArt.Hex("#85705a");

    static void BuildBackdrop(Vector3 c, float halfW, float halfH)
    {
        int w = Mathf.CeilToInt((halfW * 2f + 2f) * PPU);
        int h = Mathf.CeilToInt((halfH * 2f + 2f) * PPU);
        float left = c.x - w / PPU / 2f;
        float bottom = c.y - h / PPU / 2f;
        float top = bottom + h / PPU;

        Vector2 sun = new Vector2(c.x + halfW * 0.3f, GroundY + 6f);
        var px = new Color32[w * h];

        for (int py = 0; py < h; py++)
        {
            float wy = bottom + (py + 0.5f) / PPU;
            for (int ix = 0; ix < w; ix++)
            {
                float wx = left + (ix + 0.5f) / PPU;
                px[py * w + ix] = wy < GroundY
                    ? GroundPixel(wx, wy, ix, py)
                    : SkyPixel(wx, wy, ix, py, top, sun);
            }
        }

        var go = new GameObject("Backdrop");
        go.transform.position = new Vector3(c.x, c.y, 5f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = PixelArt.FromPixels(px, w, h, new Vector2(0.5f, 0.5f));
        sr.sortingLayerName = "Background";
        sr.sortingOrder = -100;
    }

    static Color SkyPixel(float wx, float wy, int ix, int py, float top, Vector2 sun)
    {
        float up = wy - GroundY;

        // herbe qui dépasse au-dessus de la ligne de sol
        if (up < 3f / PPU)
        {
            float blade = PixelArt.Hash(ix, 7, 3);
            int bladeH = blade > 0.9f ? 3 : blade > 0.6f ? 2 : blade > 0.35f ? 1 : 0;
            if (up < bladeH / PPU) return bladeH >= 2 ? Grass : GrassTop;
        }

        // ciel : dégradé tramé bas → haut
        float t = Mathf.Clamp01(up / (top - GroundY));
        t = PixelArt.Dither(t, ix, py, 14);
        Color col = t < 0.4f ? Color.Lerp(SkyLow, SkyMid, t / 0.4f) : Color.Lerp(SkyMid, SkyTop, (t - 0.4f) / 0.6f);

        // soleil couchant + halo
        float d = Vector2.Distance(new Vector2(wx, wy), sun);
        float glow = Mathf.Clamp01(1f - d / 7f);
        col = Color.Lerp(col, SunCol, PixelArt.Dither(glow * glow * 0.55f, ix, py, 8));
        if (d < 1.4f) col = SunCol;

        // chaîne de montagnes lointaine
        float far = GroundY + 4.2f + 4.2f * PixelArt.Fbm(wx * 0.09f, 11);
        if (wy < far)
        {
            float shade = Mathf.Clamp01((far - wy) / 4f);
            col = Color.Lerp(Color.Lerp(col, FarCol, 0.8f), MidCol, PixelArt.Dither(shade * 0.35f, ix, py, 4));
        }

        // montagnes intermédiaires
        float mid = GroundY + 1.8f + 2.6f * PixelArt.Fbm(wx * 0.17f + 40f, 23);
        if (wy < mid) col = MidCol;

        // collines + forêt de sapins en silhouette
        float hill = HillHeight(wx);
        if (wy < hill || (up < 4.6f && InTree(wx, wy))) col = HillCol;

        // brume de chaleur au ras du sol
        float haze = Mathf.Exp(-up * 0.9f) * 0.35f;
        col = Color.Lerp(col, HazeCol, PixelArt.Dither(haze, ix, py, 6));
        return col;
    }

    static float HillHeight(float wx) => GroundY + 0.5f + 0.9f * PixelArt.Fbm(wx * 0.32f + 7f, 37);

    static bool InTree(float wx, float wy)
    {
        const float cell = 0.85f;
        int ci = Mathf.FloorToInt(wx / cell);
        for (int k = ci - 2; k <= ci + 2; k++)
        {
            if (PixelArt.Hash(k, 1, 51) < 0.4f) continue;
            float tx = (k + 0.2f + 0.6f * PixelArt.Hash(k, 2, 51)) * cell;
            float th = 1.0f + 1.6f * PixelArt.Hash(k, 3, 51);
            float baseY = HillHeight(tx) - 0.3f;
            float rel = (wy - baseY) / th;
            if (rel < 0f || rel > 1f) continue;
            // sapin en étages
            float tier = 1f - (rel * 3f % 1f) * 0.35f;
            float halfWidth = th * 0.3f * (1f - rel) * tier;
            if (Mathf.Abs(wx - tx) < halfWidth + 0.04f) return true;
        }
        return false;
    }

    static Color GroundPixel(float wx, float wy, int ix, int py)
    {
        float depth = GroundY - wy;

        if (depth < 1f / PPU) return GrassTop;
        if (depth < 4f / PPU) return Grass;
        if (depth < 9f / PPU && PixelArt.Hash(ix, py, 5) < (9f / PPU - depth) * 2.2f) return GrassDark;

        float t = Mathf.Clamp01(depth / 6.5f);
        Color col = Color.Lerp(Dirt, DirtDeep, PixelArt.Dither(t, ix, py, 7));

        // strates de terre
        float band = (wy + 0.25f * Mathf.Sin(wx * 0.6f)) * 2.2f;
        if (Mathf.Repeat(band, 1f) < 0.12f) col *= 0.82f;

        // cailloux (2x2 px)
        float stone = PixelArt.Hash(ix >> 1, py >> 1, 9);
        if (stone > 0.985f) col = Color.Lerp(Pebble, col, t * 0.6f);
        else if (stone > 0.975f) col *= 0.75f;

        col.a = 1f;
        return col;
    }

    // ---------------------------------------------------------------- Nuages

    static void BuildClouds(Vector3 c, float halfW, float halfH)
    {
        for (int i = 0; i < 6; i++)
        {
            var go = new GameObject("Cloud");
            float y = Mathf.Lerp(GroundY + 7f, c.y + halfH - 1.5f, PixelArt.Hash(i, 0, 77));
            float x = Mathf.Lerp(c.x - halfW, c.x + halfW, (i + PixelArt.Hash(i, 1, 77)) / 6f);
            go.transform.position = new Vector3(x, y, 4f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = BuildCloud(i);
            sr.sortingLayerName = "Background";
            sr.sortingOrder = -90;
            var drift = go.AddComponent<CloudDrift>();
            drift.speed = 0.15f + 0.25f * PixelArt.Hash(i, 2, 77);
            drift.minX = c.x - halfW - 3f;
            drift.maxX = c.x + halfW + 3f;
        }
    }

    static Sprite BuildCloud(int seed)
    {
        const int w = 56, h = 18;
        var px = new Color32[w * h];
        Color light = PixelArt.Hex("#e7b9a0");
        Color shade = PixelArt.Hex("#8f6f7e");
        int puffs = 4 + (int)(PixelArt.Hash(seed, 5, 3) * 3);
        var centers = new Vector3[puffs];
        for (int p = 0; p < puffs; p++)
        {
            float cx = Mathf.Lerp(10, w - 10, p / (float)(puffs - 1));
            float r = 4f + 4f * PixelArt.Hash(seed, p, 4);
            centers[p] = new Vector3(cx, 5f + r * 0.4f, r);
        }
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                bool inside = y >= 3 && y <= 5 && x > 6 && x < w - 6; // base plate
                foreach (var ce in centers)
                    if (new Vector2(x - ce.x, (y - ce.y) * 1.3f).sqrMagnitude < ce.z * ce.z) inside = true;
                if (!inside) continue;
                Color col = y < 6 ? shade : light;
                col.a = 0.55f;
                px[y * w + x] = col;
            }
        return PixelArt.FromPixels(px, w, h, new Vector2(0.5f, 0.5f));
    }

    // ---------------------------------------------------------------- Châteaux

    const int FW = 58, FH = 92; // 1 px de marge pour le contour

    static readonly Color StoneLight = PixelArt.Hex("#a29d95");
    static readonly Color StoneDark = PixelArt.Hex("#57534f");
    static readonly Color Outline = PixelArt.Hex("#16130f");
    static readonly Color GateDark = PixelArt.Hex("#140f0c");
    static readonly Color Wood = PixelArt.Hex("#5b4029");
    static readonly Color WindowLit = PixelArt.Hex("#f2b450");
    static readonly Color WindowCore = PixelArt.Hex("#ffe29a");

    /// <summary>Dessine un château fort en pixel-art aux couleurs d'une équipe (pivot : bas-centre).</summary>
    public static Sprite BuildFortress(Color team)
    {
        var px = new Color32[FW * FH];
        Color teamDark = team * 0.6f; teamDark.a = 1f;

        // tours latérales, rempart, donjon central
        Stone(px, 9, 0, 47, 48);
        Crenels(px, 11, 45, 48);
        Stone(px, 19, 46, 37, 72);
        Crenels(px, 19, 37, 72);
        Stone(px, 0, 0, 14, 62);
        Stone(px, 42, 0, 56, 62);

        // toits coniques des tours latérales
        Roof(px, 0, 14, 62, 16, team, teamDark);
        Roof(px, 42, 56, 62, 16, team, teamDark);

        // porte en arc + herse
        for (int y = 0; y < 24; y++)
            for (int x = 20; x < 36; x++)
            {
                float dx = x + 0.5f - 28f, dy = y - 15f;
                if (y >= 15 && dx * dx + dy * dy > 64f) continue;
                bool bar = (x - 20) % 3 == 1 || y % 5 == 2;
                Put(px, x, y, bar && y < 21 ? Wood : GateDark);
            }

        // fenêtres éclairées
        Window(px, 6, 38); Window(px, 48, 38); Window(px, 6, 22); Window(px, 48, 22);
        Window(px, 27, 58);

        // bannières sur le rempart
        Banner(px, 15, 26, team, teamDark);
        Banner(px, 36, 26, team, teamDark);

        // mât + drapeau ondulant
        for (int y = 76; y < 91; y++) Put(px, 28, y, Outline);
        for (int x = 29; x < 39; x++)
        {
            int wave = Mathf.RoundToInt(Mathf.Sin((x - 29) * 0.7f));
            for (int y = 83; y < 90; y++)
                Put(px, x, y + wave, y == 83 ? teamDark : team);
        }

        // contour sombre autour de la silhouette
        var outlined = (Color32[])px.Clone();
        for (int y = 0; y < FH; y++)
            for (int x = 0; x < FW; x++)
            {
                if (px[y * FW + x].a != 0) continue;
                bool edge = (x > 0 && px[y * FW + x - 1].a != 0) || (x < FW - 1 && px[y * FW + x + 1].a != 0)
                         || (y < FH - 1 && px[(y + 1) * FW + x].a != 0);
                if (edge) outlined[y * FW + x] = Outline;
            }

        return PixelArt.FromPixels(outlined, FW, FH, new Vector2(0.5f, 0f));
    }

    static void Put(Color32[] px, int x, int y, Color c)
    {
        x += 1; // marge du contour
        if (x < 0 || y < 0 || x >= FW || y >= FH) return;
        c.a = 1f;
        px[y * FW + x] = c;
    }

    // maçonnerie : briques décalées, teinte variable, ombre à gauche, pied assombri
    static void Stone(Color32[] px, int x0, int y0, int x1, int y1)
    {
        for (int y = y0; y < y1; y++)
            for (int x = x0; x < x1; x++)
            {
                int row = y / 4;
                int off = (row & 1) * 4;
                bool mortar = y % 4 == 3 || (x + off) % 8 == 7;
                int brick = (x + off) / 8;
                float side = 1f - (x - x0) / (float)Mathf.Max(1, x1 - x0 - 1);
                float t = 0.15f + side * 0.4f + PixelArt.Hash(brick, row, 13) * 0.25f;
                if (y < 4) t += 0.2f;
                Color c = Color.Lerp(StoneLight, StoneDark, Mathf.Clamp01(t));
                if (mortar) c *= 0.72f;
                Put(px, x, y, c);
            }
    }

    static void Crenels(Color32[] px, int x0, int x1, int y)
    {
        for (int x = x0; x < x1; x++)
            if ((x - x0) % 6 < 4) Stone(px, x, y, x + 1, y + 4);
    }

    static void Roof(Color32[] px, int x0, int x1, int y0, int height, Color team, Color teamDark)
    {
        float cx = (x0 + x1 - 1) / 2f;
        float half = (x1 - x0) / 2f + 1f;
        for (int y = 0; y < height; y++)
        {
            float hw = half * (1f - y / (float)height);
            for (int x = Mathf.FloorToInt(cx - hw); x <= Mathf.CeilToInt(cx + hw); x++)
            {
                if (Mathf.Abs(x - cx) > hw) continue;
                Color c = x < cx ? teamDark : team;
                if (y % 3 == 0) c *= 0.85f;
                Put(px, x, y0 + y, c);
            }
        }
    }

    static void Window(Color32[] px, int x, int y)
    {
        for (int j = 0; j < 7; j++)
            for (int i = 0; i < 3; i++)
            {
                bool frame = i == 0 || i == 2 || j == 0 || j == 6;
                Put(px, x + i - 1, y + j, frame ? GateDark : (j > 3 ? WindowCore : WindowLit));
            }
    }

    static void Banner(Color32[] px, int x, int y, Color team, Color teamDark)
    {
        for (int i = -1; i < 6; i++) Put(px, x + i, y + 17, Wood);
        for (int j = 0; j < 17; j++)
            for (int i = 0; i < 5; i++)
            {
                if (j < 3 && Mathf.Abs(i - 2) > j) continue; // pointe en V
                Put(px, x + i, y + j, i == 0 || i == 4 ? teamDark : (j % 6 == 4 ? PixelArt.Hex("#e8c46a") : team));
            }
    }
}
