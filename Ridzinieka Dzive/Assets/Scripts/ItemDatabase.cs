using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public List<ItemData> items;

    public Sprite GetIcon(string name)
    {
        foreach (var item in items)
            if (item.itemName == name)
                return item.icon;

        return null;
    }
    public int GetHungerValue(string name)
    {
        foreach (var item in items)
            if (item.itemName == name)
                return item.hungerValue;

        return 0;
    }

}