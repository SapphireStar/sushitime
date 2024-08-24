using MyPackage;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SushiGenerationController : MonoBehaviour
{
    [InspectorName("Used to decide the generation of new slices")]
    public int TypesOfSlicesPerLayer = 3;

    public SushiController[] Sushis;
    public SliceData SliceIconData;

    GridMap m_gridMap;
    GameModel m_gameModel;
    System.Random random = new System.Random();
    void Start()
    {
        m_gameModel = ModelManager.Instance.GetModel<GameModel>(typeof(GameModel));
        m_gridMap = GridMap.Instance;
        EventSystem.Instance.Subscribe<GameStartEvent>(typeof(GameStartEvent), prepareSushi);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_gameModel.IsPaused)
            return;
        checkSushiDelivered();
    }
    private void OnDestroy()
    {
        EventSystem.Instance.Unsubscribe<GameStartEvent>(typeof(GameStartEvent), prepareSushi);

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
            Sushis[i].Initialize(res, SliceIconData);
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
        Sushis[index].Initialize(res, SliceIconData);
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

    public void CheckSliceMissed()
    {
        //determine which type of slice is missed, if missed, spawn it to the map.
    }
}
