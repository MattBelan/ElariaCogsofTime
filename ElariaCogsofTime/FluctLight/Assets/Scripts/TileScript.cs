using UnityEngine;

public class TileScript : MonoBehaviour 
{
    // External Refs
    // -----------------
    [HideInInspector]
    public CombatManager combatManager;

    // Tile Info
    // -------------
    public GameObject onTile;
    public string tileInfo;


    public void OnMouseEnter () 
    {
        PlayerScript playerEntity = combatManager.player; 
        if (!combatManager.IsPlaying)
            return;

        if (onTile)
        {
            // Selector
            if (!combatManager.activeSelector && combatManager.playerCharacters[combatManager.curPlayerIndex].intendedAction == ActionIntent.Attack)
                combatManager.activeSelector = Instantiate(combatManager.selectorPrefabs[1], gameObject.transform.position, gameObject.transform.rotation);

            if (onTile.GetComponent<CombatEntity>()) 
            {
                CombatEntity entity = onTile.GetComponent<CombatEntity>();
                entity.MouseHover();
            }
        }
        else
        {
            if (combatManager.playerCharacters[combatManager.curPlayerIndex].intendedAction == ActionIntent.Move)
            {
                // Clear view, then render new view
                combatManager.ResetMovementPreview();
                
                // Selector
                if (!combatManager.activeSelector)
                    combatManager.activeSelector = Instantiate(combatManager.selectorPrefabs[0], gameObject.transform.position, gameObject.transform.rotation);

                // If the tile isn't occupied, show movement preview
                playerEntity.path = playerEntity.PreviewMovement(this);
                combatManager.GenerateArrows(playerEntity.path);
            }
        }   
    }

    public void OnMouseOver ()
    {
        if (onTile)
        {
            if (onTile.GetComponent<CombatEntity>()) 
            {
                CombatEntity entity = onTile.GetComponent<CombatEntity>();
                entity.MouseHover();
            }
        }
        
        // Button press
        if (Input.GetMouseButtonDown(0))
        {
            PlayerScript playerScript = combatManager.player;

            if (!this.onTile)
                playerScript.MoveTo(this);
        }
    }
}
