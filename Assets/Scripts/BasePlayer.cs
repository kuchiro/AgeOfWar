using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasePlayer : MonoBehaviour
{
    public float maxHealth = 100;
    public int team = 0; // 0 for player, 1 for enemy

    public float currentHealth;

    public int goldPocket;

    public int startingGold = 150;

    // Start is called before the first frame update
    void Start()
    {
        goldPocket = startingGold;
        currentHealth = maxHealth;
    }


    private void OnCollisionEnter2D(Collision2D other)
    {
        Unit otherUnit = other.gameObject.GetComponent<Unit>();
        
        if (otherUnit != null && otherUnit.GetUnitTeam() != this.team)
        {
            TakeDamage(otherUnit.GetAttackDamage() * Time.deltaTime);
        }
    }
    
    private void OnCollisionStay2D(Collision2D other) 
    {
        Unit otherUnit = other.gameObject.GetComponent<Unit>();
        if (otherUnit != null && otherUnit.GetUnitTeam() != this.team)
        {
            TakeDamage(otherUnit.GetAttackDamage() * Time.deltaTime);
            
        }
    }


    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            
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
