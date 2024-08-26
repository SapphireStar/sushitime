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
    private Queue<Tuple<SliceType,int>> requireTypes;

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

        RemainPlaces = new List<List<Point>>(TotalTypesOfSlices);
        requireTypes = new Queue<Tuple<SliceType, int>>();
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

    }
    void handleSliceDropEvent(SliceDropEvent e)
    {
        int layer = CurSliceData.GetLayer(e.sliceType);
        RemainPlaces[layer].Remove(m_gridMap.GetPointViaPosition(e.lastPos));
        RemainPlaces[layer].Add(m_gridMap.GetPointViaPosition(e.nowPos));
    }
    void handleSliceSetEvent(SliceSetEvent e)
    {
        int layer = CurSliceData.GetLayer(e.sliceType);
        RemainPlaces[layer].Add(m_gridMap.GetPointViaPosition(e.pos));
    }
    void prepareSushi(IEventHandler e)
    {
        Array values = Enum.GetValues(typeof(SliceType));
        for (int i = 0; i < Sushis.Length; i++)
        {
            SliceType[] res = new SliceType[SushiController.TOTAL_SLICES];
            res[0] = SliceType.Rice;
            for (int j = 1; j < SushiController.TOTAL_SLICES; j++)
            {
                SliceType randomSlice = (SliceType)values.GetValue(random.Next(1+(j-1)* TypesOfSlicesPerLayer, 1 + j * TypesOfSlicesPerLayer));
                res[j] = randomSlice;
            }
            Sushis[i].Initialize(res, CurSliceData);
            prepareSlice(res);
        }
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
        if (RemainPlaces[layer].Count <= 0)
        {
            requireTypes.Enqueue(new Tuple<SliceType, int>(type,layer));
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
    //Let SushiController to remind SushiGenerationController, which slice need to be refreshed
    //the slice need to be stored in a queue
    public void RequestSliceRefresh(SliceType[] sliceTypes)
    {
        //Because use this method to add required types, so need to be sure the SliceType corresponds to its layer
        for (int i = 0; i < sliceTypes.Length; i++)
        {
            requireTypes.Enqueue(new Tuple<SliceType, int>(sliceTypes[i], i));
        }

    }
    void checkMissedSlices()
    {
        //determine which type of slice is missed, if missed, spawn it to the map.
        m_curRefreshTime -= Time.deltaTime;
        if (m_curRefreshTime <= 0)
        {
            m_curRefreshTime = SliceRefreshTime;
            if(requireTypes.Count<=0)
            {
                Array values = Enum.GetValues(typeof(SliceType));
                var slicetype = (SliceType)values.GetValue(UnityEngine.Random.Range(0,values.Length));
                prepareSlice(slicetype, CurSliceData.GetLayer(slicetype));
            
            }
            else
            {
                var slice = requireTypes.Dequeue();
                prepareSlice(slice.Item1, slice.Item2);
            }
        }
    }
    public void PrepareSushiAt(int index)
    {
        Array values = Enum.GetValues(typeof(SliceType));

        SliceType[] res = new SliceType[SushiController.TOTAL_SLICES];
        res[0] = SliceType.Rice;
        for (int j = 1; j < SushiController.TOTAL_SLICES; j++)
        {
            int rand = random.Next(1 + (j - 1) * TypesOfSlicesPerLayer, 1 + j * TypesOfSlicesPerLayer);
            SliceType randomSlice = (SliceType)values.GetValue(rand); 
            res[j] = randomSlice;
        }
        Sushis[index].Initialize(res, CurSliceData);
        RequestSliceRefresh(res);
    }
    void checkSushiDelivered()
    {
        for (int i = 0; i < Sushis.Length; i++)
        {
            if (Sushis[i].IsFull)
            {
                PrepareSushiAt(i);
            }
        }
             
    }


}
