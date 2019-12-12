using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;


// Parent Class for all combat entities, including player characters and enemy characters
[Serializable]

public class CombatEntity : MonoBehaviour
{
    public string id;
    public CombatManager combatManager;

    public bool Moving { get; set; }
    public float Health { get; set; }
    public float MaxHealth { get; set; }

    //Lerp Variables and Properties
    public bool IsLerping { get; set; }
    public bool LerpStart { get; set; }
    public bool IsCursorOver { get; set; }
    public float lerpSpeed;
    public float lerpLength;
    public float startTime;
    public Vector3 lerpTo;

    //Variables for combat
    public ActionIntent intendedAction;
    public float moveTotal;
    public float currentMove;
    public float range;
    public float damage;
    public bool usedAttack;
    public float dodge;
    public float startDodge;
    public float armor;

    //Animator
    public Animator animator;
    public float animFloat;

    //Data for saving
    public SaveData data;

    //Pathfinding
    public MovementGridScript grid;
    public Vector3 currGridTarget;
    public Vector3 endingTarget;
    public List<GridVertex> path;
    public int pathProgress;
    public Vector3 lerpEnd;

    //Health Bar
    public HealthBar healthDisplay;
    protected float displayHealth;
    public GameObject healthPrefab;

    // Start is called before the first frame update
    public virtual void Start()
    {   
        // Healthbar
        healthDisplay = Instantiate(healthPrefab, new Vector3(transform.position.x, transform.position.y + .65f, transform.position.z), Quaternion.identity).GetComponent<HealthBar>();

        // Stats
        startDodge = dodge;
    }

    // Update is called once per frame
    public virtual void Update()
    {
        // Healthbar ~ Set Scale
        //Debug.Log("Health: " + Health + " / " + displayHealth + " / " + MaxHealth + " - " + newScale.x);
        //float actualScaleVal = 2 * (displayHealth / MaxHealth);
        //healthDisplay.SetHealth(actualScaleVal);
        // Healthbar - Set Position
        healthDisplay.transform.position = transform.position + new Vector3(0.0f, 0.5f, 0.0f);
    }

    public virtual bool IsWithinRange(CombatEntity pTarget, float pRange)
    {
        if (pTarget) {
            float distToPlayer = Vector3.Distance(transform.position, pTarget.transform.position);
            return (distToPlayer <= pRange) 
                ? true 
                : false;
        }
        else {
            //Debug.Log("Unassigned target for " + id + ".IsWithinRange(CombatEntity pTarget, float pRange) call");
            return false;
        }
    }

    public virtual void TakeDamage(float dam)
    {
        Health -= dam;
    }

    public void CreateHeathBar()
    {   
        healthDisplay.SetStartVals(2);
        FunctionPeriodic.Create(() => {
            if (displayHealth > 0.01f) {
                if (displayHealth > Health + 0.01f) {
                    // Debug.Log("Health: " + Health + " / " + displayHealth + " / " + MaxHealth);
                    displayHealth -= .01f;
                    healthDisplay.SetHealth(2 * (displayHealth / MaxHealth));
                }
               
                // At 30% health
                if (displayHealth / MaxHealth < .3f) {
                    if ((int)((displayHealth / MaxHealth) * 100f) % 3 == 0) {
                        Debug.Log(" d-d " + displayHealth);
                        healthDisplay.SetColor(Color.white);
                    }
                    else {
                        Debug.Log(displayHealth);
                        healthDisplay.SetColor(Color.red);
                    }
                }
                // At 60% health
                else if (displayHealth / MaxHealth < .6f) {
                    healthDisplay.SetColor(Color.red);
                }
            }
        }, .05f);
    }
}
