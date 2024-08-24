using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SliceType
{
    Slice1 = 1,
    Slice2 = 2,
    Slice3 = 4,
    Slice4 = 8,
    Slice5 = 16,
    Slice6 = 32
}
public class SushiController : MonoBehaviour
{
    public const float SLICE_HEIGHT = 0.75f;

    public Vector3[] SlicesPos;
    public float SlicePadding = 0.375f;
    public float PlaceOffset = 0;
    [InspectorName("Total slices that Sushi has")]
    public int TotalSlices = 3;

    public SliceType[] PreferedSliceTypes;
    public SliceType[] CurSliceTypes;

    private int m_sliceCount;
    private bool isFull;

    
    // Start is called before the first frame update
    void Start()
    {
        DitributePlaceForSlices();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public Vector3 GetSlicePos()
    {
        if(isFull)
        {
            return new Vector3(transform.position.x, -10000, 0);
        }
        Vector3 res = SlicesPos[m_sliceCount];
        ++m_sliceCount;
        if(m_sliceCount == 3)
        {
            isFull = true;
        }
        return res;
    }
    void DitributePlaceForSlices()
    {
        SlicesPos = new Vector3[TotalSlices];
        for (int i = 0; i < TotalSlices; i++)
        {
            SlicesPos[i] = new Vector3(transform.position.x,transform.position.y + i * SlicePadding+PlaceOffset, 0);
        }
    }
}
