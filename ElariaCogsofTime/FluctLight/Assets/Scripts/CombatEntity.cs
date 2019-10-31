using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
    public bool isCursorOver { get; set; }
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
    public GameObject healthDisplay;
    public GameObject healthBar;
    public GameObject healthPrefab;

    // Start is called before the first frame update
    public virtual void Start()
    {
        startDodge = dodge;
    }

    // Update is called once per frame
     public virtual void Update()
    {
        
    }
}
