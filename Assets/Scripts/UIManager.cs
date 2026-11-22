using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public Button meleeButton;
    public Button rangedButton;

    public Spawner playerSpawner;

    public TextMeshProUGUI goldText;
    public BasePlayer playerBase;

    public GameObject meleePrefab;
    public GameObject rangedPrefab;

    [Header("Coûts")]
    public int meleeCost = 20;
    public int rangedCost = 15;

    private TextMeshProUGUI meleeLabel, rangedLabel;

    void Start()
    {
        meleeButton.onClick.AddListener(() => OnSpawnButtonClicked(meleePrefab, meleeCost));
        rangedButton.onClick.AddListener(() => OnSpawnButtonClicked(rangedPrefab, rangedCost));

        meleeLabel = meleeButton.GetComponentInChildren<TextMeshProUGUI>(true);
        rangedLabel = rangedButton.GetComponentInChildren<TextMeshProUGUI>(true);
        if (meleeLabel != null) meleeLabel.text = ButtonText("GUERRIER", meleeCost);
        if (rangedLabel != null) rangedLabel.text = ButtonText("TIREUR", rangedCost);
    }

    static string ButtonText(string name, int cost) =>
        name + "\n<size=75%><color=#F2C94C>" + cost + " or</color></size>";

    void OnSpawnButtonClicked(GameObject prefab, int unitCost)
    {
        if (playerBase != null && playerBase.goldPocket >= unitCost)
        {
            playerBase.goldPocket -= unitCost;   // on retire le coût
            playerSpawner.unitPrefab = prefab;
            playerSpawner.SpawnUnit();           // on fait apparaître l'unité
        }
    }

    void Update()
    {
        if (playerBase == null) return;

        int gold = playerBase.goldPocket;
        if (goldText != null) goldText.text = "OR  " + gold;

        // boutons grisés quand on n'a pas de quoi payer
        meleeButton.interactable = gold >= meleeCost;
        rangedButton.interactable = gold >= rangedCost;
    }
}
