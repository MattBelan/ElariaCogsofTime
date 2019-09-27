using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyDisplay : MonoBehaviour
{
    public CombatManager cm;
    public Camera cam;
    public GameObject playerCanvas;
    List<Text> enemyHealth;
    public Font textFont;

    // Start is called before the first frame update
    void Start()
    {
        enemyHealth = new List<Text>();
        foreach(EnemyScript enemy in cm.enemies)
        {
            GameObject textObj = new GameObject();
            textObj.transform.SetParent(playerCanvas.transform);
            Text text = textObj.AddComponent<Text>();
            enemyHealth.Add(text);
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < cm.enemies.Count; i++)
        {
            if (cm.enemies[i].Health <= 0)
            {
                enemyHealth[i].text = "";
            }
            else
            {
                enemyHealth[i].text = cm.enemies[i].Health + "/10";
            }
            enemyHealth[i].font = textFont;
            Vector3 screenPos = GetScreenPosition(cm.enemies[i].transform, playerCanvas.GetComponent<Canvas>(), cam);
            RectTransform textPos = enemyHealth[i].GetComponent<RectTransform>();
            textPos.anchoredPosition = screenPos;
        }
    }

    public static Vector3 GetScreenPosition(Transform transform, Canvas canvas, Camera cam)
    {
        Vector3 pos;
        float width = canvas.GetComponent<RectTransform>().sizeDelta.x;
        float height = canvas.GetComponent<RectTransform>().sizeDelta.y;
        float x = Camera.main.WorldToScreenPoint(transform.position).x / Screen.width;
        float y = Camera.main.WorldToScreenPoint(transform.position).y / Screen.height;
        pos = new Vector3(width * x - width / 2, y * height - height / 2);
        return pos;
    }
}
