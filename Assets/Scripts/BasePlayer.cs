using UnityEngine;

public class BasePlayer : MonoBehaviour
{
    public float maxHealth = 100;
    public int team = 0; // 0 for player, 1 for enemy

    public float currentHealth;

    public int goldPocket;

    public int startingGold = 150;

    [Header("Visuals")]
    public float healthBarHeight = 6.2f; // hauteur de la barre au-dessus du pied du château

    private WorldHealthBar healthBar;
    private TMPro.TextMeshPro hpLabel;
    private SpriteRenderer sr;
    private float hitFlash;
    private bool destroyed;

    // Start is called before the first frame update
    void Start()
    {
        goldPocket = startingGold;
        currentHealth = maxHealth;
        sr = GetComponent<SpriteRenderer>();

        int layer = sr != null ? sr.sortingLayerID : 0;
        healthBar = WorldHealthBar.Create(transform, new Vector3(0f, healthBarHeight, 0f),
                                          3.2f, 0.26f, team, layer, 100);

        var labelGo = new GameObject("HPLabel");
        labelGo.transform.SetParent(transform, false);
        labelGo.transform.localPosition = new Vector3(0f, healthBarHeight + 0.5f, 0f);
        hpLabel = labelGo.AddComponent<TMPro.TextMeshPro>();
        hpLabel.fontSize = 4f;
        hpLabel.fontStyle = TMPro.FontStyles.Bold;
        hpLabel.alignment = TMPro.TextAlignmentOptions.Center;
        hpLabel.enableWordWrapping = false;
        hpLabel.rectTransform.sizeDelta = new Vector2(6f, 1f);
        hpLabel.color = PixelArt.Hex("#f3e9d2");
        hpLabel.sortingLayerID = layer;
        hpLabel.sortingOrder = 110;
        RefreshLabel();
    }

    /// <summary>Remplace le carré coloré par le château dessiné et ajuste la zone de collision.</summary>
    public void ApplyFortress(Sprite fortress, Vector2 bodySize)
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = fortress;
            sr.color = Color.white;
            sr.flipX = team == 1;
            sr.drawMode = SpriteDrawMode.Simple;
            sr.sortingLayerName = "Personnage";
            sr.sortingOrder = 0;
        }
        transform.localScale = Vector3.one;

        var box = GetComponent<BoxCollider2D>();
        if (box != null)
        {
            box.size = bodySize;
            box.offset = new Vector2(0f, bodySize.y / 2f);
        }
        healthBarHeight = fortress.bounds.size.y + 0.5f;
    }

    void Update()
    {
        if (sr == null) return;
        hitFlash = Mathf.Max(0f, hitFlash - Time.unscaledDeltaTime * 5f);
        sr.color = Color.Lerp(Color.white, new Color(1f, 0.55f, 0.55f), hitFlash);
    }

    void RefreshLabel()
    {
        if (hpLabel != null)
            hpLabel.text = Mathf.CeilToInt(Mathf.Max(0f, currentHealth)) + " / " + Mathf.CeilToInt(maxHealth);
    }

    public void TakeDamage(float damage)
    {
        if (destroyed) return;

        currentHealth -= damage;
        hitFlash = 1f;
        if (healthBar != null) healthBar.Set(currentHealth / maxHealth);
        RefreshLabel();

        if (currentHealth <= 0)
        {
            destroyed = true;
            Destroy(gameObject);

            PauseMenu pauseMenu = FindObjectOfType<PauseMenu>();

            int winningTeam = (team == 0) ? 1 : 0;

            if (pauseMenu != null)
            {
                pauseMenu.ShowGameOver(winningTeam);
            }
        }
    }
}
