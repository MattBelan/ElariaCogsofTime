using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MageScript : PlayerScript
{
    public float abilityRadius;
    public float abilityRange;

    public bool usingAbility;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
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
            }
        }
    }

    public override void UseAbility()
    {
        usingAbility = !usingAbility;
    }

    public void ElementalBlast(Vector3 targetLocation)
    {
        if (playing)
        {
            //get all enemies in certain radius
            List<EnemyScript> enemiesInRange = new List<EnemyScript>();
            CombatManager combatManager = GameObject.FindGameObjectWithTag("CombatManager").GetComponent<CombatManager>();

            foreach (EnemyScript enemy in combatManager.enemies)
            {
                if (Vector3.Distance(targetLocation, enemy.transform.position) <= abilityRadius)
                {
                    enemiesInRange.Add(enemy);
                }
            }

            foreach (EnemyScript enemy in enemiesInRange)
            {
                Attack(enemy);
            }

            comLog[2].text = comLog[1].text;
            comLog[1].text = comLog[0].text;
            comLog[0].text = "Milosh used Elemental Blast!";
        }
    }
}
