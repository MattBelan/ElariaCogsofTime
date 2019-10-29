using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterScript : PlayerScript
{
    public float abilityRadius;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
    }

    public override void UseAbility()
    {
        if (playing)
        {
            //get all enemies in certain radius
            List<EnemyScript> enemiesInRange = new List<EnemyScript>();
            CombatManager combatManager = GameObject.FindGameObjectWithTag("CombatManager").GetComponent<CombatManager>();

            foreach(EnemyScript enemy in combatManager.enemies)
            {
                if(Vector3.Distance(transform.position, enemy.transform.position) <= abilityRadius)
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
            comLog[0].text = "Elaria used Flurry of Blades!";
        }
    }
}
