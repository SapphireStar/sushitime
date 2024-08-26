using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="NewSliceData",menuName ="Data/NewSliceData")]
public class SliceData : ScriptableObject
{
    public SliceWrapper[] Slices;
    public Sprite GetSprite(SliceType type)
    {
        foreach (var item in Slices)
        {
            if (item.Type == type)
                return item.icon;
        }
        return null;
    }
    public GameObject GetPrefab(SliceType type)
    {
        foreach (var item in Slices)
        {
            if (item.Type == type)
                return item.Prefab;
        }
        return null;
    }
    public int GetLayer(SliceType type)
    {
        foreach (var item in Slices)
        {
            if (item.Type == type)
                return item.Layer;
        }
        return -1;
    }
}
[Serializable]
public class SliceWrapper
{
    public SliceType Type;
    public Sprite icon;
    public GameObject Prefab;
    public int Layer;
}