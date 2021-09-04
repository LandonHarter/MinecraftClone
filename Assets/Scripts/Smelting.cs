using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smelting : MonoBehaviour
{
    public Toolbar toolbar;

    public void Smelt(SmeltableObjects smeltableObject)
    {
        for (int i = 0; i < World.Instance.smeltableObjects.Length; i++)
        {
            if (toolbar.slots[toolbar.slotindex].itemSlot.stack.id == World.Instance.smeltableObjects[i].block)
                toolbar.slots[toolbar.slotindex].itemSlot.Take(1);
        }

        for (int i = 0; i < 9; i++)
        {
            if (toolbar.slots[i].itemSlot.stack.id == smeltableObject.outputBlock)
            {
                toolbar.slots[i].itemSlot.Add(1);
                toolbar.slots[i].UpdateSlot();
                return;
            }
        }
        for (int i = 0; i < 9; i++)
        {
            if (toolbar.slots[i].itemSlot.stack.id == 0)
            {
                toolbar.slots[i].itemSlot.stack.id = (byte)smeltableObject.outputBlock;
                toolbar.slots[i].itemSlot.Add(1);
                toolbar.slots[i].UpdateSlot();
                return;
            }
        }
    }
}

[System.Serializable]
public class SmeltableObjects
{
    public string outputBlockName;
    public int block;
    public int outputBlock;
}
