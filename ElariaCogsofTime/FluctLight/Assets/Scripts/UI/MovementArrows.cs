using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class MovementArrows : MonoBehaviour
{
    private Object[] sprites;
    private Dictionary<string, Sprite> spritesDict;
    public Dictionary<string, Sprite> Arrow 
    {
        get { return spritesDict; }
    }

    public void Awake () 
    {
        sprites = Resources.LoadAll("Visuals/Sprites/UI/BattleUI/Indication/ArrowSheet");
        spritesDict = new Dictionary<string, Sprite>();

        for (int i = 1; i < sprites.Count(); i++) //First value in sprites array is an unwanted Texture2D value
        {
            spritesDict.Add(sprites[i].name, (Sprite) sprites[i]);
        }
    }
}
