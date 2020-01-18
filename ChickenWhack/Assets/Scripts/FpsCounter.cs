using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FpsCounter : MonoBehaviour
{
    Text text;

    string[] numStrings = new string[256];

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<Text>();
        
        for (int i = 0; i < numStrings.Length; i++)
        {
            numStrings[i] = i.ToString();
        }
    }

    float smoothFps = 0f;

    // Update is called once per frame
    void Update()
    {
        if(Time.deltaTime > 0f)
        {
            smoothFps = Mathf.Lerp(smoothFps, 1f / Time.deltaTime, Time.deltaTime * 2f);
        }

        text.text = numStrings[Mathf.CeilToInt(smoothFps)];
    }
}
