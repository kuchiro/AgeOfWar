using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Écran d'accueil vivant : même champ de bataille que le jeu (ciel, montagnes, châteaux) avec
/// une escarmouche décorative en arrière-plan — des soldats sortent de chaque château, se
/// croisent au milieu et se battent — sous le titre et les boutons du menu.
/// </summary>
public class MenuScene : MonoBehaviour
{
    [Header("Planches d'animation (camp bleu)")]
    public Texture2D[] blueRun;
    public Texture2D[] blueIdle;

    [Header("Planches d'animation (camp rouge)")]
    public Texture2D[] redRun;
    public Texture2D[] redIdle;

    [Header("Escarmouche")]
    public int maxPerSide = 4;
    public float walkSpeed = 1.7f;

    class Walker
    {
        public Transform t;
        public UnitAnimator anim;
        public WorldHealthBar bar;
        public int team;
        public float facing, hp = 100f, attackTimer;
    }

    readonly List<Walker> walkers = new List<Walker>();
    float baseX, centerX;
    readonly float[] spawnTimer = new float[2];

    void Start()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        cam.orthographicSize = 10f; // même cadrage que la scène de jeu
        centerX = cam.transform.position.x;

        float absX = Battlefield.BuildScenery(cam, 0.62f);
        baseX = absX;
        Castle(-1, Battlefield.PlayerColor);
        Castle(1, Battlefield.EnemyColor);

        spawnTimer[0] = 0.3f;
        spawnTimer[1] = 1.4f;

        StyleMenu();
    }

    void Castle(float side, Color color)
    {
        var go = new GameObject(side < 0 ? "ChateauBleu" : "ChateauRouge");
        go.transform.position = new Vector3(centerX + side * baseX, Battlefield.GroundY - 0.1f, 0f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Battlefield.BuildFortress(color);
        sr.flipX = side > 0;
        sr.sortingLayerName = "Personnage";
        sr.sortingOrder = 0;
    }

    // ------------------------------------------------------------ escarmouche

    void Update()
    {
        float dt = Time.deltaTime;
        for (int team = 0; team < 2; team++)
        {
            spawnTimer[team] -= dt;
            if (spawnTimer[team] <= 0f)
            {
                spawnTimer[team] = Random.Range(2.2f, 4.2f);
                if (CountTeam(team) < maxPerSide) Spawn(team);
            }
        }

        for (int i = walkers.Count - 1; i >= 0; i--) Think(walkers[i], dt);
    }

    int CountTeam(int team)
    {
        int n = 0;
        foreach (var w in walkers) if (w.team == team) n++;
        return n;
    }

    void Spawn(int team)
    {
        var runs = team == 0 ? blueRun : redRun;
        var idles = team == 0 ? blueIdle : redIdle;
        if (runs == null || runs.Length == 0) return;
        int k = Random.Range(0, runs.Length);

        float facing = team == 0 ? 1f : -1f;
        var go = new GameObject(team == 0 ? "SoldatBleu" : "SoldatRouge");
        go.transform.position = new Vector3(centerX - facing * (baseX - 2.2f), Battlefield.GroundY, 0f);

        var src = go.AddComponent<SpriteRenderer>();
        src.sortingLayerName = "Personnage";
        src.sortingOrder = 10;

        var anim = go.AddComponent<UnitAnimator>();
        anim.Init(src, runs[k], idles != null && k < idles.Length ? idles[k] : null, 2f, 18f, facing);

        var bar = WorldHealthBar.Create(go.transform, new Vector3(0f, 2.12f, 0f), 1.1f, 0.14f,
                                        team, src.sortingLayerID, 100);

        walkers.Add(new Walker { t = go.transform, anim = anim, bar = bar, team = team, facing = facing,
                                 attackTimer = Random.Range(0f, 0.5f) });
    }

    void Think(Walker w, float dt)
    {
        const float body = Unit.BodyHalfWidth * 2f;
        float x = w.t.position.x;
        w.attackTimer += dt;

        // ennemi au contact → on frappe
        Walker foe = null;
        float best = float.MaxValue;
        foreach (var o in walkers)
        {
            if (o.team == w.team) continue;
            float ahead = (o.t.position.x - x) * w.facing;
            if (ahead < -Unit.BodyHalfWidth) continue;
            float gap = Mathf.Abs(ahead) - body;
            if (gap < best) { best = gap; foe = o; }
        }
        if (foe != null && best <= 0.12f)
        {
            w.anim.SetMoving(false);
            if (w.attackTimer >= 0.9f)
            {
                w.attackTimer = 0f;
                w.anim.Attack();
                foe.hp -= Random.Range(18f, 34f);
                foe.anim.Hit();
                foe.bar.Set(foe.hp / 100f);
                if (foe.hp <= 0f) Kill(foe);
            }
            return;
        }

        // arrivé au château adverse → il entre et disparaît
        float enemyFront = centerX + w.facing * (baseX - 1.5f);
        if ((enemyFront - x) * w.facing <= Unit.BodyHalfWidth) { Kill(w); return; }

        // file derrière les alliés
        foreach (var o in walkers)
        {
            if (o == w || o.team != w.team) continue;
            float ahead = (o.t.position.x - x) * w.facing;
            if (ahead > 0.001f && ahead < body + 0.25f) { w.anim.SetMoving(false); return; }
        }

        w.anim.SetMoving(true);
        w.t.position += Vector3.right * w.facing * walkSpeed * dt;
    }

    void Kill(Walker w)
    {
        walkers.Remove(w);
        DeathFade.Spawn(w.anim.Body);
        Destroy(w.t.gameObject);
    }

    // ------------------------------------------------------------ interface

    static readonly Color Gold = PixelArt.Hex("#f2c94c");
    static readonly Color Cream = PixelArt.Hex("#f3e9d2");

    void StyleMenu()
    {
        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        // même mise à l'échelle que le HUD du jeu (sinon tout est minuscule en plein écran)
        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(800f, 600f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        // titre
        var title = Find(canvas, "TitreJeu");
        RectTransform titleRt = null;
        if (title != null)
        {
            titleRt = (RectTransform)title.transform;
            Place(titleRt, new Vector2(0.5f, 1f), new Vector2(0f, -34f), new Vector2(720f, 96f));
            var tmp = title.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = "AGE OF WAR";
                tmp.enableAutoSizing = false;
                tmp.fontSize = 72;
                tmp.fontStyle = FontStyles.Bold;
                tmp.characterSpacing = 14f;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.enableWordWrapping = false;
                tmp.color = Gold;
                tmp.outlineWidth = 0.22f;
                tmp.outlineColor = new Color32(30, 18, 10, 255);
                tmp.fontMaterial.EnableKeyword("UNDERLAY_ON");
                tmp.fontMaterial.SetFloat("_UnderlayOffsetY", -0.6f);
                tmp.fontMaterial.SetFloat("_UnderlayDilate", 0.3f);
                tmp.fontMaterial.SetColor("_UnderlayColor", new Color(0f, 0f, 0f, 0.6f));
            }
        }

        // filet doré + sous-titre
        var line = new GameObject("TitreFilet", typeof(RectTransform), typeof(Image));
        line.transform.SetParent(canvas.transform, false);
        Place((RectTransform)line.transform, new Vector2(0.5f, 1f), new Vector2(0f, -134f), new Vector2(260f, 2f));
        var lineImg = line.GetComponent<Image>();
        lineImg.color = new Color(Gold.r, Gold.g, Gold.b, 0.7f);
        lineImg.raycastTarget = false;

        var sub = new GameObject("SousTitre", typeof(RectTransform));
        sub.transform.SetParent(canvas.transform, false);
        Place((RectTransform)sub.transform, new Vector2(0.5f, 1f), new Vector2(0f, -142f), new Vector2(600f, 30f));
        var subTmp = sub.AddComponent<TextMeshProUGUI>();
        subTmp.text = "DÉFENDS TON CHÂTEAU  ·  RASE LE SIEN";
        subTmp.fontSize = 16;
        subTmp.characterSpacing = 8f;
        subTmp.alignment = TextAlignmentOptions.Center;
        subTmp.color = new Color(Cream.r, Cream.g, Cream.b, 0.85f);
        subTmp.raycastTarget = false;

        // boutons
        Button("ButtonPlay", "JOUER", new Vector2(0f, 30f), canvas);
        Button("ButtonQuitter", "QUITTER", new Vector2(0f, -38f), canvas);
    }

    static void Button(string name, string label, Vector2 pos, Canvas canvas)
    {
        var go = Find(canvas, name);
        if (go == null) return;
        Place((RectTransform)go.transform, new Vector2(0.5f, 0.5f), pos, new Vector2(240f, 54f));
        foreach (var t in go.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            t.text = label;
            t.fontSizeMax = 24;
        }
    }

    static GameObject Find(Canvas canvas, string name)
    {
        foreach (var t in canvas.GetComponentsInChildren<RectTransform>(true))
            if (t.name == name) return t.gameObject;
        return null;
    }

    static void Place(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, anchor.y);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }
}
