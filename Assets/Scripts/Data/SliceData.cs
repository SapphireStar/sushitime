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
}
[Serializable]
public class SliceWrapper
{
    public SliceType Type;
    public Sprite icon;
}