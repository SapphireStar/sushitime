using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConveyerBelt : MonoBehaviour
{
    public RawImage[] belts;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var belt in belts)
        {
            belt.uvRect = new Rect(0, (belt.uvRect.y + Time.deltaTime*3) % 1, 1, 1);
        }  
    }
}
