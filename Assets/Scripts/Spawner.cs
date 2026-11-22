using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Références")]
    public GameObject unitPrefab;     // le prefab de l’unité à instancier
    //public Transform spawnPoint;      // là où l’unité va apparaître
    public Transform targetBase;      // la base ennemie à attaquer

    public BasePlayer basePlayer; 

    [Header("Paramètres d’équipe")]
    public int team = 0;              // 0 = joueur, 1 = ennemi

    [Header("Timing")]
    public float spawnDelay = 3f;     // délai entre les spawns

    public float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnDelay)
        {
            SpawnUnit();
            timer = 0f;
        }
    }

    public void SpawnUnit()
    {
        if (unitPrefab != null)
        {
            GameObject newUnit = Instantiate(unitPrefab, transform.position, Quaternion.identity);
            
            // On récupère le script Unit et on lui donne les infos nécessaires
            Unit unitScript = newUnit.GetComponent<Unit>();
            if (unitScript != null)
            {
                unitScript.targetBase = targetBase;
                unitScript.unitTeam = team;
                unitScript.baseP = basePlayer;
            }
        }
    }
}
