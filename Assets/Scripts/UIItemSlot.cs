using UnityEngine.UI;
using UnityEngine;

public class UIItemSlot : MonoBehaviour
{
    public bool isLinked = false;
    public ItemSlot itemSlot;
    public Image slotImage;
    public Image slotIcon;
    public Text slotAmount;

    World world;

    void Awake()
    {
        if (gameObject.name != "CursorSlot")
            world = GameObject.Find("World").GetComponent<World>();
        else if (gameObject.name == "CursorSlot" && !GameObject.Find("MainMenu").gameObject.activeSelf)
            world = GameObject.Find("World").GetComponent<World>();
    }

    public bool hasItem
    {
        get
        {
            if (itemSlot == null)
                return false;
            else
                return itemSlot.HasItem;
        }
    }

    public void Link(ItemSlot _itemSlot)
    {
        itemSlot = _itemSlot;
        isLinked = true;
        itemSlot.LinkUiItemSlot(this);
        UpdateSlot();
    }
    public void Unlink()
    {
        itemSlot.UnlinkUiItemSlot();
        itemSlot = null;
        UpdateSlot();
    }
    public void UpdateSlot()
    {
        if (itemSlot != null && itemSlot.HasItem)
        {
            slotIcon.sprite = world.blocktypes[itemSlot.stack.id].icon;    
            slotAmount.text = itemSlot.stack.amount.ToString();
            slotIcon.enabled = true;
            slotAmount.enabled = true;
        }
        else
        {
            Clear();
        }
    }

    public void Clear()
    {
        slotIcon.sprite = Toolbar.transparent;
        itemSlot.stack.id = 0;
    }

    private void OnDestroy()
    {
        if (isLinked)
            itemSlot.UnlinkUiItemSlot();
    }
}
public class ItemSlot
{
    public ItemStack stack = null;
    public UIItemSlot uiItemSlot = null;

    public ItemSlot (UIItemSlot _uiItemSlot)
    {
        stack = null;
        uiItemSlot = _uiItemSlot;
        uiItemSlot.Link(this);
    }
    public ItemSlot (UIItemSlot _slot, ItemStack _stack)
    {
        stack = _stack;
        uiItemSlot = _slot;
        uiItemSlot.Link(this);
    }

    public void LinkUiItemSlot(UIItemSlot uiSlot)
    {
        uiItemSlot = uiSlot;
    }
    public void UnlinkUiItemSlot()
    {
        uiItemSlot = null;
    }
    public void EmptySlot()
    {
        stack = null;
        if (uiItemSlot != null)
            uiItemSlot.UpdateSlot();
    }

    public int Take(int amt)
    {
        if (amt > stack.amount)
        {
            int _amt = stack.amount;
            return _amt;
        }
        else if (amt < stack.amount)
        {
            stack.amount -= amt;
            uiItemSlot.UpdateSlot();
            return amt;
        }
        else
        {
            stack.amount = 0;
            stack.id = 0;
            uiItemSlot.UpdateSlot();
            return amt;
        }
    }
    public int Add(int amt)
    {
        stack.amount += amt;
        uiItemSlot.UpdateSlot();
        return amt;
    }
    public ItemStack TakeAll()
    {
        ItemStack handOver = new ItemStack(stack.id, stack.amount);
        return handOver;
    }

    public void ChangeBlock(byte blockID)
    {
        stack.id = blockID;
    }

    public bool HasItem
    {
        get
        {
            if (stack != null)
                return true;
            else
                return false;
        }
    }
}
