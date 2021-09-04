using UnityEngine.UI;
using UnityEngine;

public class CraftingItem : MonoBehaviour
{
    public CraftingRecipe recipe;
    private World world;
    private Crafting crafting;
    public Image icon;

    public void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        crafting = GameObject.Find("Crafting").GetComponent<Crafting>();

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
