using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;

public class HealthBar : MonoBehaviour
{
    // Bar references
    public GameObject bar;
    public GameObject barDamage;

    // Health Values
    public float currentHealth;
    public float maxHealth;

    public void SetupHealthBar(float pHealth, float pMaxHealth)
    {
        currentHealth = pHealth;
        maxHealth = pMaxHealth;
        FunctionPeriodic.Create(() => {
            if (currentHealth > 0) {
                currentHealth -= .01f;
                SetHealth(currentHealth);

                // At 60% health
                if (currentHealth / maxHealth > .6f) {
                    SetColor(Color.red);
                }
                // At 30% health
                else if (currentHealth / maxHealth > .3f) {
                    if ((currentHealth * 100f) % 3 == 0) 
                        SetColor(Color.white);
                    else
                        SetColor(Color.red);
                }
            }
        }, .03f);
    }

    // Set values
    public void SetHealth(float pTargetValue)
    {
        currentHealth = pTargetValue;
        barDamage.transform.localScale = new Vector3(currentHealth / maxHealth, 1f);
    }

    public void SetColor(Color pColor)
    {
        barDamage.GetComponent<SpriteRenderer>().color = pColor;
    }
}
