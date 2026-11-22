using UnityEngine;

/// <summary>Texte flottant (ex : "+10 or") qui monte et disparaît.</summary>
public class FloatingText : MonoBehaviour
{
    TMPro.TextMeshPro tmp;
    float t;

    public static void Spawn(Vector3 pos, string text, Color color, int sortingLayerId)
    {
        var go = new GameObject("FloatingText");
        go.transform.position = pos;
        var tmp = go.AddComponent<TMPro.TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = 5f;
        tmp.fontStyle = TMPro.FontStyles.Bold;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        tmp.color = color;
        tmp.rectTransform.sizeDelta = new Vector2(4, 1);
        tmp.sortingLayerID = sortingLayerId;
        tmp.sortingOrder = 200;
        go.AddComponent<FloatingText>().tmp = tmp;
    }

    void Update()
    {
        t += Time.deltaTime / 1.1f;
        if (t >= 1f) { Destroy(gameObject); return; }
        transform.position += Vector3.up * Time.deltaTime * 1.1f;
        var c = tmp.color;
        c.a = t < 0.6f ? 1f : 1f - (t - 0.6f) / 0.4f;
        tmp.color = c;
    }
}
