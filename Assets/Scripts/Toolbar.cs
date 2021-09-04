using UnityEngine.UI;
using UnityEngine;

public class Toolbar : MonoBehaviour
{
    public static Sprite transparent;
    public Sprite transparentImage;
    public UIItemSlot[] slots;
    public Player player;
    public RectTransform highlight;
    public int slotindex = 0;

    private void Start()
    {
        transparent = transparentImage;
        int count = 0;

        foreach (UIItemSlot _slot in slots)
        {
            if (World.Instance.mainMenu.newWorld)
            {
                ItemStack stack = new ItemStack(0, 0);
                ItemSlot slot = new ItemSlot(_slot, stack);
            }
            if (!World.Instance.mainMenu.newWorld)
            {
                ItemStack stack = new ItemStack((byte)SaveSystem.LoadToolbarID(World.Instance.worldData.worldName)[count], SaveSystem.LoadToolbarAmount(World.Instance.worldData.worldName)[count]);
                ItemSlot slot = new ItemSlot(_slot, stack);
            }

            count++;
        }
    }

    private void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0)
        {
            if (scroll < 0)
                slotindex++;
            else
                slotindex--;

            if (slotindex > slots.Length - 1)
                slotindex = 0;
            if (slotindex < 0)
                slotindex = slots.Length - 1;

            highlight.position = slots[slotindex].slotIcon.transform.position;
        }

        if (Input.GetButtonDown("ScrollLeft"))
        {
            slotindex--;

            if (slotindex > slots.Length - 1)
                slotindex = 0;
            if (slotindex < 0)
                slotindex = slots.Length - 1;

            highlight.position = slots[slotindex].slotIcon.transform.position;
        }

        if (Input.GetButtonDown("ScrollRight"))
        {
            slotindex++;

            if (slotindex > slots.Length - 1)
                slotindex = 0;
            if (slotindex < 0)
                slotindex = slots.Length - 1;

            highlight.position = slots[slotindex].slotIcon.transform.position;
        }
    }
}