using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BiomeMaker : MonoBehaviour
{
    public World world;
    public MainMenu mainMenu;
    public TMP_InputField scale;
    public TMP_InputField height;
    public TMP_Dropdown surfaceBlock;

    public void Start()
    {
        scale.characterValidation = TMP_InputField.CharacterValidation.Integer;
        height.characterValidation = TMP_InputField.CharacterValidation.Integer;

        List<string> options = new List<string>();
        surfaceBlock.ClearOptions();
        for (int i = 1; i < world.blocktypes.Length; i++)
        {
            options.Add(world.blocktypes[i].blockName);
        }

        surfaceBlock.AddOptions(options);
    }

    public void MakeBiome()
    {
        BiomeAttributes biome = new BiomeAttributes();
        if (!(scale.text == "" || scale.text == null))
            biome.terrainScale = float.Parse(scale.text) / 100;
        if (!(height.text == "" || height.text == null))
            biome.solidGroundHeight = int.Parse(height.text);
        if (!(height.text == "" || height.text == null))
            biome.terrainHeight = int.Parse(height.text);

        biome.placeMajorFlora = false;
        biome.offset = Random.Range(0, 10000);

        biome.surfaceBlock = (byte)((byte)surfaceBlock.value + 1);
        biome.subSurfaceBlock = 5;

        Lode lode = new Lode();
        lode.blockID = 5;
        lode.minHeight = 0;
        lode.maxHeight = 0;
        lode.scale = 0;
        lode.threshold = 0;
        lode.noiseOffset = 0;
        lode.nodeName = "";
        biome.lodes = new Lode[1];
        biome.lodes[0] = lode;


        for (int i = 0; i < world.biomes.Length; i++)
        {
            world.biomes[i] = biome;
        }

        mainMenu._GenerateWorld();
        Back();
    }

    public void Back()
    {
        gameObject.SetActive(false);
    }
}
