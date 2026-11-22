using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Restyle l'UI existante au chargement d'une scène (boutons Unity blancs par défaut → boutons
/// sombres à liseré doré, panneaux de pause / fin de partie assombris, HUD repositionné).
/// Rien n'est modifié dans les fichiers de scène : on ne touche qu'à l'apparence à l'exécution.
/// </summary>
public static class HUDStyler
{
    static readonly Color TextCream = PixelArt.Hex("#f3e9d2");
    static readonly Color Gold = PixelArt.Hex("#f2c94c");

    public static void StyleScene()
    {
        foreach (var b in Object.FindObjectsOfType<Button>(true)) StyleButton(b);

        foreach (var name in new[] { "PauseGame", "GameOverPanel" })
        {
            var go = FindAny(name);
            if (go == null) continue;
            var img = go.GetComponent<Image>();
            if (img != null) img.color = new Color(0.03f, 0.04f, 0.07f, 0.78f);
            foreach (var t in go.GetComponentsInChildren<TextMeshProUGUI>(true))
                if (t.GetComponentInParent<Button>() == null) StyleTitle(t);
        }

        LayoutGameHUD();
    }

    static GameObject FindAny(string name)
    {
        foreach (var t in Object.FindObjectsOfType<RectTransform>(true))
            if (t.name == name) return t.gameObject;
        return null;
    }

    public static void StyleButton(Button b)
    {
        var img = b.targetGraphic as Image;
        if (img != null)
        {
            img.sprite = PixelArt.UIButton;
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 2f;
            img.color = Color.white;
        }

        var colors = b.colors;
        colors.normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.highlightedColor = Color.white;
        colors.selectedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.75f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        b.colors = colors;

        foreach (var t in b.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            t.color = TextCream;
            t.fontStyle = FontStyles.Bold;
            t.enableAutoSizing = true;
            t.fontSizeMin = 10;
            t.fontSizeMax = 20;
            t.alignment = TextAlignmentOptions.Center;
            t.enableWordWrapping = true;
            t.margin = new Vector4(6, 2, 6, 2);
            t.characterSpacing = 2f;
        }
    }

    static void StyleTitle(TextMeshProUGUI t)
    {
        t.color = Gold;
        t.fontStyle = FontStyles.Bold | FontStyles.UpperCase;
        t.characterSpacing = 6f;
        t.enableAutoSizing = true;
        t.fontSizeMin = 14;
        t.fontSizeMax = 40;
    }

    /// <summary>Barre d'invocation centrée en bas, or en haut à gauche, pause en haut à droite.</summary>
    static void LayoutGameHUD()
    {
        Place("MeleeButton", new Vector2(0.5f, 0f), new Vector2(-92f, 14f), new Vector2(170f, 58f));
        Place("RangeButton", new Vector2(0.5f, 0f), new Vector2(92f, 14f), new Vector2(170f, 58f));
        Place("PauseButton", new Vector2(1f, 1f), new Vector2(-12f, -12f), new Vector2(96f, 34f));

        var pause = FindAny("PauseButton");
        if (pause != null)
            foreach (var t in pause.GetComponentsInChildren<TextMeshProUGUI>(true)) t.text = "PAUSE";

        var gold = FindAny("GoldText");
        if (gold != null)
        {
            var rt = (RectTransform)gold.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(12f, -12f);
            rt.sizeDelta = new Vector2(150f, 36f);

            var tmp = gold.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.enableAutoSizing = false;
                tmp.fontSize = 20;
                tmp.fontStyle = FontStyles.Bold;
                tmp.color = Gold;
                tmp.alignment = TextAlignmentOptions.Left;
                tmp.margin = new Vector4(14, 0, 10, 0);
            }

            // panneau sombre derrière le compteur d'or
            var panel = new GameObject("GoldPanel", typeof(RectTransform), typeof(Image));
            var prt = (RectTransform)panel.transform;
            prt.SetParent(rt.parent, false);
            prt.SetSiblingIndex(rt.GetSiblingIndex());
            prt.anchorMin = prt.anchorMax = prt.pivot = new Vector2(0f, 1f);
            prt.anchoredPosition = new Vector2(12f, -12f);
            prt.sizeDelta = new Vector2(150f, 36f);
            var pimg = panel.GetComponent<Image>();
            pimg.sprite = PixelArt.UIPanel;
            pimg.type = Image.Type.Sliced;
            pimg.pixelsPerUnitMultiplier = 2f;
            pimg.raycastTarget = false;
        }
    }

    static void Place(string name, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        var go = FindAny(name);
        if (go == null) return;
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = new Vector2(anchor.x == 0.5f ? 0.5f : anchor.x, anchor.y);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }
}
