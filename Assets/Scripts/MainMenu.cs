using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using System;

public class MainMenu : MonoBehaviour
{
    public GameObject world;
    public GameObject player;
    public GameObject crafting;
    public GameObject toolbar;
    public GameObject crosshair;
    public GameObject mainMenuCam;
    public GameObject generatingWait;
    public Toggle negative;
    public GameObject settingsMenu;
    public GameObject biomeMenu;
    public GameObject loadWorldMenu;
    public InputField worldNameInput;

    public InputField seedInput;

    public int seed;

    public bool newWorld = false;

    void Start()
    {
        seedInput.characterValidation = InputField.CharacterValidation.Integer;
    }

    void Update()
    {
        if (world.transform.childCount >= VoxelData.ViewDistanceInChunks * VoxelData.ViewDistanceInChunks)
        {
            if (world.GetComponent<World>().chunksToCreate.Count == 0)
            {
                if (world.GetComponent<World>().chunksToDraw.Count == 0)
                {
                    mainMenuCam.SetActive(false);
                    gameObject.SetActive(false);
                }
            }     
        }
    }

    public void ChangeSeed()
    {
        World.seed = int.Parse(seedInput.text);
    }

    public void _GenerateWorld()
    {
        generatingWait.SetActive(true);
        GenerateWorld();
    }
    void GenerateWorld()
    {
        if (seedInput.text == "" || seedInput.text == null)
            if (negative.isOn)
                seed = Mathf.RoundToInt(UnityEngine.Random.Range(-999999, 0));
            else
                seed = Mathf.RoundToInt(UnityEngine.Random.Range(0, 999999));
        if (negative.isOn)
            seed *= -1;

        newWorld = true;
        world.SetActive(true);
        player.SetActive(true);
        crafting.SetActive(true);
        toolbar.SetActive(true);
        crosshair.SetActive(true);
    }

    public void OpenSettingsMenu()
    {
        settingsMenu.SetActive(true);
    }

    public void OpenBiomeMenu()
    {
        biomeMenu.SetActive(true);
    }

    public void OpenLoadMenu()
    {
        loadWorldMenu.SetActive(true);
    }
}
