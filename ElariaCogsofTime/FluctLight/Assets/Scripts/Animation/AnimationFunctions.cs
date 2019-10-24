using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * ----- Animation Functions Class -----
 * 
 * Useful for retrieving animation information regarding types, length of
 * animation clips, and the duration of said clips. This class should have
 * access to ALL animators.
 * 
 * --------------------------------------------------------------------------
 *
 * ! IMPORTANT ! - In order for this class to determine types properly,
 *                 naming conventions should follow a consistent pattern.
 *                 See Explanations Below:
 *
 *        ~ General Searches ~
 *                    "Elaria_Attack_Basic" will have type of 
 *                 "Character_Attack_Basic" because we check for the string
 *                           "Attack_Basic" within the name of the anim clip.
 *
 *        ~ Name Specific Searches ~
 *                 Self-explanatory but we check for the base name of the
 *                 associated object itself. 
 *                 "Attk_Panel_Enter" will have a type of 
 *                 "Attk_Panel" since we search for
 *                 "Attk_Panel" within the name of the anim clip.
 *
 */
public enum AnimationType
{
    Attk_Panel,
    Character_Run,
    Character_Attack_Basic,
    Character_UseItem,
    Unassigned,
}

public class AnimationFunctions : MonoBehaviour
{
    // Animations
    private List<Animator> animators;
    public GameObject[] animatedObjects;
    public Dictionary<string, AnimationItem> animationLibrary;

    public void Start()
    {
        // Get all animators present in the objects provided in the inspector
        animators = new List<Animator>();
        foreach (GameObject obj in animatedObjects)
        {
            Animator[] tempArr = obj.GetComponentsInChildren<Animator>();
            foreach (Animator anim in tempArr)
            {
                animators.Add(anim);
            }
        }

        // Determine the number of total clips
        int clipCount = 0;
        animators.ForEach(delegate(Animator anim) 
        {
            AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;
            clipCount += clips.Length;
        });

        // Create appropriate dictionary entries
        animationLibrary = new Dictionary<string, AnimationItem>(clipCount);
        animators.ForEach(delegate(Animator anim)
        {
            AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;
            foreach (AnimationClip clip in clips) 
            {
                // Debug.Log(clip.name);
                if (!animationLibrary.ContainsKey(clip.name))
                    animationLibrary.Add(clip.name, new AnimationItem(clip.name, clip.length));
            }
        });
        
        // Debug.Log(animationLibrary.ContainsKey("Test"));
    }
}
