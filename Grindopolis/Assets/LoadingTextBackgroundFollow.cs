using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingTextBackgroundFollow : MonoBehaviour
{
    public RectTransform loadingText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(GetComponent<RectTransform>().anchoredPosition, loadingText.anchoredPosition, 0.25f);
    }
}
