using UnityEngine.UI;
using UnityEngine;

public class WorldLoader : MonoBehaviour
{
    private GameObject world;

    public void LoadWorld()
    {
        world = transform.parent.parent.parent.parent.GetComponent<LoadWorlds>().world;
        world.SetActive(true);
        for (int i = 0; i < World.Instance.playerParts.Length; i++)
            World.Instance.playerParts[i].SetActive(true);
        world.GetComponent<World>().worldData = SaveSystem.LoadWorld(transform.GetChild(0).GetComponent<Text>().text);
        transform.parent.parent.parent.parent.GetComponent<LoadWorlds>().Back();
    }
}
