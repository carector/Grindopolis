using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameDisplayer : MonoBehaviour
{
    Image nameBackground;
    Text nameText;
    public GameObject playerTarget;
    public PlayerControllerRigidbody thisPlayer;


    // Start is called before the first frame update
    void Start()
    {
        
        nameText = GetComponentInChildren<Text>();
        nameBackground = GetComponentInChildren<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        playerTarget = GameObject.Find("ClientPlayer");

        if (playerTarget != null)
        {
            float opacity = 2 - (Vector3.Distance(transform.position, playerTarget.transform.position) / 10);

            nameText.color = new Color(nameText.color.r, nameText.color.g, nameText.color.b, opacity);
            nameBackground.color = new Color(nameBackground.color.r, nameBackground.color.g, nameBackground.color.b, opacity - (112/255));

            Vector3 targetVector = transform.position - playerTarget.transform.position;
            Quaternion lookAt = Quaternion.LookRotation(targetVector);

            transform.rotation = lookAt;

            nameText.text = thisPlayer.mVar.playerName;
        }
    }

    
}
