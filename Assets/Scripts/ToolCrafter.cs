using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolCrafter : MonoBehaviour
{
    public Toolbar toolbar;
    public GameObject craftingPanel;
    public GameObject craftingItemPrefab;

    public void Start()
    {
        int index = 0;

        for (int i = 0; i < World.Instance.toolRecipes.Length; i++)
        {
            GameObject item = Instantiate(craftingItemPrefab, craftingPanel.transform);

            int height = (Mathf.FloorToInt(i / 13) * -53) + 147;

            if (index > 12)
            {
                index = 0;
            }

            item.transform.localPosition = new Vector3((index * 53) - 320, height, 0);
            item.GetComponent<ToolCraftingItem>().recipe = World.Instance.toolRecipes[i];

            index++;
        }
    }

    public void MakeItem(ToolRecipes recipe)
    {
        bool hasStick = false;
        bool hasMaterial = false;
        int stickIndex = 0;
        int materialIndex = 0;

        int material = 7;
        if (recipe.materialName == "Wood")
            material = 7;
        if (recipe.materialName == "Stone")
            material = 9;

        for (int i = 0; i < 9; i++)
        {
            if (toolbar.slots[i].itemSlot.stack.id == 26 && toolbar.slots[i].itemSlot.stack.amount >= recipe.stickAmount)
            {
                hasStick = true;
                stickIndex = i;
            }
            if (toolbar.slots[i].itemSlot.stack.id == material && toolbar.slots[i].itemSlot.stack.amount >= recipe.materialAmount)
            {
                hasMaterial = true;
                materialIndex = i;
            }
        }


        for (int i = 0; i < 9; i++)
        {
            if (!hasStick || !hasMaterial)
                return;

            if (toolbar.slots[i].itemSlot.stack.id == 0)
            {
                toolbar.slots[stickIndex].itemSlot.Take(recipe.stickAmount);
                toolbar.slots[materialIndex].itemSlot.Take(recipe.materialAmount);

                toolbar.slots[i].itemSlot.stack.id = (byte)recipe.outputItem;
                toolbar.slots[i].itemSlot.Add(1);
                toolbar.slots[i].UpdateSlot();
                return;
            }
        }
    }
}

[System.Serializable]
public class ToolRecipes
{
    public string toolName;
    public string materialName;
    public int stickAmount;
    public int materialAmount;
    public int outputItem;
}
