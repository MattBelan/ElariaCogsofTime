using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MageScript : PlayerScript
{
    public float abilityRadius;
    public float abilityRange;

    public bool usingAbility;


    public override void Start ()
    {
        base.Start();

        movementRangeStat = 4;
        attackRangeStat = 3;
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        if (usingAbility)
        {
            if (Input.GetMouseButtonDown(0))
            {
                //get mouse world position
                Vector3 mousePos = combatManager.cam.GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = -1;
                ElementalBlast(mousePos);
                abilityCooldown = 3;
            }
            if (Input.GetMouseButtonDown(1))
            {
                usingAbility = false;
            }
        }
    }

    public override void UseAbility()
    {
        usingAbility = !usingAbility;
    }

    public void ElementalBlast(Vector3 targetLocation)
    {
        //get all enemies in certain radius
        List<EnemyScript> enemiesInRange = new List<EnemyScript>();
        CombatManager combatManager = GameObject.FindGameObjectWithTag("CombatManager").GetComponent<CombatManager>();

        foreach (EnemyScript enemy in combatManager.enemies)
        {
            if (Vector3.Distance(targetLocation, enemy.transform.position) <= abilityRadius)
            {
                enemiesInRange.Add(enemy);
                // Debug.Log(enemiesInRange.Count);
            }
        }

        foreach (EnemyScript enemy in enemiesInRange)
        {
            enemy.TakeDamage(damageStat);
        }

        // comLog[2].text = comLog[1].text;
        // comLog[1].text = comLog[0].text;
        // comLog[0].text = "Milosh used Elemental Blast!";

        usingAbility = false;
    }
}
