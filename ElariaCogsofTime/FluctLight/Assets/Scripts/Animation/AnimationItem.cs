using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationItem 
{
    public AnimationType animType;
    public string name;
    public float duration;

    // Constructor
    public AnimationItem(string pName, float pDuration) 
    {
        name = pName;
        duration = pDuration;
        animType = DetermineType(pName);
    }

    // Determine type from names of animations
    private AnimationType DetermineType (string pName)
    {
        if (pName.Contains("Attk_Panel"))
            return AnimationType.Attk_Panel;
        else if (pName.Contains("Run"))
            return AnimationType.Character_Run;
        else if (pName.Contains("Attack_Basic"))
            return AnimationType.Character_Attack_Basic;
        else if (pName.Contains("UseItem"))
            return AnimationType.Character_UseItem;
        
        else
            return AnimationType.Unassigned;
    }
}