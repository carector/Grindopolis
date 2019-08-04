using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageTextScript : MonoBehaviour {

    public Text damageText;
    private float riseAmount = 100;

    Transform lookAt;

	// Use this for initialization
	void Start () {

        StartCoroutine(RiseText());
	}
	
	// Update is called once per frame
	IEnumerator RiseText () {

        // Rotate towards the player if we have a stored transform
        if(lookAt != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - lookAt.position);
        }

        // Slowly "float" the text upwards
        while (riseAmount > 2)
        {
            damageText.rectTransform.anchoredPosition = new Vector2(damageText.rectTransform.anchoredPosition.x, damageText.rectTransform.anchoredPosition.y + riseAmount*0.1f);
            riseAmount /= 1.25f;
            yield return new WaitForFixedUpdate();
        }

        // Slight delay
        yield return new WaitForSeconds(0.1f);

        // Fade out the text
        while(damageText.color.a > 0.05f)
        {
            damageText.color = new Color(damageText.color.r, damageText.color.g, damageText.color.b, damageText.color.a - 0.075f);
            yield return new WaitForFixedUpdate();
        }

        // Remove our hitnumber
        Destroy(this.gameObject);
	}

    // Recieves amount of damage dealt 
    public void RecieveDamage(int damage, Transform target)
    {
        damageText.text = "-" + damage;
        lookAt = target;
    }

    public void RecieveDamage(int damage)
    {
        damageText.text = "-" + damage;
    }

    // Restores designated amount of health
    public void RestoreHealth(int health)
    {
        damageText.text = "+" + health;
    }
}
