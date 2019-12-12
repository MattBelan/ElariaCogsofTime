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
    public GameObject healthPrefab;

    // Start is called before the first frame update
    public virtual void Start()
    {
        // Setuo healthbar
        healthDisplay = Instantiate(healthPrefab, new Vector3(transform.position.x, transform.position.y + .65f, transform.position.z), Quaternion.identity).GetComponent<HealthBar>();
        healthDisplay.SetupHealthBar(Health, MaxHealth);
        // Stats
        startDodge = dodge;
    }

    // Update is called once per frame
    public virtual void Update()
    {
        // Follow entity
        Vector3 newScale = healthDisplay.transform.localScale;
        newScale.x = Health / 10;
        healthDisplay.transform.localScale = newScale;

        if (Health != healthDisplay.currentHealth) {
            healthDisplay.currentHealth = Health;
        }
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
}
