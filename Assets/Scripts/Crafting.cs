using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crafting : MonoBehaviour
{
    public Toolbar toolbar;
    public GameObject craftingPanel;
    public GameObject craftingItemPrefab;

    public void Start()
    {
        int index = 0;

        for (int i = 0; i < World.Instance.recipes.Length; i++)
        {
            GameObject item = Instantiate(craftingItemPrefab, craftingPanel.transform);

            int height = (Mathf.FloorToInt(i / 13) * -53) + 147;

            if (index > 12)
            {
                index = 0;
            }

            item.transform.localPosition = new Vector3((index * 53) - 320, height, 0);
            item.GetComponent<CraftingItem>().recipe = World.Instance.recipes[i];

            index++;
        }
    }

    public void MakeItem(CraftingRecipe recipe)
    {
        bool crafted = false;

        for (int i = 0; i < 9; i++)
        {
            if (toolbar.slots[i].itemSlot.stack.id == recipe.requiredItem)
            {
                if (toolbar.slots[i].itemSlot.stack.amount >= recipe.amountOfItem)
                {
                    toolbar.slots[i].itemSlot.Take(recipe.amountOfItem);
                    toolbar.slots[i].UpdateSlot();
                    crafted = true;
                }
            }
        }

        if (crafted)
        {
            for (int i = 0; i < 9; i++)
            {
                if (toolbar.slots[i].itemSlot.stack.id == recipe.outputItem)
                {
                    toolbar.slots[i].itemSlot.Add(recipe.amountOfOutput);
                    toolbar.slots[i].UpdateSlot();
                    return;
                }
            }
            for (int i = 0; i < 9; i++)
            {
                if (toolbar.slots[i].itemSlot.stack.id == 0)
                {
                    toolbar.slots[i].itemSlot.stack.id = recipe.outputItem;
                    toolbar.slots[i].itemSlot.Add(recipe.amountOfOutput);
                    toolbar.slots[i].UpdateSlot();
                    return;
                }
            }
        }
    }
}

[System.Serializable]
public class CraftingRecipe
{
    public string outputItemName;
    public int requiredItem;
    public int amountOfItem;
    public byte outputItem;
    public int amountOfOutput;
}
