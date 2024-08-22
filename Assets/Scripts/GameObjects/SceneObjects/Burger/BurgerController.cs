using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurgerController : MonoBehaviour
{
    public const float SLICE_HEIGHT = 0.75f;

    public SliceController[] Slices;
    public Vector3[] SlicesPos;
    public float SlicePadding = 0.375f;
    public float PlaceOffset = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        
        DitributePlaceForSlices();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public Vector3 GetSlicePos(SliceController slice)
    {
        for (int i = 0; i < Slices.Length; i++)
        {
            if(slice == Slices[i])
            {
                return SlicesPos[i];
            }
        }
        return Vector3.zero;
    }
    void DitributePlaceForSlices()
    {
        SlicesPos = new Vector3[Slices.Length];
        for (int i = 0; i < Slices.Length; i++)
        {
            SlicesPos[i] = new Vector3(0, i * SlicePadding+PlaceOffset, 0);
        }
    }
}
