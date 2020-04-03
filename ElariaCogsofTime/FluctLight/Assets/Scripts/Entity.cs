using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    // External Refs
    // -----------------
    public CombatManager combatManager;
    public GameObject currentTile;

    // User Interface
    // ------------------
    public bool IsCursorOver { get; set; }
    public int displayHealth;

    // Stat Values
    // ---------------
    // ~ Volatile
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int healthStat;
    public int armorStat;


    // Set references
    public virtual void Awake ()
    {
        combatManager = GameObject.FindGameObjectWithTag("CombatManager").GetComponent<CombatManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void MouseHover ()
    {
        if (combatManager.IsPlaying)
        {
            IsCursorOver = true;
        }
    }
    public virtual void MouseExit ()
    {
        if (combatManager.IsPlaying) 
        {
            IsCursorOver = false;
        }
    }
}
