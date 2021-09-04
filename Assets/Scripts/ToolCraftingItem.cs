using UnityEngine;
using UnityEngine.UI;

public class ToolCraftingItem : MonoBehaviour
{
    public ToolRecipes recipe;
    private World world;
    private ToolCrafter crafting;
    public Image icon;

    public void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        crafting = GameObject.Find("Crafting").GetComponent<ToolCrafter>();

        icon.sprite = world.blocktypes[recipe.outputItem].icon;
    }

    public void AdjustPosition(int index)
    {
        transform.position = new Vector3((index * 48) + 30, 147, 0);
    }

    public void CraftItem()
    {
        crafting.MakeItem(recipe);
    }
}
