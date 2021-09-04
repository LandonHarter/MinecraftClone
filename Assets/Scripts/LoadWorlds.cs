using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadWorlds : MonoBehaviour
{
    public GameObject world;
    public GameObject worldUIPrefab;
    public GameObject scrollArea;
    public GameObject generatingWorldPanel;
    private List<GameObject> worldGO = new List<GameObject>();
    public GameObject mainmenu;
    public GameObject mainMenuCam;

    void Start()
    {
        for (int i = 0; i < SaveSystem.AmountOfWorlds(); i++)
        {
            GameObject world = Instantiate(worldUIPrefab);
            world.transform.SetParent(scrollArea.transform);
            worldGO.Add(world);
        }

        int index = 0;

        for (int i = 0; i < worldGO.Count; i++)
        {
            worldGO[i].transform.localPosition = new Vector3(0, 397 - index, 0);
            worldGO[i].transform.GetChild(0).GetComponent<Text>().text = SaveSystem.WorldNames()[i];
            index += 110;
        }
    }

    public void Back()
    {
        gameObject.SetActive(false);
    }
}
