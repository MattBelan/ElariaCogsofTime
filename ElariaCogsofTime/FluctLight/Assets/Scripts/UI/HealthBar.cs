using UnityEngine;

public class HealthBar : MonoBehaviour
{
    // Bar references
    public GameObject bar;
    public GameObject barDamage;

    // Set values
    public void SetHealth(float pTargetValue)
    {
        barDamage.transform.localScale = new Vector3(pTargetValue, 1f);
    }

    public void SetColor(Color pColor)
    {
        barDamage.GetComponent<SpriteRenderer>().color = pColor;
    }

    public void SetStartVals(float startScale) {
        bar.transform.localScale = new Vector3(startScale, 1f);
        barDamage.transform.localScale = new Vector3(startScale, 1f);
    }
}
