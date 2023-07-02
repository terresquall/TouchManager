using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Terresquall.FruitSlicer;

public class ComboPop : MonoBehaviour
{
    private TextMeshPro text;
    private Color textColor;
    private float lifeTime;
    // Start is called before the first frame update

    public static ComboPop Pop(Vector3 position, int comboValue)
    {
        Fruit fruit = FindObjectOfType<Fruit>();
        Transform comboPopUpTransform = Instantiate(fruit.comboPopUp, position, Quaternion.identity);
        ComboPop comboPopUp = comboPopUpTransform.GetComponent<ComboPop>();
        comboPopUp.Setup(comboValue);

        return comboPopUp;
    }
    void Awake()
    {
        text = GetComponent<TextMeshPro>();
    }
    private void FixedUpdate()
    {
        transform.position += new Vector3(0, 10f) * Time.deltaTime;
        lifeTime -= Time.deltaTime;
        if(lifeTime <= 0)
        {
            textColor.a -= 5f * Time.deltaTime;
            text.color = textColor;
            if(textColor.a < 0)
            {
                Destroy(gameObject);
            }
        }
    }

    // Update is called once per frame
    public void Setup(int comboValue)
    {
        text.SetText(comboValue.ToString() + " Fruit Combo + " + comboValue.ToString());
        textColor = text.color;
        lifeTime = 0.4f;
    }
}
