using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public Button meleeButton;
    public Button rangedButton;
    
    //public Button spawnButton;
    public Spawner playerSpawner;

    public TextMeshProUGUI goldText;
    public BasePlayer playerBase;

    public GameObject meleePrefab;
    public GameObject rangedPrefab;


    // Start is called before the first frame update
    void Start()
    {
    
        meleeButton.onClick.AddListener(() => OnSpawnButtonClicked(meleePrefab, 20));
        rangedButton.onClick.AddListener(() => OnSpawnButtonClicked(rangedPrefab, 15));
        
    }

    /*void OnSpawnButtonClicked()
    {
        int unitCost = 20;

        if (playerBase.goldPocket >= unitCost)
        {
            playerBase.goldPocket -= unitCost;   // on retire le coût
            playerSpawner.SpawnUnit();           // on fait apparaître l’unité
        }
    }*/

     void OnSpawnButtonClicked(GameObject prefab, int unitCost)
    {
        //int unitCost = 20;

        if (playerBase.goldPocket >= unitCost)
        {
            playerBase.goldPocket -= unitCost;   // on retire le coût
            playerSpawner.unitPrefab = prefab;
            playerSpawner.SpawnUnit();           // on fait apparaître l’unité
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        if (playerBase != null && goldText != null)
        {
            //Debug.Log("Updating gold display: " + playerBase.team);
            goldText.text = "Gold: " + playerBase.goldPocket.ToString();
        }
    }
}
