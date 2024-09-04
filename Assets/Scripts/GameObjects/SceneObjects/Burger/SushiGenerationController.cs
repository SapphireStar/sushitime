using MyPackage;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SushiGenerationController : MonoBehaviour
{
    [InspectorName("Used to decide the generation of new slices")]
    public int TypesOfSlicesPerLayer = 3;
    public int TotalTypesOfSlices = 3;
    public float SliceRefreshTime = 5;
    private float m_curRefreshTime;

    public SushiController[] Sushis;
    public SliceData CurSliceData;

    [InspectorName("Layers that can refresh slices")]
    public Transform[] Layer0;
    public Transform[] Layer1;
    public Transform[] Layer2;
    public List<List<Point>> RemainPlaces;
    public List<List<Point>> OriginPlaces;
    private LinkedList<Tuple<SliceType,int>> requireTypes;
    [InspectorName("Orders of Sushi")]
    public SliceType[] OrderSushi0;
    public SliceType[] OrderSushi1;
    public SliceType[] OrderSushi2;
    //check map has the food on orders, is not, add to requestqueue
    private HashSet<SliceType> m_orderedSlices;

    GridMap m_gridMap;
    GameModel m_gameModel;
    System.Random random = new System.Random();
    private void Awake()
    {


    }
    void Start()
    {
        m_gameModel = ModelManager.Instance.GetModel<GameModel>(typeof(GameModel));
        m_gridMap = GridMap.Instance;
        EventSystem.Instance.Subscribe<GameStartEvent>(typeof(GameStartEvent), prepareSushi);
        EventSystem.Instance.Subscribe<SliceDropEvent>(typeof(SliceDropEvent), handleSliceDropEvent);
        EventSystem.Instance.Subscribe<SliceSetEvent>(typeof(SliceSetEvent), handleSliceSetEvent);
        EventSystem.Instance.Subscribe<SliceFallEvent>(typeof(SliceFallEvent), handleSliceFallEvent);
        m_gameModel.PropertyValueChanged += onGameModelChangedHandler;
        RemainPlaces = new List<List<Point>>();
        OriginPlaces = new List<List<Point>>();
        requireTypes = new LinkedList<Tuple<SliceType, int>>();
        initializeOrderedSlices();
        //Use TotalTypesOfSlices to determine how many arrays needed to store the refresh points for each type of slices
        for (int i = 0; i < TotalTypesOfSlices; i++)
        {
            RemainPlaces.Add(new List<Point>());
            switch (i)
            {
                case 0:
                    foreach (var item in Layer0)
                    {
                        RemainPlaces[i].Add(m_gridMap.GetPointViaPosition(item.position));
                    }
                    break;
                case 1:
                    foreach (var item in Layer1)
                    {
                        RemainPlaces[i].Add(m_gridMap.GetPointViaPosition(item.position));
                    }
                    break;
                case 2:
                    foreach (var item in Layer2)
                    {
                        RemainPlaces[i].Add(m_gridMap.GetPointViaPosition(item.position));
                    }
                    break;
                default:
                    break;
            }
        }
        for (int i = 0; i < RemainPlaces.Count; i++)
        {
            OriginPlaces.Add(new List<Point>());
            OriginPlaces[i].AddRange(RemainPlaces[i]);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (m_gameModel.IsPaused)
            return;
        checkMissedSlices();
        checkSushiDelivered();
    }
    private void OnDestroy()
    {
        EventSystem.Instance.Unsubscribe<GameStartEvent>(typeof(GameStartEvent), prepareSushi);
        EventSystem.Instance.Unsubscribe<SliceDropEvent>(typeof(SliceDropEvent), handleSliceDropEvent);
        EventSystem.Instance.Unsubscribe<SliceSetEvent>(typeof(SliceSetEvent), handleSliceSetEvent);
        EventSystem.Instance.Unsubscribe<SliceFallEvent>(typeof(SliceFallEvent), handleSliceFallEvent);
        m_gameModel.PropertyValueChanged -= onGameModelChangedHandler;
    }
    void handleSliceDropEvent(SliceDropEvent e)
    {
        for (int i = 0; i < OriginPlaces.Count; i++)
        {
            if (OriginPlaces[i].Contains(m_gridMap.GetPointViaPosition(e.lastPos)))
            {
                RemainPlaces[i].Add(m_gridMap.GetPointViaPosition(e.lastPos));
            }
        }

        foreach (var item in RemainPlaces)
        {
            if (item.Contains(m_gridMap.GetPointViaPosition(e.nowPos)))
            {
                item.Remove(m_gridMap.GetPointViaPosition(e.nowPos));
            }
        }
    }
    void handleSliceSetEvent(SliceSetEvent e)
    {
        if (!e.isHitByOther)
        {
            for (int i = 0; i < OriginPlaces.Count; i++)
            {
                if (OriginPlaces[i].Contains(m_gridMap.GetPointViaPosition(e.pos)))
                {
                    RemainPlaces[i].Add(m_gridMap.GetPointViaPosition(e.pos));
                }
            }
        }

    }
    //BUG: if a slice falls down because of another slice, then it will override its lastpos
    //as a RemainPlace, but in fact it was placed by the another slice
    void handleSliceFallEvent(SliceFallEvent e)
    {
        if (!e.IsHitByOther)
        {
            for (int i = 0; i < OriginPlaces.Count; i++)
            {
                if (OriginPlaces[i].Contains(m_gridMap.GetPointViaPosition(e.LastPos)))
                {
                    RemainPlaces[i].Add(m_gridMap.GetPointViaPosition(e.LastPos));
                }
            }
        }

        foreach (var item in RemainPlaces)
        {
            if (item.Contains(m_gridMap.GetPointViaPosition(e.CurPos)))
            {
                item.Remove(m_gridMap.GetPointViaPosition(e.CurPos));
            }
            
        }
    }
    void onGameModelChangedHandler(object sender, PropertyValueChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case "TotalSlices":
                if (m_gameModel.TotalSlices < 5)
                {
                    SliceRefreshTime = 5;
                }
                if(m_gameModel.TotalSlices >= 9)
                {
                    SliceRefreshTime = 10;
                }
                break;
            default:
                break;
        }
    }
    //Initialize the orderedSlices, so that system know what food to generate
    void initializeOrderedSlices()
    {
        m_orderedSlices = new HashSet<SliceType>();
        foreach (var item in OrderSushi0)
        {
            m_orderedSlices.Add(item);
        }
        foreach (var item in OrderSushi1)
        {
            m_orderedSlices.Add(item);
        }
        foreach (var item in OrderSushi2)
        {
            m_orderedSlices.Add(item);
        }
    }
    void prepareSushi(IEventHandler e)
    {
        Array values = Enum.GetValues(typeof(SliceType));
        for (int i = 0; i < Sushis.Length; i++)
        {
            /*            SliceType[] res = new SliceType[SushiController.TOTAL_SLICES];
                        res[0] = SliceType.Rice;
                        for (int j = 1; j < SushiController.TOTAL_SLICES; j++)
                        {
                            SliceType randomSlice = (SliceType)values.GetValue(random.Next(1+(j-1)* TypesOfSlicesPerLayer, 1 + j * TypesOfSlicesPerLayer));
                            res[j] = randomSlice;
                        }*/
            SliceType[] res = new SliceType[SushiController.TOTAL_SLICES];
            switch (i)
            {
                case 0:
                    res = OrderSushi0;
                    break;
                case 1:
                    res = OrderSushi1;
                    break;
                case 2:
                    res = OrderSushi2;
                    break;
            }
            Sushis[i].Initialize(res, CurSliceData);
            prepareSlice(res);
        }
    }
    public void PrepareSushiAt(int index)
    {
        /*        Array values = Enum.GetValues(typeof(SliceType));

                SliceType[] res = new SliceType[SushiController.TOTAL_SLICES];
                res[0] = SliceType.Rice;
                for (int j = 1; j < SushiController.TOTAL_SLICES; j++)
                {
                    int rand = random.Next(1 + (j - 1) * TypesOfSlicesPerLayer, 1 + j * TypesOfSlicesPerLayer);
                    SliceType randomSlice = (SliceType)values.GetValue(rand);
                    res[j] = randomSlice;
                }*/
        SliceType[] res = new SliceType[SushiController.TOTAL_SLICES];
        switch (index)
        {
            case 0:
                res = OrderSushi0;
                break;
            case 1:
                res = OrderSushi1;
                break;
            case 2:
                res = OrderSushi2;
                break;
        }
        Sushis[index].Initialize(res, CurSliceData);
        RequestSliceRefresh(res);
    }
    void prepareSlice(SliceType[] types)
    {
        for (int i = 0; i < types.Length; i++)
        {
            if (RemainPlaces[i].Count <= 0)
            {
                continue;
            }
                
            int index = UnityEngine.Random.Range(0, RemainPlaces[i].Count);
            Vector3 target = m_gridMap.GetPositionViaPoint(RemainPlaces[i][index]);
            RemainPlaces[i].RemoveAt(index);
            GameObject prefab = CurSliceData.GetPrefab(types[i]);
            GameObject slice = Instantiate(prefab, target, Quaternion.identity, transform);
            slice.transform.localScale = Vector3.one * SushiController.SLICE_SCALE;
            //Add to existedTypes, so that controller know which slice need to be refreshed, which is not
        }
    }
    void prepareSlice(SliceType type, int layer)
    {
        if(layer == 0)
        {
            for (int i = 0; i < RemainPlaces.Count; i++)
            {
                if (RemainPlaces[i].Count > 0)
                {
                    int index = UnityEngine.Random.Range(0, RemainPlaces[layer].Count);
                    Vector3 target = m_gridMap.GetPositionViaPoint(RemainPlaces[layer][index]);
                    RemainPlaces[layer].RemoveAt(index);
                    GameObject prefab = CurSliceData.GetPrefab(type);
                    GameObject slice = Instantiate(prefab, target, Quaternion.identity, transform);
                    slice.transform.localScale = Vector3.one * SushiController.SLICE_SCALE;
                    break;
                }
            }
        }
        else
        {
            if (RemainPlaces[layer].Count <= 0)
            {
                requireTypes.AddLast(new Tuple<SliceType, int>(type, layer));
                return;
            }

            int index = UnityEngine.Random.Range(0, RemainPlaces[layer].Count);
            Vector3 target = m_gridMap.GetPositionViaPoint(RemainPlaces[layer][index]);
            RemainPlaces[layer].RemoveAt(index);
            GameObject prefab = CurSliceData.GetPrefab(type);
            GameObject slice = Instantiate(prefab, target, Quaternion.identity, transform);
            slice.transform.localScale = Vector3.one * SushiController.SLICE_SCALE;
            //Add to existedTypes, so that controller know which slice need to be refreshed, which is not

        }
    }
    //Let SushiController to remind SushiGenerationController, which slice need to be refreshed
    //the slice need to be stored in a queue
    public void RequestSliceRefresh(SliceType[] sliceTypes)
    {
        //Because use this method to add required types, so need to be sure the SliceType corresponds to its layer
        for (int i = 0; i < sliceTypes.Length; i++)
        {
            addSlice(sliceTypes[i]); 
        }

    }
    void checkNeededFood()
    {
        int total = 0;
        foreach (var item in RemainPlaces)
        {
            total += item.Count;
        }
        m_gameModel.TotalSlices = total;
        Debug.Log($"total remain places:{total}");
        var slices = transform.GetComponentsInChildren<SliceController>();
        Debug.Log(slices.Length);
        HashSet<SliceType> curSlices = new HashSet<SliceType>();
        foreach (var item in slices)
        {
            curSlices.Add(item.CurSliceType);
        }
        foreach (var item in m_orderedSlices)
        {
            if (!curSlices.Contains(item))
            {
                addSlice(item);
            }
        }
    }
    void checkMissedSlices()
    {
        
        //determine which type of slice is missed, if missed, spawn it to the map.
        m_curRefreshTime -= Time.deltaTime;
        if (m_curRefreshTime <= 0)
        {
            checkNeededFood();
            m_curRefreshTime = SliceRefreshTime;
            if(requireTypes.Count<=0)
            {
                Array values = Enum.GetValues(typeof(SliceType));
                var slicetype = (SliceType)values.GetValue(UnityEngine.Random.Range(0, values.Length));
                prepareSlice(slicetype, CurSliceData.GetLayer(slicetype));

            }
            else
            {
                var slice = requireTypes.First.Value;
                requireTypes.RemoveFirst();
                prepareSlice(slice.Item1, slice.Item2);
            }


/*            for (int i = 0; i < Sushis.Length; i++)
            {
                foreach (var item in Sushis[i].PreferredSliceTypes)
                {
                    var tuple = new Tuple<SliceType, int>(item, CurSliceData.GetLayer(item));
                    if (!requireTypes.Contains(tuple))
                    {
                        requireTypes.AddLast(tuple);
                    }
                }
            }*/
        }
    }
    void addSlice(SliceType type)
    {
        int layer = CurSliceData.GetLayer(type);
        requireTypes.AddFirst(new LinkedListNode<Tuple<SliceType, int>>(new Tuple<SliceType, int>(type, layer)));

/*        if (layer == 0 && requireTypes.First.Value.Item1 != SliceType.Rice)
        {
            requireTypes.AddFirst(new LinkedListNode<Tuple<SliceType, int>>(new Tuple<SliceType, int>(type, layer)));
        }
        else
        {
            requireTypes.AddLast(new LinkedListNode<Tuple<SliceType, int>>(new Tuple<SliceType, int>(type, layer)));

        }*/

    }
    void checkSushiDelivered()
    {
        for (int i = 0; i < Sushis.Length; i++)
        {

            if (Sushis[i].IsDelivered)
            {
                PrepareSushiAt(i);
            }
        }
             
    }


}
