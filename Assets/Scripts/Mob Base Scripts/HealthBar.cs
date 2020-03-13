using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{

    public Color[] colors;
    public SpriteRenderer bar;
    public HP hp;
    float fullWidth;

	Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {
        fullWidth = bar.transform.localScale.x;
        offset = transform.position - hp.transform.position;
        transform.SetParent(hp.transform.parent);
    }

    // Update is called once per frame
    void Update()
    {

        transform.position = hp.transform.position + offset;

        float fractionalHealth = Mathf.Clamp01(hp.health / hp.maxHealth);
        bar.transform.localScale = new Vector3(fullWidth * fractionalHealth, bar.transform.localScale.y, bar.transform.localScale.z);
        bar.transform.localPosition = new Vector3(fullWidth * (fractionalHealth - 1) / 2, bar.transform.localPosition.y, bar.transform.localPosition.z);

		if(fractionalHealth > 0.6) {
            bar.color = colors[0];
		} else if (fractionalHealth > 0.25){
            bar.color = colors[1];
		} else {
            bar.color = colors[2];
		}

    }
}
