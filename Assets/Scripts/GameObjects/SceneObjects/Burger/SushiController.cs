using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SliceType
{
    Rice = 1,
    Slice2 = 2,
    Slice3 = 4,
    Slice4 = 8,
    Slice5 = 16,
    Slice6 = 32,
    Slice7 = 64
}
public class SushiResultModel
{
    public int CorrectSlice;
    public int CorrectOrder;
    public int FinalScore;
    public SushiResultModel(int correctSlice, int correctOrder, int finalScore)
    {
        this.CorrectSlice = correctSlice;
        this.CorrectOrder = correctOrder;
        this.FinalScore = finalScore;
    }
    public void print()
    {
        Debug.Log($"CorrectSlice: {CorrectSlice}");
        Debug.Log($"CorrectOrder: {CorrectOrder}");
        Debug.Log($"FinalScore: {FinalScore}");
    }

}
public class SushiController : MonoBehaviour
{
    public const float SLICE_HEIGHT = 0.75f;
    [InspectorName("Total slices that Sushi has")]
    public const int TOTAL_SLICES = 3;
    public const int SLICE_SCALE = 3;

    public float RightOrderIncrease = 30;
    public float WrongOrderIncrease = -5;

    [InspectorName("Offsets for fall down slices")]
    public Vector3[] SlicesPos;
    public float SlicePadding = 0.375f;
    public float PlaceOffset = 0;


    [InspectorName("Get Child Slices")]
    public SpriteRenderer[] SliceIcons;
    public SpriteRenderer SushiIcon;
        
    public List<SliceType> PreferredSliceTypes;
    public List<SliceType> CurSliceTypes;

    [InspectorName("Waypoints that the complete Sushi will follow")]
    public Transform SushiDeliverWaypoints;
    private List<Transform> waypoints;

    [InspectorName("The speed of Sushi to be delivered")]
    public float SushiMoveSpeed=5;


    private int m_sliceCount;
    private bool isFull;
    public bool IsFull
    {
        get =>isFull;
    }
    private bool isDelivered;
    public bool IsDelivered
    {
        get => isDelivered;
    }


    // Start is called before the first frame update
    void Start()
    {
        DitributePlaceForSlices();
        waypoints = new List<Transform>();
        waypoints.Add(SushiDeliverWaypoints.GetChild(0));
        waypoints.Add(SushiDeliverWaypoints.GetChild(1));

        for (int i = 0; i < SliceIcons.Length; i++)
        {
            SliceIcons[i].transform.position = SlicesPos[i];
        }
    }
    public void Initialize(SliceType[] preferredSliceTypes ,SliceData data)
    {
        isDelivered = false;
        isFull = false;
        m_sliceCount = 0;
        PreferredSliceTypes = new List<SliceType>();
        for (int i = 0; i < TOTAL_SLICES; i++)
        {
            PreferredSliceTypes.Add( preferredSliceTypes[i]);
        }
        for (int i = 0; i < SliceIcons.Length; i++)
        {
            SliceIcons[i].sprite = data.GetSprite(PreferredSliceTypes[i]);
        }

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
        if(m_sliceCount == TOTAL_SLICES)
        {
            isFull = true;
        }
        return res;
    }
    public void SetSlice(SliceType slice)
    {
        CurSliceTypes.Add(slice);
        if(CurSliceTypes.Count == TOTAL_SLICES)
        {
            DeliverSushi();
        }
    }
    void DitributePlaceForSlices()
    {
        SlicesPos = new Vector3[TOTAL_SLICES];
        for (int i = 0; i < TOTAL_SLICES; i++)
        {
            SlicesPos[i] = new Vector3(transform.position.x,transform.position.y + i * SlicePadding+PlaceOffset, 0);
        }
    }

    //Return a  SushiResultModel object, containing result of the combination of sushi
    //Do some effect after checking
    public SushiResultModel CheckSushiResult(List<SliceType> lastSliceTypes, List<SliceType> orderedSliceTypes)
    {
        int correctSlice = 0;
        int correctOrder = 0;
        int finalScore = 0;
        for (int i = 0; i < orderedSliceTypes.Count; i++)
        {
            if(orderedSliceTypes[i] == lastSliceTypes[i])
            {
                ++correctOrder;
            }
        }
        for (int i = 0; i < orderedSliceTypes.Count; i++)
        {
            if(lastSliceTypes.Contains(orderedSliceTypes[i]))
            {
                ++correctSlice;
                lastSliceTypes.Remove(PreferredSliceTypes[i]);
            }
        }
        finalScore = correctOrder * 1000 + correctSlice * 100;

        
        SushiResultModel res = new SushiResultModel(correctSlice, correctOrder, finalScore);
        res.print();

        if(correctOrder == SushiController.TOTAL_SLICES)
        {
            GameManager.Instance.IncreasePatienceBar(RightOrderIncrease);
        }
        else
        {
            GameManager.Instance.IncreasePatienceBar(WrongOrderIncrease);

        }

        /*        GameManager.Instance.IncreaseFullBar(res);
        */
        GameManager.Instance.CheckSushiResult(res);
        return res;
    }
    //Customer eats the sushi
    public void ConsumeSushi(GameObject slice)
    {
        Destroy(slice);
    }
    public void DeliverSushi()
    {
        List<SliceType> lastSliceType = new List<SliceType>();
        lastSliceType.AddRange(CurSliceTypes);
        CurSliceTypes.Clear();
        //record ordered slicetype for checking, otherwise will check with new order
        List<SliceType> orderedSliceType = new List<SliceType>();
        orderedSliceType.AddRange(PreferredSliceTypes);
        StartCoroutine(StartDeliver(lastSliceType, orderedSliceType));
        isDelivered = true;
    }
    IEnumerator StartDeliver(List<SliceType> lastSliceTypes, List<SliceType> orderedSliceTypes)
    {
        SliceController[] slices = GetComponentsInChildren<SliceController>();
        Transform lastslice = slices[TOTAL_SLICES - 1].transform;
        for (int i = 0; i < waypoints.Count; i++)
        {
            Vector3 target = waypoints[i].position;
            while (Vector3.Distance(lastslice.position, target) > 0.1f)
            {
                foreach (var item in slices)
                {
                    item.transform.Translate(Vector3.Normalize(target-lastslice.position) * Time.deltaTime * SushiMoveSpeed);
                }
                yield return new WaitForEndOfFrame();
            }      
        }
        foreach (var item in slices)
        {
            ConsumeSushi(item.gameObject);
        }
        CheckSushiResult(lastSliceTypes, orderedSliceTypes);
        
    }
}
