using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : MonoBehaviour
{
    public string type;
    public int value;
    public bool isEquiped;

    // Start is called before the first frame update
    void Start()
    {
        isEquiped = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Equip(PlayerScript player)
    {
        if (isEquiped)
        {
            switch (type)
            {
                case "armor":
                    player.armor -= value;
                    break;

                case "weapon":
                    player.damage -= value;
                    break;

                default:
                    break;
            }
            isEquiped = !isEquiped;
        }
        else
        {
            switch (type)
            {
                case "armor":
                    player.armor += value;
                    break;

                case "weapon":
                    player.damage += value;
                    break;

                default:
                    break;
            }
            isEquiped = !isEquiped;
        }
    }
}
