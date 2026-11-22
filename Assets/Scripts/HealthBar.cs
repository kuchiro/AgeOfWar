using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public Vector3 offset;

    public Transform target;

    public void SetMaxHealth(float maxHealth)
    {
        slider.maxValue = maxHealth;
        slider.value = maxHealth;
    }

    public void SetHealth(float health)
    {
        slider.value = health;
    }

    void Update()
    {
        if (Camera.main != null && target != null)
        {
            transform.position = Camera.main.WorldToScreenPoint(target.position + offset);
        }
    }
}
