using UnityEngine.UI;
using UnityEngine;

public class ToolbarName : MonoBehaviour
{
    void Update()
    {
        GetComponent<Text>().text = World.Instance.blocktypes[GetComponentInParent<Toolbar>().slots[GetComponentInParent<Toolbar>().slotindex].itemSlot.stack.id].blockName;
    }
}
